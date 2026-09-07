using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterInfoCards.Util;

internal static class GameAccess
{
	private static readonly FieldInfo CurrentPosField = AccessTools.Field(typeof(HoverTextDrawer), "currentPos");
	private static readonly FieldInfo ShadowBarsField = AccessTools.Field(typeof(HoverTextDrawer), "shadowBars");
	private static readonly FieldInfo OverlayHoverField = AccessTools.Field(typeof(SelectToolHoverTextCard), "overlayValidHoverObjects");

	public static void AddCurrentPosY(HoverTextDrawer drawer, float delta)
	{
		if (drawer == null || CurrentPosField == null)
			return;
		Vector2 pos = (Vector2)CurrentPosField.GetValue(drawer);
		pos.y += delta;
		CurrentPosField.SetValue(drawer, pos);
	}

	public static object GetShadowBars(HoverTextDrawer drawer)
	{
		if (drawer == null || ShadowBarsField == null)
			return null;
		return ShadowBarsField.GetValue(drawer);
	}

	public static List<KSelectable> OverlayHoverObjects(SelectToolHoverTextCard instance)
	{
		if (instance == null || OverlayHoverField == null)
			return null;
		return OverlayHoverField.GetValue(instance) as List<KSelectable>;
	}
}
