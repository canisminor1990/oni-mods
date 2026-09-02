using KMod;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModListPreviews
{
	internal static class SettingsScreen
	{
		private const string SettingsButtonName = "MLP_SettingsButton";
		private const string RefreshButtonName = "MLP_RefreshButton";

		private static LocText sizeValue;
		private static LocText statusLine;
		private static KButton sizeMinus;
		private static KButton sizePlus;

		public static void Show()
		{
			GameObject parent = DialogParent();
			if (parent == null || ScreenPrefabs.Instance == null || ScreenPrefabs.Instance.InfoDialogScreen == null)
			{
				Debug.LogWarning(Mod.LogPrefix + "cannot open settings: game dialog prefab is missing");
				return;
			}

			ClearRefs();

			InfoDialogScreen dialog = Util.KInstantiateUI<InfoDialogScreen>(ScreenPrefabs.Instance.InfoDialogScreen.gameObject, parent, true);
			dialog.SetHeader(STRINGS.UI.MODLISTPREVIEWS.SETTINGS_TITLE)
				.AddPlainText(STRINGS.UI.MODLISTPREVIEWS.SETTINGS_HINT)
				.AddOption(STRINGS.UI.MODLISTPREVIEWS.MINUS, _ => { })
				.AddDefaultOK();

			BuildBody(dialog);
			RefreshDisplay();
		}

		public static void AddModButtons(object modsScreen)
		{
			var mods = Global.Instance?.modManager?.mods;
			IEnumerable rows = ModsScreenRows.GetDisplayedRows(modsScreen);
			if (rows == null || mods == null)
				return;

			foreach (object row in rows)
			{
				if (row == null)
					continue;
				if (!ModsScreenRows.TryReadRow(row, out int index, out RectTransform rt))
					continue;
				if (index < 0 || index >= mods.Count)
					continue;
				KMod.Mod mod = mods[index];
				if (mod == null || mod.staticID != Mod.StaticId)
					continue;

				if (rt == null)
					return;

				HierarchyReferences refs = rt.GetComponent<HierarchyReferences>();
				if (refs == null)
					return;

				KButton manage = refs.GetReference<KButton>("ManageButton");
				if (manage == null)
					return;

				Transform buttonParent = manage.transform.parent;
				HideChild(buttonParent, RefreshButtonName);
				AddRowButton(buttonParent, manage, SettingsButtonName, STRINGS.UI.MODLISTPREVIEWS.SETTINGS_BUTTON, Show);
				return;
			}
		}

		public static void RefreshCovers()
		{
			PreviewService.RefreshAll();
			SetLoc(statusLine, STRINGS.UI.MODLISTPREVIEWS.REFRESH_STARTED);
			PreviewListUI.RefreshOpenScreen();
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

			AddStepperRow(template, plain, body,
				() => string.Format(STRINGS.UI.MODLISTPREVIEWS.SIZE_VALUE, ModSettings.ThumbSize),
				() => ChangeSize(-ModSettings.SizeStep),
				() => ChangeSize(ModSettings.SizeStep),
				out sizeValue, out sizeMinus, out sizePlus);

			AddSpacer(body);
			AddNote(plain, body, STRINGS.UI.MODLISTPREVIEWS.REFRESH_HINT);
			AddFullButton(template, body, STRINGS.UI.MODLISTPREVIEWS.REFRESH_COVERS, RefreshCovers);
			statusLine = AddNote(plain, body, " ");
		}

		private static void AddStepperRow(
			KButton template,
			InfoScreenPlainText plain,
			Transform body,
			Func<string> text,
			System.Action minus,
			System.Action plus,
			out LocText value,
			out KButton minusButton,
			out KButton plusButton)
		{
			Transform row = CreateRow(body, 40f);
			minusButton = AddStepButton(template, row, STRINGS.UI.MODLISTPREVIEWS.MINUS, minus);
			value = AddValueLabel(plain, row, text());
			plusButton = AddStepButton(template, row, STRINGS.UI.MODLISTPREVIEWS.PLUS, plus);
		}

		private static Transform CreateRow(Transform body, float height)
		{
			GameObject row = new GameObject("MLP_SettingsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
			row.transform.SetParent(body, false);
			Stretch(row.GetComponent<RectTransform>());

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

		private static LocText AddNote(InfoScreenPlainText plain, Transform body, string text)
		{
			return CloneText(plain, body, text);
		}

		private static LocText CloneText(InfoScreenPlainText plain, Transform body, string text)
		{
			GameObject go = Util.KInstantiateUI(plain.gameObject, body.gameObject, true);
			go.name = "MLP_SettingsText";
			LocText loc = go.GetComponentInChildren<LocText>(true);
			SetLoc(loc, text);
			return loc;
		}

		private static KButton AddStepButton(KButton template, Transform row, string text, System.Action onClick)
		{
			GameObject go = CloneButton(template, row, text, onClick, 0f, 48f, 56f);
			return go.GetComponent<KButton>() ?? go.GetComponentInChildren<KButton>(true);
		}

		private static void AddFullButton(KButton template, Transform body, string text, System.Action onClick)
		{
			CloneButton(template, body, text, onClick, 1f, 0f, 120f);
		}

		private static GameObject CloneButton(KButton template, Transform parent, string text, System.Action onClick, float flex, float minWidth, float preferredWidth)
		{
			GameObject go = Util.KInstantiateUI(template.gameObject, parent.gameObject, true);
			go.name = "MLP_SettingsButton";
			go.SetActive(true);
			LocText loc = go.GetComponentInChildren<LocText>(true);
			SetLoc(loc, text);
			if (loc != null)
				loc.alignment = TextAlignmentOptions.Center;

			LayoutElement size = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
			size.minHeight = 32f;
			size.preferredHeight = 36f;
			size.minWidth = minWidth;
			size.preferredWidth = preferredWidth;
			size.flexibleWidth = flex;
			size.flexibleHeight = 0f;

			KButton button = go.GetComponent<KButton>() ?? go.GetComponentInChildren<KButton>(true);
			if (button != null)
			{
				button.ClearOnClick();
				button.onClick += onClick;
			}
			return go;
		}

		private static LocText AddValueLabel(InfoScreenPlainText plain, Transform row, string text)
		{
			GameObject go = Util.KInstantiateUI(plain.gameObject, row.gameObject, true);
			go.name = "MLP_SettingsValue";
			LayoutElement size = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
			size.minHeight = 32f;
			size.preferredHeight = 36f;
			size.minWidth = 96f;
			size.flexibleWidth = 1f;
			size.flexibleHeight = 0f;
			LocText loc = go.GetComponentInChildren<LocText>(true);
			SetLoc(loc, text);
			if (loc != null)
				loc.alignment = TextAlignmentOptions.Center;
			return loc;
		}

		private static void AddSpacer(Transform body)
		{
			GameObject go = new GameObject("MLP_Spacer", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(body, false);
			LayoutElement size = go.GetComponent<LayoutElement>();
			size.minHeight = 10f;
			size.preferredHeight = 10f;
			size.flexibleWidth = 1f;
		}

		private static void Stretch(RectTransform rt)
		{
			if (rt == null)
				return;
			rt.anchorMin = new Vector2(0f, 1f);
			rt.anchorMax = new Vector2(1f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);
			rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y);
		}

		private static void ChangeSize(int delta)
		{
			ModSettings.ChangeSize(delta);
			RefreshDisplay();
			PreviewListUI.RefreshOpenScreen();
		}

		private static void RefreshDisplay()
		{
			SetLoc(sizeValue, string.Format(STRINGS.UI.MODLISTPREVIEWS.SIZE_VALUE, ModSettings.ThumbSize));
			SetInteractable(sizeMinus, ModSettings.ThumbSize > ModSettings.MinThumbSize);
			SetInteractable(sizePlus, ModSettings.ThumbSize < ModSettings.MaxThumbSize);
		}

		private static void SetInteractable(KButton button, bool enabled)
		{
			if (button != null)
				button.isInteractable = enabled;
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
			sizeValue = null;
			statusLine = null;
			sizeMinus = sizePlus = null;
		}

		private static KButton FindOptionButton(InfoDialogScreen dialog)
		{
			string needle = STRINGS.UI.MODLISTPREVIEWS.MINUS;
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

		private static void HideChild(Transform parent, string childName)
		{
			Transform existing = parent != null ? parent.Find(childName) : null;
			if (existing != null)
				existing.gameObject.SetActive(false);
		}

		private static void AddRowButton(Transform parent, KButton manage, string childName, string labelText, System.Action onClick)
		{
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
				label.text = labelText;
			KButton button = buttonGo.GetComponent<KButton>();
			if (button != null)
			{
				button.ClearOnClick();
				button.onClick += onClick;
			}
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
