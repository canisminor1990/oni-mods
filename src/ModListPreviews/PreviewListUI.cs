using HarmonyLib;
using KMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ModListPreviews
{
	internal static class PreviewListUI
	{
		public const string ChildName = "MLP_Preview";
		private const string PlateName = "Plate";

		private static ModsScreen openScreen;
		private static Texture2D placeholder;

		public static bool HasOpenScreen
		{
			get { return openScreen != null; }
		}

		public static void Apply(ModsScreen screen)
		{
			ApplyInternal(screen, true);
		}

		public static void RefreshOpenScreen()
		{
			if (openScreen == null)
				return;
			ApplyInternal(openScreen, false);
		}

		public static void OnScreenClosed()
		{
			openScreen = null;
		}

		private static void ApplyInternal(ModsScreen screen, bool newPass)
		{
			if (screen == null)
				return;
			openScreen = screen;
			PreviewPump.Ensure(screen.gameObject);
			PreviewService.EnsureReady();

			if (newPass)
				PreviewService.BeginRetryPass();

			List<KMod.Mod> mods = Global.Instance?.modManager?.mods;
			IEnumerable rows = ModsScreenRows.GetDisplayedRows(screen);
			if (rows == null || mods == null)
				return;

			List<KMod.Mod> visible = new List<KMod.Mod>();
			foreach (object row in rows)
			{
				if (row == null)
					continue;
				if (!ModsScreenRows.TryReadRow(row, out int index, out RectTransform rt))
					continue;
				if (index < 0 || index >= mods.Count)
					continue;
				KMod.Mod mod = mods[index];
				if (mod == null)
					continue;
				visible.Add(mod);
				if (rt != null)
					EnsureThumb(rt, mod);
			}

			PreviewService.RequestMissingSteam(visible);
			RelayoutRows(screen);
			RelayoutRowsLater(screen);
		}

		public static void RelayoutRowsLater(ModsScreen screen)
		{
			PreviewPump.EnqueueAfterFrames(1, () => RelayoutRows(screen));
			PreviewPump.EnqueueAfterFrames(8, () => RelayoutRows(screen));
		}

		public static void RelayoutRows(ModsScreen screen)
		{
			if (screen == null)
				return;
			IEnumerable rows = ModsScreenRows.GetDisplayedRows(screen);
			if (rows == null)
				return;

			foreach (object row in rows)
			{
				if (row == null)
					continue;
				if (!ModsScreenRows.TryReadRow(row, out _, out RectTransform rt) || rt == null)
					continue;
				HierarchyReferences refs = rt.GetComponent<HierarchyReferences>();
				Transform parent = ContentParent(rt, refs);
				if (parent == null)
					continue;
				Transform thumb = parent.Find(ChildName) ?? rt.Find(ChildName);
				if (thumb == null)
					continue;
				if (thumb.parent != parent)
					thumb.SetParent(parent, false);
				PlaceThumb(thumb, parent, refs);
				ApplyThumbSize(thumb.gameObject);
				LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
			}
		}

		private static void EnsureThumb(RectTransform row, KMod.Mod mod)
		{
			HierarchyReferences refs = row.GetComponent<HierarchyReferences>();
			Transform parent = ContentParent(row, refs);
			if (parent == null)
				parent = row;
			Transform existing = parent.Find(ChildName) ?? row.Find(ChildName);
			GameObject frame;
			RawImage image;
			if (existing != null)
			{
				frame = existing.gameObject;
				if (frame.transform.parent != parent)
					frame.transform.SetParent(parent, false);
				image = frame.GetComponentInChildren<RawImage>(true);
			}
			else
			{
				frame = CreateFrame(parent);
				image = frame.GetComponentInChildren<RawImage>(true);
			}

			PlaceThumb(frame.transform, parent, refs);
			ApplyThumbSize(frame);
			Texture2D tex = PreviewService.Get(mod);
			frame.SetActive(true);
			ApplyTexture(image, tex);
		}

		private static Transform ContentParent(RectTransform row, HierarchyReferences refs)
		{
			Transform toggle = GetRefTransform(refs, "EnabledToggle");
			if (toggle != null && toggle.parent != null)
				return toggle.parent;

			Transform title = GetRefTransform(refs, "Title");
			if (title != null)
			{
				HorizontalLayoutGroup layout = title.GetComponentInParent<HorizontalLayoutGroup>();
				if (layout != null)
					return layout.transform;
			}

			HorizontalLayoutGroup rowLayout = row.GetComponent<HorizontalLayoutGroup>();
			return rowLayout != null ? rowLayout.transform : row;
		}

		private static Transform GetRefTransform(HierarchyReferences refs, string name)
		{
			if (refs == null)
				return null;
			try
			{
				RectTransform rt = refs.GetReference<RectTransform>(name);
				if (rt != null)
					return rt;
			}
			catch
			{
			}
			try
			{
				MultiToggle toggle = refs.GetReference<MultiToggle>(name);
				if (toggle != null)
					return toggle.transform;
			}
			catch
			{
			}
			try
			{
				LocText title = refs.GetReference<LocText>(name);
				if (title != null)
					return title.transform;
			}
			catch
			{
			}
			return null;
		}

		private static GameObject CreateFrame(Transform parent)
		{
			GameObject frame = new GameObject(ChildName, typeof(RectTransform), typeof(LayoutElement), typeof(VerticalLayoutGroup));
			frame.transform.SetParent(parent, false);

			GameObject plate = new GameObject(PlateName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
			plate.transform.SetParent(frame.transform, false);

			Image bg = plate.GetComponent<Image>();
			bg.color = new Color(0.12f, 0.12f, 0.12f, 1f);
			bg.raycastTarget = false;

			GameObject pic = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			pic.transform.SetParent(plate.transform, false);
			RectTransform picRt = pic.GetComponent<RectTransform>();
			picRt.anchorMin = Vector2.zero;
			picRt.anchorMax = Vector2.one;
			picRt.offsetMin = new Vector2(2f, 2f);
			picRt.offsetMax = new Vector2(-2f, -2f);

			RawImage raw = pic.GetComponent<RawImage>();
			raw.color = Color.white;
			raw.raycastTarget = false;

			ApplyThumbSize(frame);
			return frame;
		}

		private static void PlaceThumb(Transform frame, Transform parent, HierarchyReferences refs)
		{
			if (parent == null || frame == null)
				return;

			Transform titleSlot = FindDirectChild(parent, GetRefTransform(refs, "Title"));
			if (titleSlot != null)
			{
				int titleIndex = titleSlot.GetSiblingIndex();
				int frameIndex = frame.GetSiblingIndex();
				if (frameIndex < titleIndex)
					titleIndex--;
				frame.SetSiblingIndex(titleIndex);
				return;
			}

			int insert = FirstContentIndex(parent, frame);
			frame.SetSiblingIndex(insert);
		}

		private static int FirstContentIndex(Transform parent, Transform frame)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child == null || child == frame || child.name == ChildName)
					continue;
				if (IsMpmChrome(child))
					continue;
				return child.GetSiblingIndex() + (IsLeadingHandle(child) ? 1 : 0);
			}
			return 0;
		}

		private static bool IsMpmChrome(Transform child)
		{
			if (child == null)
				return false;
			string name = child.name ?? "";
			return name.IndexOf("PinBtn", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("tagBtn", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("MPM_", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static bool IsLeadingHandle(Transform child)
		{
			if (child == null)
				return false;
			string name = child.name ?? "";
			return name.IndexOf("Drag", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("Reorder", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("Handle", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static Transform FindDirectChild(Transform parent, Transform descendant)
		{
			if (parent == null || descendant == null)
				return null;
			Transform current = descendant;
			while (current.parent != null && current.parent != parent)
				current = current.parent;
			return current.parent == parent ? current : null;
		}

		private static void ApplyThumbSize(GameObject frame)
		{
			if (frame == null)
				return;
			float thumb = ModSettings.ThumbSize;
			EnsurePlate(frame, out RectTransform plateRt, out LayoutElement plateSize);

			LayoutElement cell = frame.GetComponent<LayoutElement>() ?? frame.AddComponent<LayoutElement>();
			cell.minWidth = cell.preferredWidth = thumb;
			cell.minHeight = thumb;
			cell.preferredHeight = thumb;
			cell.flexibleWidth = 0f;
			cell.flexibleHeight = 1f;
			cell.ignoreLayout = false;

			VerticalLayoutGroup vlg = frame.GetComponent<VerticalLayoutGroup>() ?? frame.AddComponent<VerticalLayoutGroup>();
			vlg.padding = new RectOffset(0, 0, 0, 0);
			vlg.spacing = 0f;
			vlg.childAlignment = TextAnchor.MiddleCenter;
			vlg.childControlWidth = true;
			vlg.childControlHeight = true;
			vlg.childForceExpandWidth = false;
			vlg.childForceExpandHeight = false;
			vlg.childScaleWidth = false;
			vlg.childScaleHeight = false;

			if (plateSize != null)
			{
				plateSize.minWidth = plateSize.preferredWidth = thumb;
				plateSize.minHeight = plateSize.preferredHeight = thumb;
				plateSize.flexibleWidth = 0f;
				plateSize.flexibleHeight = 0f;
				plateSize.ignoreLayout = false;
			}
			if (plateRt != null)
			{
				plateRt.pivot = new Vector2(0.5f, 0.5f);
				plateRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, thumb);
				plateRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, thumb);
			}

			Image outer = frame.GetComponent<Image>();
			if (outer != null)
				outer.enabled = false;
		}

		private static void EnsurePlate(GameObject frame, out RectTransform plateRt, out LayoutElement plateSize)
		{
			plateRt = null;
			plateSize = null;
			if (frame == null)
				return;

			Transform plate = frame.transform.Find(PlateName);
			if (plate == null)
			{
				Transform pic = frame.transform.Find("Image");
				if (pic != null)
				{
					GameObject plateGo = new GameObject(PlateName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
					plateGo.transform.SetParent(frame.transform, false);
					plateGo.transform.SetAsFirstSibling();
					Image bg = plateGo.GetComponent<Image>();
					bg.color = new Color(0.12f, 0.12f, 0.12f, 1f);
					bg.raycastTarget = false;
					pic.SetParent(plateGo.transform, false);
					RectTransform picRt = pic as RectTransform;
					if (picRt != null)
					{
						picRt.anchorMin = Vector2.zero;
						picRt.anchorMax = Vector2.one;
						picRt.offsetMin = new Vector2(2f, 2f);
						picRt.offsetMax = new Vector2(-2f, -2f);
					}
					plate = plateGo.transform;
				}
			}

			if (plate == null)
				return;
			plateRt = plate as RectTransform;
			plateSize = plate.GetComponent<LayoutElement>() ?? plate.gameObject.AddComponent<LayoutElement>();
		}

		private static void ApplyTexture(RawImage image, Texture2D tex)
		{
			if (image == null)
				return;
			Texture2D shown = tex != null ? tex : Placeholder();
			image.enabled = true;
			image.color = Color.white;
			image.texture = shown;
			image.uvRect = tex != null ? CoverUv(tex) : new Rect(0f, 0f, 1f, 1f);
		}

		private static Texture2D Placeholder()
		{
			if (placeholder != null)
				return placeholder;

			const int size = 64;
			placeholder = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				name = "MLP_Placeholder",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};

			Color bg = new Color(0.16f, 0.16f, 0.18f, 1f);
			Color mountain = new Color(0.38f, 0.38f, 0.42f, 1f);
			Color sun = new Color(0.62f, 0.62f, 0.66f, 1f);
			Color[] pixels = new Color[size * size];
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					float nx = x / (float)(size - 1);
					float ny = y / (float)(size - 1);
					Color color = bg;
					float dx = nx - 0.72f;
					float dy = ny - 0.74f;
					if (dx * dx + dy * dy < 0.032f)
						color = sun;
					float peakA = 0.20f + 0.48f * (1f - Mathf.Abs(nx - 0.34f) * 2.15f);
					float peakB = 0.10f + 0.40f * (1f - Mathf.Abs(nx - 0.72f) * 2.55f);
					if (ny < peakA || ny < peakB)
						color = mountain;
					pixels[y * size + x] = color;
				}
			}
			placeholder.SetPixels(pixels);
			placeholder.Apply(false, false);
			return placeholder;
		}

		public static Rect CoverUv(Texture tex)
		{
			if (tex == null || tex.height <= 0)
				return new Rect(0f, 0f, 1f, 1f);
			float texAspect = (float)tex.width / tex.height;
			if (texAspect > 1.01f)
			{
				float w = 1f / texAspect;
				return new Rect((1f - w) * 0.5f, 0f, w, 1f);
			}
			if (texAspect < 0.99f)
			{
				float h = texAspect;
				return new Rect(0f, (1f - h) * 0.5f, 1f, h);
			}
			return new Rect(0f, 0f, 1f, 1f);
		}
	}

	internal static class ModsScreenRows
	{
		private static FieldInfo displayedModsField;
		private static FieldInfo modIndexField;
		private static FieldInfo rectTransformField;

		public static IEnumerable GetDisplayedRows(object screen)
		{
			if (screen == null)
				return null;
			if (displayedModsField == null)
				displayedModsField = AccessTools.Field(typeof(ModsScreen), "displayedMods");
			return displayedModsField?.GetValue(screen) as IEnumerable;
		}

		public static bool TryReadRow(object row, out int index, out RectTransform rt)
		{
			index = -1;
			rt = null;
			if (row == null)
				return false;

			Type type = row.GetType();
			if (modIndexField == null || modIndexField.DeclaringType != type)
				modIndexField = AccessTools.Field(type, "mod_index");
			if (rectTransformField == null || rectTransformField.DeclaringType != type)
				rectTransformField = AccessTools.Field(type, "rect_transform");

			object indexObj = modIndexField?.GetValue(row);
			if (!(indexObj is int parsed))
				return false;
			index = parsed;
			rt = rectTransformField?.GetValue(row) as RectTransform;
			return true;
		}
	}
}
