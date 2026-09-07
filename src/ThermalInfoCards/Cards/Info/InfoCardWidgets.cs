using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BetterInfoCards;

public class InfoCardWidgets
{
	public List<object> widgets = new List<object>();
	public object shadowBar;
	public object selectBorder;
	public Vector2 offset = new Vector2();

	public float YMax
	{
		get
		{
			RectTransform rect = RectOf(shadowBar);
			return rect != null ? rect.anchoredPosition.y : 0f;
		}
	}

	public float YMin
	{
		get { return YMax - Height; }
	}

	public float Width
	{
		get
		{
			RectTransform rect = RectOf(shadowBar);
			return rect != null ? rect.rect.width : 0f;
		}
	}

	public float Height
	{
		get
		{
			RectTransform rect = RectOf(shadowBar);
			return rect != null ? rect.rect.height : 0f;
		}
	}

	public void AddWidget(object entry, GameObject prefab)
	{
		if (entry == null || HoverTextScreen.Instance == null || HoverTextScreen.Instance.drawer == null)
			return;
		HoverTextDrawer.Skin skin = HoverTextScreen.Instance.drawer.skin;
		if (prefab == skin.shadowBarWidget.gameObject)
			shadowBar = entry;
		else if (prefab == skin.selectBorderWidget.gameObject)
			selectBorder = entry;
		else
			widgets.Add(entry);
	}

	public void Translate(float x)
	{
		Vector2 shift = new Vector2(x, offset.y);
		Shift(shadowBar, shift);
		Shift(selectBorder, shift);
		for (int i = 0; i < widgets.Count; i++)
			Shift(widgets[i], shift);
	}

	public void SetWidth(float width)
	{
		RectTransform bar = RectOf(shadowBar);
		if (bar == null || InterceptHoverDrawer.drawerInstance == null)
			return;

		object bars = BetterInfoCards.Util.GameAccess.GetShadowBars(InterceptHoverDrawer.drawerInstance);
		if (bars == null)
			return;
		object extra = AccessTools.Method(bars.GetType(), "Draw", new[] { typeof(Vector2) })
			?.Invoke(bars, new object[] { bar.anchoredPosition + new Vector2(bar.sizeDelta.x, 0f) });
		RectTransform extraRect = RectOf(extra);
		if (extraRect != null)
			extraRect.sizeDelta = new Vector2(width - bar.sizeDelta.x, bar.sizeDelta.y);

		RectTransform border = RectOf(selectBorder);
		if (border != null)
			border.sizeDelta = new Vector2(width + 2f, border.sizeDelta.y);
	}

	private static void Shift(object entry, Vector2 shift)
	{
		RectTransform rect = RectOf(entry);
		if (rect != null)
			rect.anchoredPosition += shift;
	}

	private static RectTransform RectOf(object entry)
	{
		if (entry == null)
			return null;
		return AccessTools.Field(entry.GetType(), "rect")?.GetValue(entry) as RectTransform;
	}
}
