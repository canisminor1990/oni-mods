using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StockProduction
{
	internal class StockRecipeUiState : MonoBehaviour
	{
		public LocText modeLabel;
		public float originalMax = 99f;
		public GameObject unitButton;
		public LocText unitLabel;
		public readonly Dictionary<string, bool> useTons = new Dictionary<string, bool>();
	}

	internal static class StockRecipeUi
	{
		private const int KgPerTon = 1000;
		private const int DigitMax = 99;

		public static void Install(SelectedRecipeQueueScreen screen)
		{
			if (screen == null || screen.InfiniteButton == null || screen.QueueCount == null)
				return;
			if (screen.GetComponent<StockRecipeUiState>() != null)
				return;

			StockRecipeUiState state = screen.gameObject.AddComponent<StockRecipeUiState>();
			state.originalMax = screen.QueueCount.maxValue > 0f ? screen.QueueCount.maxValue : DigitMax;
			state.modeLabel = screen.InfiniteButton.GetComponentInChildren<LocText>();
			CreateUnitButton(screen, state);
			BindVanillaButtons(screen);
			SetModeTooltip(screen.InfiniteButton);
		}

		public static void Refresh(SelectedRecipeQueueScreen screen)
		{
			StockRecipeUiState state = screen != null ? screen.GetComponent<StockRecipeUiState>() : null;
			if (state == null)
				return;

			ComplexFabricator fabricator = GetFabricator(screen);
			ComplexRecipe recipe = GetSelectedRecipe(screen);
			StockRecipeController controller = fabricator != null
				? fabricator.GetComponent<StockRecipeController>()
				: null;
			bool stock = controller != null && recipe != null && controller.IsEnabled(recipe.id);
			int queue = 0;
			if (fabricator != null && recipe != null)
			{
				try
				{
					queue = fabricator.GetRecipeQueueCount(recipe);
				}
				catch
				{
					queue = 0;
				}
			}

			if (state.modeLabel != null)
			{
				if (stock)
					state.modeLabel.text = STRINGS.STOCK_PRODUCTION.MODE_STOCK;
				else if (queue == ComplexFabricator.QUEUE_INFINITE)
					state.modeLabel.text = STRINGS.STOCK_PRODUCTION.MODE_FOREVER;
				else
					state.modeLabel.text = STRINGS.STOCK_PRODUCTION.MODE_ONCE;
				FitLabel(state.modeLabel);
			}

			if (!stock)
			{
				screen.QueueCount.maxValue = state.originalMax;
				screen.QueueCount.minValue = 0f;
				screen.QueueCount.decimalPlaces = 0;
				SetUnitButtonVisible(state, false);
				return;
			}

			int target = controller.GetTarget(recipe.id);
			bool mass = StockInventory.UsesMass(recipe);
			bool tons = mass && UseTons(state, recipe.id, target);
			screen.QueueCount.minValue = 1f;
			screen.QueueCount.maxValue = DigitMax;
			screen.QueueCount.decimalPlaces = 0;
			SetUnitButtonVisible(state, mass);
			if (mass)
				RefreshUnitButton(state, tons);

			if (screen.QueueCount != null && !IsFieldEditing(screen.QueueCount))
				screen.QueueCount.SetAmount(DisplayAmount(target, tons));
			if (screen.InfiniteIcon != null)
				screen.InfiniteIcon.gameObject.SetActive(false);

			ToolTip queueTip = screen.QueueCount.GetComponent<ToolTip>()
				?? screen.QueueCount.gameObject.GetComponentInChildren<ToolTip>();
			if (queueTip != null)
				queueTip.SetSimpleTooltip(STRINGS.STOCK_PRODUCTION.TARGET_TOOLTIP);
		}

		public static string FormatQueueCount(ComplexRecipe recipe, int target)
		{
			if (StockInventory.UsesMass(recipe) && target >= KgPerTon)
				return DisplayAmount(target, true) + STRINGS.STOCK_PRODUCTION.UNIT_T;
			return target.ToString();
		}

		private static int DisplayAmount(int targetKg, bool tons)
		{
			if (!tons)
				return Mathf.Clamp(targetKg, 1, DigitMax);
			int tonsValue = Mathf.RoundToInt(targetKg / (float)KgPerTon);
			if (tonsValue < 1)
				tonsValue = 1;
			return Mathf.Clamp(tonsValue, 1, DigitMax);
		}

		private static bool IsFieldEditing(KNumberInputField field)
		{
			return field != null && field.field != null && field.field.isFocused;
		}

		private static void BindVanillaButtons(SelectedRecipeQueueScreen screen)
		{
			screen.IncrementButton.onClick = delegate
			{
				OnStep(screen, 1);
			};
			screen.DecrementButton.onClick = delegate
			{
				OnStep(screen, -1);
			};
			screen.InfiniteButton.ClearOnClick();
			screen.InfiniteButton.onClick += delegate
			{
				OnCycleMode(screen);
			};
			screen.QueueCount.onEndEdit += delegate
			{
				OnTargetEdited(screen);
			};
		}

		private static void CreateUnitButton(SelectedRecipeQueueScreen screen, StockRecipeUiState state)
		{
			Transform parent = screen.InfiniteButton.transform.parent;
			if (parent == null)
				return;

			GameObject go = Util.KInstantiateUI(screen.InfiniteButton.gameObject, parent.gameObject, true);
			go.name = "StockUnitToggle";
			go.transform.SetSiblingIndex(screen.InfiniteButton.transform.GetSiblingIndex());

			LayoutElement srcLayout = screen.InfiniteButton.GetComponent<LayoutElement>();
			LayoutElement dstLayout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
			if (srcLayout != null)
			{
				dstLayout.minWidth = srcLayout.minWidth;
				dstLayout.preferredWidth = srcLayout.preferredWidth;
				dstLayout.flexibleWidth = srcLayout.flexibleWidth;
				dstLayout.minHeight = srcLayout.minHeight;
				dstLayout.preferredHeight = srcLayout.preferredHeight;
				dstLayout.flexibleHeight = srcLayout.flexibleHeight;
			}

			RectTransform srcRt = screen.InfiniteButton.GetComponent<RectTransform>();
			RectTransform dstRt = go.GetComponent<RectTransform>();
			if (srcRt != null && dstRt != null)
				dstRt.sizeDelta = srcRt.sizeDelta;

			KButton button = go.GetComponent<KButton>();
			if (button != null)
			{
				button.ClearOnClick();
				button.onClick += delegate
				{
					OnToggleUnit(screen);
				};
			}

			LocText label = go.GetComponentInChildren<LocText>(true);
			FitLabel(label);
			SetSimpleTooltip(go, STRINGS.STOCK_PRODUCTION.UNIT_TOOLTIP);

			state.unitButton = go;
			state.unitLabel = label;
			go.SetActive(false);
		}

		private static void SetUnitButtonVisible(StockRecipeUiState state, bool visible)
		{
			if (state.unitButton != null)
				state.unitButton.SetActive(visible);
		}

		private static void RefreshUnitButton(StockRecipeUiState state, bool tons)
		{
			if (state.unitLabel != null)
			{
				state.unitLabel.text = tons
					? STRINGS.STOCK_PRODUCTION.UNIT_T
					: STRINGS.STOCK_PRODUCTION.UNIT_KG;
				FitLabel(state.unitLabel);
			}
			if (state.unitButton != null)
				SetSimpleTooltip(state.unitButton, STRINGS.STOCK_PRODUCTION.UNIT_TOOLTIP);
		}

		private static bool UseTons(StockRecipeUiState state, string recipeId, int targetKg)
		{
			if (state.useTons.TryGetValue(recipeId, out bool stored))
				return stored;
			return targetKg > DigitMax;
		}

		private static void OnToggleUnit(SelectedRecipeQueueScreen screen)
		{
			StockRecipeUiState state = screen.GetComponent<StockRecipeUiState>();
			ComplexRecipe recipe = GetSelectedRecipe(screen);
			StockRecipeController controller = GetFabricator(screen)?.GetComponent<StockRecipeController>();
			if (state == null || recipe == null || controller == null || !controller.IsEnabled(recipe.id))
				return;
			if (!StockInventory.UsesMass(recipe))
				return;

			int target = controller.GetTarget(recipe.id);
			bool tons = UseTons(state, recipe.id, target);
			int digits = DisplayAmount(target, tons);
			bool nowTons = !tons;
			state.useTons[recipe.id] = nowTons;
			controller.SetTarget(recipe.id, nowTons ? digits * KgPerTon : digits);
			RefreshAfterChange(screen);
		}

		private static void SetModeTooltip(KButton button)
		{
			if (button == null)
				return;
			SetSimpleTooltip(button.gameObject, STRINGS.STOCK_PRODUCTION.MODE_TOOLTIP);
		}

		private static void SetSimpleTooltip(GameObject go, string text)
		{
			if (go == null)
				return;
			ToolTip tip = go.GetComponent<ToolTip>() ?? go.GetComponentInChildren<ToolTip>();
			if (tip == null)
				tip = go.AddComponent<ToolTip>();
			tip.SetSimpleTooltip(text);
		}

		private static void FitLabel(LocText label)
		{
			if (label == null)
				return;
			label.textWrappingMode = TextWrappingModes.NoWrap;
			label.overflowMode = TextOverflowModes.Overflow;
			label.raycastTarget = false;
		}

		private static void OnCycleMode(SelectedRecipeQueueScreen screen)
		{
			ComplexFabricator fabricator = GetFabricator(screen);
			ComplexRecipe recipe = GetSelectedRecipe(screen);
			if (fabricator == null || recipe == null)
				return;
			StockRecipeController controller = fabricator.GetComponent<StockRecipeController>();
			if (controller == null)
				return;

			int count = 0;
			try
			{
				count = fabricator.GetRecipeQueueCount(recipe);
			}
			catch
			{
				count = 0;
			}

			if (controller.IsEnabled(recipe.id))
				controller.Disable(recipe.id, ComplexFabricator.QUEUE_INFINITE);
			else if (count == ComplexFabricator.QUEUE_INFINITE)
				fabricator.SetRecipeQueueCount(recipe, 0);
			else
				controller.Enable(recipe, count);

			RefreshAfterChange(screen);
		}

		private static void OnStep(SelectedRecipeQueueScreen screen, int delta)
		{
			ComplexFabricator fabricator = GetFabricator(screen);
			ComplexRecipe recipe = GetSelectedRecipe(screen);
			if (fabricator == null || recipe == null)
				return;
			StockRecipeController controller = fabricator.GetComponent<StockRecipeController>();
			if (controller != null && controller.IsEnabled(recipe.id))
			{
				StockRecipeUiState state = screen.GetComponent<StockRecipeUiState>();
				int target = controller.GetTarget(recipe.id);
				bool tons = state != null && StockInventory.UsesMass(recipe) && UseTons(state, recipe.id, target);
				int digits = DisplayAmount(target, tons) + delta;
				digits = Mathf.Clamp(digits, 1, DigitMax);
				int kg = tons ? digits * KgPerTon : digits;
				controller.SetTarget(recipe.id, kg);
			}
			else if (delta > 0)
			{
				fabricator.IncrementRecipeQueueCount(recipe);
			}
			else
			{
				fabricator.DecrementRecipeQueueCount(recipe, false);
			}
			RefreshAfterChange(screen);
		}

		private static void OnTargetEdited(SelectedRecipeQueueScreen screen)
		{
			ComplexFabricator fabricator = GetFabricator(screen);
			ComplexRecipe recipe = GetSelectedRecipe(screen);
			StockRecipeController controller = fabricator != null
				? fabricator.GetComponent<StockRecipeController>()
				: null;
			if (controller == null || recipe == null || !controller.IsEnabled(recipe.id))
				return;

			StockRecipeUiState state = screen.GetComponent<StockRecipeUiState>();
			int digits = Mathf.Clamp(Mathf.RoundToInt(screen.QueueCount.currentValue), 1, DigitMax);
			bool tons = state != null && StockInventory.UsesMass(recipe)
				&& UseTons(state, recipe.id, controller.GetTarget(recipe.id));
			controller.SetTarget(recipe.id, tons ? digits * KgPerTon : digits);
			RefreshAfterChange(screen);
		}

		private static void RefreshAfterChange(SelectedRecipeQueueScreen screen)
		{
			AccessTools.Method(typeof(SelectedRecipeQueueScreen), "RefreshQueueCountDisplay")
				?.Invoke(screen, null);
			AccessTools.Method(typeof(SelectedRecipeQueueScreen), "RefreshIngredientDescriptors")
				?.Invoke(screen, null);
			ComplexFabricator fabricator = GetFabricator(screen);
			string category = AccessTools.Field(typeof(SelectedRecipeQueueScreen), "selectedRecipeCategoryID")
				?.GetValue(screen) as string;
			ComplexFabricatorSideScreen owner = AccessTools.Field(typeof(SelectedRecipeQueueScreen), "ownerScreen")
				?.GetValue(screen) as ComplexFabricatorSideScreen;
			if (owner != null && fabricator != null && !string.IsNullOrEmpty(category))
				owner.RefreshQueueCountDisplayForRecipeCategory(category, fabricator);
		}

		private static ComplexFabricator GetFabricator(SelectedRecipeQueueScreen screen)
		{
			return AccessTools.Field(typeof(SelectedRecipeQueueScreen), "target")?.GetValue(screen) as ComplexFabricator;
		}

		private static ComplexRecipe GetSelectedRecipe(SelectedRecipeQueueScreen screen)
		{
			return AccessTools.Property(typeof(SelectedRecipeQueueScreen), "selectedRecipe")?.GetValue(screen) as ComplexRecipe;
		}
	}
}
