using HarmonyLib;
using KMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DrywallTileSkins
{
	internal static class SettingsScreen
	{
		private static readonly List<System.Action> Refreshers = new List<System.Action>();
		internal static MultiToggle ToggleTemplate;

		public static void Show()
		{
			GameObject parent = DialogParent();
			if (parent == null || ScreenPrefabs.Instance == null || ScreenPrefabs.Instance.InfoDialogScreen == null)
			{
				Debug.LogWarning("[DrywallTileSkins] cannot open settings: game dialog prefab is missing");
				return;
			}

			InfoDialogScreen dialog = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, parent, true);
			dialog.SetHeader(STRINGS.DRYWALL_TILE_SKINS.SETTINGS_TITLE)
				.AddPlainText(STRINGS.DRYWALL_TILE_SKINS.SETTINGS_HINT)
				.AddOption(STRINGS.DRYWALL_TILE_SKINS.SETTINGS_ALL_ON, _ => SetAll(true))
				.AddOption(STRINGS.DRYWALL_TILE_SKINS.SETTINGS_ALL_OFF, _ => SetAll(false))
				.AddDefaultOK();

			DisableHintRaycast(dialog);
			PopulateRows(dialog);
		}

		private static GameObject DialogParent()
		{
			ModsScreen mods = UnityEngine.Object.FindObjectOfType<ModsScreen>();
			if (mods != null)
			{
				Canvas canvas = mods.GetComponentInParent<Canvas>();
				if (canvas != null)
					return canvas.gameObject;
				return mods.gameObject;
			}
			if (FrontEndManager.Instance != null)
				return FrontEndManager.Instance.gameObject;
			return null;
		}

		private static void DisableHintRaycast(InfoDialogScreen dialog)
		{
			Graphic[] graphics = dialog.GetComponentsInChildren<Graphic>(true);
			for (int i = 0; i < graphics.Length; i++)
			{
				if (graphics[i] is Text || graphics[i] is TextMeshProUGUI || graphics[i] is LocText)
					graphics[i].raycastTarget = false;
			}
		}

		private static void PopulateRows(InfoDialogScreen dialog)
		{
			Refreshers.Clear();
			Transform body = FindBody(dialog);
			InfoScreenPlainText labelTemplate = dialog.GetComponentInChildren<InfoScreenPlainText>(true);
			MultiToggle toggleTemplate = ToggleTemplate ?? FindModsToggle();
			if (body == null || labelTemplate == null || toggleTemplate == null)
			{
				Debug.LogWarning("[DrywallTileSkins] settings checkbox template was not found (body="
					+ (body != null) + ", label=" + (labelTemplate != null) + ", toggle=" + (toggleTemplate != null) + ")");
				return;
			}

			List<ModSettings.GroupEntry> groups = ModSettings.SortedGroups();
			for (int i = 0; i < groups.Count; i++)
				AddRow(body, toggleTemplate, labelTemplate, groups[i]);
		}

		private static void AddRow(Transform body, MultiToggle toggleTemplate, InfoScreenPlainText labelTemplate, ModSettings.GroupEntry group)
		{
			GameObject row = new GameObject("DTS_SettingsRow_" + group.id, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(Image));
			row.transform.SetParent(body, false);

			RectTransform rt = row.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0f, 1f);
			rt.anchorMax = new Vector2(1f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);
			rt.sizeDelta = Vector2.zero;

			HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
			layout.spacing = 10f;
			layout.padding = new RectOffset(4, 4, 2, 2);
			layout.childAlignment = TextAnchor.MiddleLeft;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = true;

			LayoutElement rowSize = row.GetComponent<LayoutElement>();
			rowSize.minHeight = 36f;
			rowSize.preferredHeight = 40f;
			rowSize.flexibleWidth = 1f;
			rowSize.flexibleHeight = 0f;

			Image hit = row.GetComponent<Image>();
			hit.color = new Color(0f, 0f, 0f, 0.01f);
			hit.raycastTarget = true;

			GameObject toggleGo = Util.KInstantiateUI(toggleTemplate.gameObject, row, true);
			toggleGo.name = "DTS_Check_" + group.id;
			toggleGo.SetActive(true);
			LayoutElement toggleSize = toggleGo.GetComponent<LayoutElement>() ?? toggleGo.AddComponent<LayoutElement>();
			toggleSize.minWidth = 28f;
			toggleSize.preferredWidth = 32f;
			toggleSize.minHeight = 28f;
			toggleSize.preferredHeight = 32f;
			toggleSize.flexibleWidth = 0f;
			toggleSize.flexibleHeight = 0f;

			MultiToggle toggle = toggleGo.GetComponent<MultiToggle>() ?? toggleGo.GetComponentInChildren<MultiToggle>(true);
			KButton leftoverButton = toggleGo.GetComponent<KButton>() ?? toggleGo.GetComponentInChildren<KButton>(true);
			if (leftoverButton != null)
				leftoverButton.ClearOnClick();

			GameObject labelGo = Util.KInstantiateUI(labelTemplate.gameObject, row, true);
			labelGo.name = "DTS_Label_" + group.id;
			LayoutElement labelSize = labelGo.GetComponent<LayoutElement>() ?? labelGo.AddComponent<LayoutElement>();
			labelSize.minHeight = 32f;
			labelSize.preferredHeight = 36f;
			labelSize.flexibleWidth = 1f;
			labelSize.flexibleHeight = 0f;
			LocText loc = labelGo.GetComponentInChildren<LocText>(true);
			if (loc != null)
			{
				loc.alignment = TextAlignmentOptions.Left;
				loc.raycastTarget = false;
			}

			string id = group.id;
			System.Action refresh = () =>
			{
				bool on = ModSettings.IsGroupEnabled(id);
				if (toggle != null)
					toggle.ChangeState(on ? 1 : 0);
				if (loc != null)
				{
					loc.key = "";
					loc.text = StripLinks(ModSettings.DisplayName(id));
				}
			};
			refresh();

			System.Action clicked = () =>
			{
				ModSettings.SetGroupEnabled(id, !ModSettings.IsGroupEnabled(id));
				refresh();
			};

			if (toggle != null)
			{
				toggle.onClick = clicked;
				toggle.enabled = true;
			}

			SettingsRowClick rowClick = row.AddComponent<SettingsRowClick>();
			rowClick.onClick = clicked;
			Refreshers.Add(refresh);
		}

		internal static MultiToggle FindModsToggle()
		{
			ModsScreen modsScreen = UnityEngine.Object.FindObjectOfType<ModsScreen>();
			if (modsScreen == null)
				return null;

			object displayed = AccessTools.Field(typeof(ModsScreen), "displayedMods")?.GetValue(modsScreen);
			if (!(displayed is IEnumerable rows))
				return modsScreen.GetComponentInChildren<MultiToggle>(true);

			foreach (object row in rows)
			{
				if (row == null)
					continue;
				RectTransform rt = AccessTools.Field(row.GetType(), "rect_transform")?.GetValue(row) as RectTransform;
				if (rt == null)
					continue;
				MultiToggle toggle = ToggleFromRow(rt);
				if (toggle != null)
					return toggle;
			}
			return modsScreen.GetComponentInChildren<MultiToggle>(true);
		}

		internal static MultiToggle ToggleFromRow(RectTransform row)
		{
			if (row == null)
				return null;
			HierarchyReferences refs = row.GetComponent<HierarchyReferences>();
			if (refs != null)
			{
				try
				{
					MultiToggle named = refs.GetReference<MultiToggle>("EnabledToggle");
					if (named != null)
						return named;
				}
				catch
				{
				}
			}
			return row.GetComponentInChildren<MultiToggle>(true);
		}

		private static Transform FindBody(InfoDialogScreen dialog)
		{
			InfoScreenPlainText plain = dialog.GetComponentInChildren<InfoScreenPlainText>(true);
			if (plain != null && plain.transform.parent != null)
				return plain.transform.parent;
			return dialog.transform;
		}

		private static void SetAll(bool enabled)
		{
			List<ModSettings.GroupEntry> groups = ModSettings.SortedGroups();
			for (int i = 0; i < groups.Count; i++)
				ModSettings.SetGroupEnabled(groups[i].id, enabled);
			for (int i = 0; i < Refreshers.Count; i++)
				Refreshers[i]();
		}

		private static string StripLinks(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;
			return Regex.Replace(value, "<[^>]+>", "");
		}
	}

	internal sealed class SettingsRowClick : MonoBehaviour, IPointerClickHandler
	{
		public System.Action onClick;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (onClick != null)
				onClick();
		}
	}

	internal static class ModsScreenPatch
	{
		[HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
		public static class ModsScreen_BuildDisplay_Patch
		{
			public static void Postfix(object __instance)
			{
				try
				{
					AddSettingsButton(__instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning("[DrywallTileSkins] ModsScreen button failed: " + ex.Message);
				}
			}

			private static void AddSettingsButton(object modsScreen)
			{
				var mods = Global.Instance.modManager.mods;
				object displayed = AccessTools.Field(typeof(ModsScreen), "displayedMods")?.GetValue(modsScreen);
				if (!(displayed is IEnumerable rows) || mods == null)
					return;

				foreach (object row in rows)
				{
					if (row == null)
						continue;
					object indexObj = AccessTools.Field(row.GetType(), "mod_index")?.GetValue(row);
					if (!(indexObj is int index) || index < 0 || index >= mods.Count)
						continue;
					KMod.Mod mod = mods[index];
					if (mod == null || mod.staticID != "DarrenLee.DrywallTileSkins")
						continue;

					RectTransform rt = AccessTools.Field(row.GetType(), "rect_transform")?.GetValue(row) as RectTransform;
					if (rt == null)
						continue;

					HierarchyReferences refs = rt.GetComponent<HierarchyReferences>();
					if (refs == null)
						return;

					MultiToggle toggle = SettingsScreen.ToggleFromRow(rt);
					if (toggle != null)
						SettingsScreen.ToggleTemplate = toggle;

					KButton manage = refs.GetReference<KButton>("ManageButton");
					if (manage == null)
						return;

					Transform parent = manage.transform.parent;
					string childName = "DTS_SettingsButton";
					Transform existing = parent.Find(childName);
					if (existing != null)
					{
						existing.gameObject.SetActive(true);
						return;
					}

					GameObject buttonGo = Util.KInstantiateUI(manage.gameObject, parent.gameObject, true);
					buttonGo.name = childName;
					buttonGo.transform.SetSiblingIndex(manage.transform.GetSiblingIndex());
					LocText label = buttonGo.GetComponentInChildren<LocText>();
					if (label != null)
						label.text = STRINGS.DRYWALL_TILE_SKINS.SETTINGS_BUTTON;
					KButton button = buttonGo.GetComponent<KButton>();
					if (button != null)
					{
						button.ClearOnClick();
						button.onClick += SettingsScreen.Show;
					}
					return;
				}
			}
		}
	}
}
