using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterInfoCards.Export;

public static class ExportWidgets
{
	private static InfoCardWidgets curICWidgets;
	private static List<InfoCardWidgets> icWidgets = new List<InfoCardWidgets>();

	public static List<InfoCardWidgets> ConsumeWidgets()
	{
		List<InfoCardWidgets> cardWidgets = icWidgets;
		icWidgets = new List<InfoCardWidgets>();
		return cardWidgets;
	}

	[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginDrawing))]
	class OnBeginDrawing
	{
		static void Postfix()
		{
			icWidgets.Clear();
		}
	}

	[HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
	class OnBeginShadowBar
	{
		static void Postfix()
		{
			if (!InterceptHoverDrawer.IsInterceptMode)
			{
				curICWidgets = new InfoCardWidgets();
				icWidgets.Add(curICWidgets);
			}
		}
	}

	[HarmonyPatch]
	class GetWidget_Patch
	{
		static MethodBase TargetMethod()
		{
			System.Type pool = AccessTools.Inner(typeof(HoverTextDrawer), "Pool`1");
			if (pool == null)
				return null;
			return AccessTools.Method(pool.MakeGenericType(typeof(MonoBehaviour)), "Draw");
		}

		static void Postfix(object __result, GameObject ___prefab)
		{
			if (curICWidgets != null && __result != null)
				curICWidgets.AddWidget(__result, ___prefab);
		}
	}
}
