using HarmonyLib;
using KMod;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomChineseFonts
{
	internal static class SettingsScreen
	{
		private const string SettingsButtonName = "CCF_SettingsButton";

		private static LocText englishOffLabel;
		private static LocText englishOnLabel;
		private static LocText titleHarmonyLabel;
		private static LocText titleJinzisheLabel;
		private static LocText titleCustomLabel;

		public static void Show()
		{
			FontLoader.EnsureLoaded();
			GameObject parent = DialogParent();
			if (parent == null || ScreenPrefabs.Instance == null || ScreenPrefabs.Instance.InfoDialogScreen == null)
			{
				Debug.LogWarning(Mod.LogPrefix + "cannot open settings: game dialog prefab is missing");
				return;
			}

			ClearRefs();
			InfoDialogScreen dialog = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, parent, true);
			dialog.SetHeader(STRINGS.CUSTOM_CHINESE_FONTS.SETTINGS_TITLE)
				.AddPlainText(STRINGS.CUSTOM_CHINESE_FONTS.SETTINGS_HINT)
				.AddOption(STRINGS.CUSTOM_CHINESE_FONTS.ENGLISH_OFF, _ => { })
				.AddDefaultOK();

			BuildBody(dialog);
			RefreshDisplay();
		}

		public static void AddModButton(object modsScreen)
		{
			var mods = Global.Instance?.modManager?.mods;
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
				if (mod == null || mod.staticID != Mod.StaticId)
					continue;

				RectTransform rt = AccessTools.Field(row.GetType(), "rect_transform")?.GetValue(row) as RectTransform;
				if (rt == null)
					return;

				HierarchyReferences refs = rt.GetComponent<HierarchyReferences>();
				if (refs == null)
					return;

				KButton manage = refs.GetReference<KButton>("ManageButton");
				if (manage == null)
					return;

				Transform parent = manage.transform.parent;
				Transform existing = parent.Find(SettingsButtonName);
				if (existing != null)
				{
					existing.gameObject.SetActive(true);
					return;
				}

				GameObject buttonGo = Util.KInstantiateUI(manage.gameObject, parent.gameObject, true);
				buttonGo.name = SettingsButtonName;
				buttonGo.transform.SetSiblingIndex(manage.transform.GetSiblingIndex());
				LocText label = buttonGo.GetComponentInChildren<LocText>();
				if (label != null)
					label.text = STRINGS.CUSTOM_CHINESE_FONTS.SETTINGS_BUTTON;
				KButton button = buttonGo.GetComponent<KButton>();
				if (button != null)
				{
					button.ClearOnClick();
					button.onClick += Show;
				}
				return;
			}
		}

		private static void BuildBody(InfoDialogScreen dialog)
		{
			KButton template = FindOptionButton(dialog);
			Transform body = FindBody(dialog);
			InfoScreenPlainText plain = dialog.GetComponentInChildren<InfoScreenPlainText>(true);
			if (template == null || body == null || plain == null)
			{
				Debug.LogWarning(Mod.LogPrefix + "settings dialog layout was not found");
				return;
			}

			template.gameObject.SetActive(false);

			AddHeader(plain, body, STRINGS.CUSTOM_CHINESE_FONTS.SECTION_ENGLISH);
			AddChoiceRow(template, body,
				STRINGS.CUSTOM_CHINESE_FONTS.ENGLISH_OFF, () => SetReplaceEnglish(false),
				STRINGS.CUSTOM_CHINESE_FONTS.ENGLISH_ON, () => SetReplaceEnglish(true),
				out englishOffLabel, out englishOnLabel);

			AddSpacer(body);
			AddHeader(plain, body, STRINGS.CUSTOM_CHINESE_FONTS.SECTION_TITLE);
			AddTitleRow(template, body);
			AddSpacer(body);
			CloneText(plain, body, STRINGS.CUSTOM_CHINESE_FONTS.CUSTOM_FONTS_HINT);
		}

		private static void AddChoiceRow(
			KButton template,
			Transform body,
			string leftText,
			System.Action left,
			string rightText,
			System.Action right,
			out LocText leftLabel,
			out LocText rightLabel)
		{
			Transform row = CreateRow(body, 40f);
			leftLabel = AddFlexButton(template, row, leftText, left);
			rightLabel = AddFlexButton(template, row, rightText, right);
		}

		private static void AddTitleRow(KButton template, Transform body)
		{
			Transform row = CreateRow(body, 40f);
			titleHarmonyLabel = AddFlexButton(template, row, STRINGS.CUSTOM_CHINESE_FONTS.TITLE_HARMONYOS, () => SetTitleFont(ModSettings.TitleHarmonyOS));
			titleJinzisheLabel = AddFlexButton(template, row, STRINGS.CUSTOM_CHINESE_FONTS.TITLE_JINZISHE, () => SetTitleFont(ModSettings.TitleJinzishe));
			if (FontLoader.CustomTitle != null)
				titleCustomLabel = AddFlexButton(template, row, STRINGS.CUSTOM_CHINESE_FONTS.TITLE_CUSTOM, () => SetTitleFont(ModSettings.TitleCustom));
		}

		private static Transform CreateRow(Transform body, float height)
		{
			GameObject row = new GameObject("CCF_SettingsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
			row.transform.SetParent(body, false);
			RectTransform rt = row.GetComponent<RectTransform>();
			rt.anchorMin = new Vector2(0f, 1f);
			rt.anchorMax = new Vector2(1f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);

			HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
			layout.spacing = 8f;
			layout.padding = new RectOffset(0, 0, 2, 2);
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.childControlWidth = true;
			layout.childControlHeight = true;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = true;

			LayoutElement size = row.GetComponent<LayoutElement>();
			size.minHeight = height;
			size.preferredHeight = height;
			size.flexibleWidth = 1f;
			size.flexibleHeight = 0f;
			return row.transform;
		}

		private static void AddHeader(InfoScreenPlainText plain, Transform body, string text)
		{
			LocText loc = CloneText(plain, body, text);
			if (loc == null)
				return;
			loc.fontStyle = FontStyles.Bold;
		}

		private static LocText CloneText(InfoScreenPlainText plain, Transform body, string text)
		{
			GameObject go = Util.KInstantiateUI(plain.gameObject, body.gameObject, true);
			go.name = "CCF_SettingsText";
			LocText loc = go.GetComponentInChildren<LocText>(true);
			SetLoc(loc, text);
			return loc;
		}

		private static LocText AddFlexButton(KButton template, Transform row, string text, System.Action onClick)
		{
			GameObject go = Util.KInstantiateUI(template.gameObject, row.gameObject, true);
			go.name = "CCF_SettingsButton";
			go.SetActive(true);
			LocText loc = go.GetComponentInChildren<LocText>(true);
			SetLoc(loc, text);
			if (loc != null)
				loc.alignment = TextAlignmentOptions.Center;

			LayoutElement size = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
			size.minHeight = 32f;
			size.preferredHeight = 36f;
			size.minWidth = 80f;
			size.flexibleWidth = 1f;
			size.flexibleHeight = 0f;

			KButton button = go.GetComponent<KButton>() ?? go.GetComponentInChildren<KButton>(true);
			if (button != null)
			{
				button.ClearOnClick();
				button.onClick += onClick;
			}
			return loc;
		}

		private static void AddSpacer(Transform body)
		{
			GameObject go = new GameObject("CCF_Spacer", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(body, false);
			LayoutElement size = go.GetComponent<LayoutElement>();
			size.minHeight = 10f;
			size.preferredHeight = 10f;
			size.flexibleWidth = 1f;
		}

		private static void SetReplaceEnglish(bool enabled)
		{
			ModSettings.ReplaceEnglish = enabled;
			RefreshDisplay();
			Patches.Apply();
		}

		private static void SetTitleFont(string id)
		{
			ModSettings.TitleFontId = id;
			RefreshDisplay();
			Patches.Apply();
		}

		private static void RefreshDisplay()
		{
			bool english = ModSettings.ReplaceEnglish;
			string title = ModSettings.TitleFontId;
			SetLoc(englishOffLabel, ChoiceText(STRINGS.CUSTOM_CHINESE_FONTS.ENGLISH_OFF, !english));
			SetLoc(englishOnLabel, ChoiceText(STRINGS.CUSTOM_CHINESE_FONTS.ENGLISH_ON, english));
			SetLoc(titleHarmonyLabel, ChoiceText(STRINGS.CUSTOM_CHINESE_FONTS.TITLE_HARMONYOS, title == ModSettings.TitleHarmonyOS));
			SetLoc(titleJinzisheLabel, ChoiceText(STRINGS.CUSTOM_CHINESE_FONTS.TITLE_JINZISHE, title == ModSettings.TitleJinzishe));
			SetLoc(titleCustomLabel, ChoiceText(STRINGS.CUSTOM_CHINESE_FONTS.TITLE_CUSTOM, title == ModSettings.TitleCustom));
		}

		private static string ChoiceText(string label, bool selected)
		{
			return (selected ? "●  " : "    ") + label;
		}

		private static void SetLoc(LocText loc, string text)
		{
			if (loc == null)
				return;
			loc.key = "";
			loc.text = text;
		}

		private static void ClearRefs()
		{
			englishOffLabel = englishOnLabel = titleHarmonyLabel = titleJinzisheLabel = titleCustomLabel = null;
		}

		private static KButton FindOptionButton(InfoDialogScreen dialog)
		{
			string needle = STRINGS.CUSTOM_CHINESE_FONTS.ENGLISH_OFF;
			KButton[] buttons = dialog.GetComponentsInChildren<KButton>(true);
			for (int i = 0; i < buttons.Length; i++)
			{
				LocText loc = buttons[i].GetComponentInChildren<LocText>(true);
				if (loc != null && !string.IsNullOrEmpty(loc.text) && loc.text.IndexOf(needle) >= 0)
					return buttons[i];
			}
			for (int i = 0; i < buttons.Length; i++)
			{
				LocText loc = buttons[i].GetComponentInChildren<LocText>(true);
				if (loc != null && !string.IsNullOrEmpty(loc.text))
					return buttons[i];
			}
			return null;
		}

		private static Transform FindBody(InfoDialogScreen dialog)
		{
			InfoScreenPlainText plain = dialog.GetComponentInChildren<InfoScreenPlainText>(true);
			if (plain != null && plain.transform.parent != null)
				return plain.transform.parent;
			return dialog.transform;
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
	}
}
