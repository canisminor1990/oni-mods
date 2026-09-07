using BetterInfoCards.Util;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ThermalInfoCards;

namespace BetterInfoCards;

public class HideElementCategory
{
	public static bool OrHideCategories(bool isVacuum)
	{
		return Options.Opts.HideElementCategories || isVacuum;
	}

	public static IEnumerable<CodeInstruction> ChildTranspiler(IEnumerable<CodeInstruction> codes)
	{
		List<CodeInstruction> method = new List<CodeInstruction>(codes);
		MethodInfo getCategory = AccessTools.Method(typeof(Element), nameof(Element.GetMaterialCategoryTag), System.Type.EmptyTypes);
		MethodInfo isVacuum = AccessTools.PropertyGetter(typeof(Element), nameof(Element.IsVacuum));
		MethodInfo orHide = AccessTools.Method(typeof(HideElementCategory), nameof(OrHideCategories));

		int category = TranspilerUtil.FindCallBackwards(method, getCategory, method.Count - 1);
		int vacuum = category >= 0 ? TranspilerUtil.FindCallBackwards(method, isVacuum, category) : -1;
		if (vacuum >= 0 && orHide != null)
		{
			TranspilerUtil.InsertAfter(method, vacuum, new CodeInstruction(OpCodes.Call, orHide));
			Debug.Log(Mod.LogPrefix + "hide-category transpiler applied");
		}
		else
			Debug.LogWarning(Mod.LogPrefix + "hide-category transpiler missed IsVacuum / GetMaterialCategoryTag");
		return method;
	}
}
