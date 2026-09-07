using BetterInfoCards.Util;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ThermalInfoCards;
using UnityEngine;

namespace BetterInfoCards;

[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.DrawIcon), new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) })]
class NoDrawIconShadows
{
	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
	{
		List<CodeInstruction> method = new List<CodeInstruction>(codes);
		MethodInfo addIndent = AccessTools.Method(typeof(HoverTextDrawer), nameof(HoverTextDrawer.AddIndent));
		MethodInfo setSize = AccessTools.PropertySetter(typeof(RectTransform), nameof(RectTransform.sizeDelta));

		int indent = TranspilerUtil.FindCall(method, addIndent, 0);
		if (indent < 0 || setSize == null)
		{
			Debug.LogWarning(Mod.LogPrefix + "icon-shadow transpiler missed AddIndent");
			return method;
		}

		int size = TranspilerUtil.FindCall(method, setSize, indent + 1);
		if (size < 0)
		{
			Debug.LogWarning(Mod.LogPrefix + "icon-shadow transpiler missed sizeDelta");
			return method;
		}

		method.RemoveRange(indent + 1, size - indent);
		Debug.Log(Mod.LogPrefix + "icon-shadow transpiler applied");
		return method;
	}
}
