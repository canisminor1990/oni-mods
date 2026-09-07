using BetterInfoCards.Util;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ThermalInfoCards;
using UnityEngine;

namespace BetterInfoCards;

public static class DetectRunStart_Patch
{
	public static IEnumerable<CodeInstruction> ChildTranspiler(IEnumerable<CodeInstruction> codes)
	{
		List<CodeInstruction> method = new List<CodeInstruction>(codes);
		MethodInfo getChore = AccessTools.Method(typeof(Component), nameof(Component.GetComponent), System.Type.EmptyTypes);
		if (getChore != null && getChore.IsGenericMethodDefinition)
			getChore = getChore.MakeGenericMethod(typeof(ChoreConsumer));
		MethodInfo draw = AccessTools.Method(typeof(DetectRunStart_Patch), nameof(DrawUnreachableCard));

		int chore = TranspilerUtil.FindCall(method, getChore, 0);
		int zero = -1;
		if (chore >= 0)
		{
			for (int i = chore + 1; i < method.Count; i++)
			{
				if (method[i].opcode == OpCodes.Ldc_I4_0)
				{
					zero = i;
					break;
				}
			}
		}

		if (zero >= 0 && draw != null)
		{
			TranspilerUtil.InsertAfter(method, zero,
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Call, draw));
			Debug.Log(Mod.LogPrefix + "unreachable-card transpiler applied");
		}
		else
			Debug.LogWarning(Mod.LogPrefix + "unreachable-card transpiler missed ChoreConsumer / ldc.i4.0");
		return method;
	}

	public static void DrawUnreachableCard(SelectToolHoverTextCard instance)
	{
		if (instance == null || HoverTextScreen.Instance == null || HoverTextScreen.Instance.drawer == null)
			return;

		StatusItem unreachable = Db.Get().MiscStatusItems.PickupableUnreachable;
		List<KSelectable> hovers = GameAccess.OverlayHoverObjects(instance);
		if (unreachable == null || hovers == null || !hovers.Any(item => item != null && item.HasStatusItem(unreachable)))
			return;

		HoverTextDrawer drawer = HoverTextScreen.Instance.drawer;
		drawer.BeginShadowBar();
		drawer.DrawIcon(unreachable.sprite.sprite, instance.Styles_BodyText.Standard.textColor, 18, -6);
		drawer.AddIndent(8);
		drawer.DrawText(unreachable.Name.ToUpper(), instance.Styles_Title.Standard);
		drawer.EndShadowBar();
	}
}
