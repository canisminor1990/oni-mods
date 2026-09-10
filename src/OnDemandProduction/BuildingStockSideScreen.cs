using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StockProduction
{
	public class BuildingStockSideScreen : SideScreenContent
	{
		private BuildingStockController controller;
		private bool built;
		private KNumberInputField countField;
		private MultiToggle decrement;
		private MultiToggle increment;
		private KButton unitButton;
		private KButton modeButton;
		private LocText unitLabel;
		private LocText modeLabel;
		private GameObject unitGo;

		public static void Register(DetailsScreen details)
		{
			if (details == null)
				return;

			var field = AccessTools.Field(typeof(DetailsScreen), "sideScreens");
			var list = field != null
				? field.GetValue(details) as List<DetailsScreen.SideScreenRef>
				: null;
			if (list == null)
				return;

			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null && list[i].screenPrefab is BuildingStockSideScreen)
					return;
			}

			GameObject go = new GameObject("OnDemandBuildingStock");
			go.AddComponent<RectTransform>();
			LayoutElement layout = go.AddComponent<LayoutElement>();
			layout.minHeight = 48f;
			layout.preferredHeight = 48f;
			layout.flexibleWidth = 1f;
			BuildingStockSideScreen screen = go.AddComponent<BuildingStockSideScreen>();
			screen.ContentContainer = go;
			go.SetActive(false);
			Object.DontDestroyOnLoad(go);

			list.Add(new DetailsScreen.SideScreenRef
			{
				name = "OnDemandBuildingStock",
				screenPrefab = screen,
				offset = Vector2.zero,
				tab = DetailsScreen.SidescreenTabTypes.Config
			});
		}

		public override bool IsValidForTarget(GameObject target)
		{
			return ModSettings.ExtraBuildings
				&& target != null
				&& target.GetComponent<BuildingStockController>() != null;
		}

		public override string GetTitle()
		{
			return STRINGS.STOCK_PRODUCTION.SIDE_TITLE;
		}

		public override int GetSideScreenSortOrder()
		{
			if (controller != null && controller.HasAutoMode)
				return -10;
			return 0;
		}

		public override void SetTarget(GameObject target)
		{
			controller = target != null ? target.GetComponent<BuildingStockController>() : null;
			EnsureControls();
			Refresh();
		}

		public override void ClearTarget()
		{
			controller = null;
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			ContentContainer = gameObject;
		}

		private void EnsureControls()
		{
			if (built)
				return;

			SelectedRecipeQueueScreen source = FindSource();
			if (source == null || source.QueueCount == null)
			{
				Debug.LogWarning(Mod.LogPrefix + "building stock UI: recipe screen missing");
				return;
			}

			GameObject cloneGo = Util.KInstantiateUI(source.gameObject, gameObject, false);
			cloneGo.name = "StockQueueClone";
			SelectedRecipeQueueScreen clone = cloneGo.GetComponent<SelectedRecipeQueueScreen>();
			if (clone == null)
			{
				Object.Destroy(cloneGo);
				Debug.LogWarning(Mod.LogPrefix + "building stock UI: clone failed");
				return;
			}
			clone.enabled = false;
			if (clone.InfiniteIcon != null)
				clone.InfiniteIcon.SetActive(false);

			ResolveWidgets(clone);
			if (countField == null)
			{
				Object.Destroy(cloneGo);
				Debug.LogWarning(Mod.LogPrefix + "building stock UI: queue field missing");
				return;
			}

			GameObject row = new GameObject("StockQueueRow", typeof(RectTransform));
			row.transform.SetParent(transform, false);
			LayoutElement rowLayout = row.AddComponent<LayoutElement>();
			rowLayout.minHeight = 32f;
			rowLayout.preferredHeight = 32f;
			rowLayout.flexibleWidth = 1f;
			HorizontalLayoutGroup horizontal = row.AddComponent<HorizontalLayoutGroup>();
			horizontal.spacing = 2f;
			horizontal.padding = new RectOffset(0, 0, 0, 0);
			horizontal.childAlignment = TextAnchor.MiddleCenter;
			horizontal.childControlWidth = false;
			horizontal.childControlHeight = false;
			horizontal.childForceExpandWidth = false;
			horizontal.childForceExpandHeight = false;
			ResetRect(row.transform as RectTransform, 32f);

			MoveWidget(decrement, row, 22f, 22f, true);
			MoveWidget(countField, row, 40f, 24f, false);
			MoveWidget(increment, row, 22f, 22f, false);

			if (modeButton != null)
			{
				unitGo = Util.KInstantiateUI(modeButton.gameObject, row, true);
				unitGo.name = "StockUnitToggle";
				unitButton = unitGo.GetComponent<KButton>();
				unitLabel = unitGo.GetComponentInChildren<LocText>(true);
				StockRecipeUi.FitLabel(unitLabel);
				if (unitButton != null)
				{
					unitButton.ClearOnClick();
					unitButton.onClick += OnToggleUnit;
				}
				StockRecipeUi.SetSimpleTooltip(unitGo, STRINGS.STOCK_PRODUCTION.UNIT_TOOLTIP);
				KeepSize(unitGo, 72f, 26f, false);

				modeLabel = modeButton.GetComponentInChildren<LocText>(true);
				StockRecipeUi.FitLabel(modeLabel);
				modeButton.ClearOnClick();
				modeButton.onClick += OnCycleMode;
				StockRecipeUi.SetSimpleTooltip(modeButton.gameObject, STRINGS.STOCK_PRODUCTION.BUILDING_MODE_TOOLTIP);
				MoveWidget(modeButton, row, 72f, 26f, false);
			}

			Object.Destroy(cloneGo);

			if (decrement != null)
			{
				decrement.onClick = delegate
				{
					OnStep(-1);
				};
			}
			if (increment != null)
			{
				increment.onClick = delegate
				{
					OnStep(1);
				};
			}
			countField.minValue = 1f;
			countField.maxValue = 99f;
			countField.decimalPlaces = 0;
			countField.onEndEdit += OnCountEdited;

			row.SetActive(true);
			built = true;
		}

		private void Refresh()
		{
			if (controller == null || !built)
				return;

			bool showAmount = controller.CurrentMode == BuildingStockController.Mode.Maintain;
			SetActive(decrement, showAmount);
			SetActive(increment, showAmount);
			SetActive(countField, showAmount);
			if (showAmount && countField != null && !StockRecipeUi.IsFieldEditing(countField))
				countField.SetAmount(controller.GetDisplayDigits());

			if (modeLabel != null)
			{
				modeLabel.text = controller.ModeButtonText;
				StockRecipeUi.FitLabel(modeLabel);
			}

			bool showUnit = showAmount && controller.CanToggleUnit;
			if (unitGo != null)
				unitGo.SetActive(showUnit);
			if (showUnit && unitLabel != null)
			{
				unitLabel.text = controller.UseTons
					? STRINGS.STOCK_PRODUCTION.UNIT_T
					: STRINGS.STOCK_PRODUCTION.UNIT_KG;
				StockRecipeUi.FitLabel(unitLabel);
			}
		}

		private void ResolveWidgets(SelectedRecipeQueueScreen clone)
		{
			countField = clone.QueueCount;
			decrement = clone.DecrementButton;
			increment = clone.IncrementButton;
			modeButton = clone.InfiniteButton;

			MultiToggle prev = AccessTools.Field(typeof(SelectedRecipeQueueScreen), "previousRecipeButton")
				?.GetValue(clone) as MultiToggle;
			MultiToggle next = AccessTools.Field(typeof(SelectedRecipeQueueScreen), "nextRecipeButton")
				?.GetValue(clone) as MultiToggle;

			if ((decrement == null || increment == null) && countField != null)
			{
				List<MultiToggle> arrows = new List<MultiToggle>();
				Transform search = countField.transform;
				while (search != null)
				{
					arrows.Clear();
					MultiToggle[] all = search.GetComponentsInChildren<MultiToggle>(true);
					for (int i = 0; i < all.Length; i++)
					{
						if (all[i] == null || all[i] == prev || all[i] == next)
							continue;
						arrows.Add(all[i]);
					}
					if (arrows.Count >= 2)
						break;
					search = search.parent;
				}
				if (arrows.Count >= 2)
				{
					arrows.Sort(CompareByNameAndOrder);
					if (decrement == null)
						decrement = PickNamed(arrows, "Dec", "Minus", "-") ?? arrows[0];
					if (increment == null)
						increment = PickNamed(arrows, "Inc", "Plus", "+") ?? arrows[arrows.Count - 1];
				}
			}

			if (modeButton == null && countField != null)
			{
				Transform search = countField.transform.parent;
				while (search != null && modeButton == null)
				{
					KButton[] buttons = search.GetComponentsInChildren<KButton>(true);
					for (int i = 0; i < buttons.Length; i++)
					{
						KButton button = buttons[i];
						if (button == null)
							continue;
						if (button.transform == countField.transform || button.transform.IsChildOf(countField.transform))
							continue;
						modeButton = button;
						break;
					}
					search = search.parent;
				}
			}
		}

		private static MultiToggle PickNamed(List<MultiToggle> arrows, string a, string b, string c)
		{
			for (int i = 0; i < arrows.Count; i++)
			{
				string name = arrows[i].gameObject.name;
				if (ContainsIgnoreCase(name, a) || ContainsIgnoreCase(name, b) || name.Contains(c))
					return arrows[i];
			}
			return null;
		}

		private static bool ContainsIgnoreCase(string text, string part)
		{
			return text != null && text.IndexOf(part, System.StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static int CompareByNameAndOrder(MultiToggle left, MultiToggle right)
		{
			return left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex());
		}

		private static void SetActive(Component widget, bool active)
		{
			if (widget != null)
				widget.gameObject.SetActive(active);
		}

		private static void MoveWidget(Component widget, GameObject row, float width, float height, bool flipX)
		{
			if (widget == null || row == null)
				return;
			widget.transform.SetParent(row.transform, false);
			widget.gameObject.SetActive(true);
			KeepSize(widget.gameObject, width, height, flipX);
		}

		private static void KeepSize(GameObject go, float width, float height, bool flipX)
		{
			if (go == null)
				return;
			RectTransform rt = go.GetComponent<RectTransform>();
			if (rt != null)
			{
				float scaleX = flipX ? -1f : 1f;
				rt.localScale = new Vector3(scaleX, 1f, 1f);
				rt.localRotation = Quaternion.identity;
				rt.anchorMin = new Vector2(0.5f, 0.5f);
				rt.anchorMax = new Vector2(0.5f, 0.5f);
				rt.pivot = new Vector2(0.5f, 0.5f);
				rt.sizeDelta = new Vector2(width, height);
			}
			LayoutElement layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
			layout.minWidth = width;
			layout.preferredWidth = width;
			layout.minHeight = height;
			layout.preferredHeight = height;
			layout.flexibleWidth = 0f;
			layout.flexibleHeight = 0f;
			layout.ignoreLayout = false;
		}

		private static void ResetRect(RectTransform rt, float height)
		{
			if (rt == null)
				return;
			rt.anchorMin = new Vector2(0f, 0.5f);
			rt.anchorMax = new Vector2(1f, 0.5f);
			rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = Vector2.zero;
			rt.sizeDelta = new Vector2(0f, height);
			rt.localScale = Vector3.one;
		}

		private void OnStep(int delta)
		{
			if (controller == null)
				return;
			controller.Step(delta);
			Refresh();
		}

		private void OnCountEdited()
		{
			if (controller == null || countField == null)
				return;
			controller.SetDisplayDigits(Mathf.RoundToInt(countField.currentValue));
			Refresh();
		}

		private void OnToggleUnit()
		{
			if (controller == null)
				return;
			controller.ToggleUnit();
			Refresh();
		}

		private void OnCycleMode()
		{
			if (controller == null)
				return;
			controller.CycleMode();
			Refresh();
		}

		private static SelectedRecipeQueueScreen FindSource()
		{
			SelectedRecipeQueueScreen[] screens = Object.FindObjectsByType<SelectedRecipeQueueScreen>(
				FindObjectsInactive.Include,
				FindObjectsSortMode.None);
			for (int i = 0; i < screens.Length; i++)
			{
				SelectedRecipeQueueScreen screen = screens[i];
				if (screen == null || screen.QueueCount == null || screen.InfiniteButton == null)
					continue;
				if (screen.GetComponentInParent<BuildingStockSideScreen>() != null)
					continue;
				return screen;
			}
			return FindRecipePrefab();
		}

		private static SelectedRecipeQueueScreen FindRecipePrefab()
		{
			DetailsScreen details = DetailsScreen.Instance;
			if (details == null)
				return null;
			var field = AccessTools.Field(typeof(DetailsScreen), "sideScreens");
			var list = field != null
				? field.GetValue(details) as List<DetailsScreen.SideScreenRef>
				: null;
			if (list == null)
				return null;

			for (int i = 0; i < list.Count; i++)
			{
				DetailsScreen.SideScreenRef side = list[i];
				if (side == null)
					continue;
				ComplexFabricatorSideScreen fab = side.screenPrefab as ComplexFabricatorSideScreen;
				if (fab == null)
					fab = side.screenInstance as ComplexFabricatorSideScreen;
				if (fab != null && fab.recipeScreenPrefab != null)
					return fab.recipeScreenPrefab;
			}
			return null;
		}
	}
}
