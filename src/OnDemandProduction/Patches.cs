using HarmonyLib;
using System;
using UnityEngine;

namespace StockProduction
{
	public static class Patches
	{
		[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
		public static class Localization_Initialize_Patch
		{
			public static void Postfix()
			{
				try
				{
					I18n.Register();
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "i18n failed: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(ComplexFabricator), "OnPrefabInit")]
		public static class ComplexFabricator_OnPrefabInit_Patch
		{
			public static void Postfix(ComplexFabricator __instance)
			{
				__instance.gameObject.AddOrGet<StockRecipeController>();
			}
		}

		[HarmonyPatch(typeof(FertilizerMakerConfig), nameof(FertilizerMakerConfig.DoPostConfigureComplete))]
		public static class FertilizerMakerConfig_DoPostConfigureComplete_Patch
		{
			public static void Postfix(GameObject go)
			{
				go.AddOrGet<BuildingStockController>().Configure(BuildingStockController.Kind.Fertilizer);
			}
		}

		[HarmonyPatch(typeof(PowerControlStationConfig), nameof(PowerControlStationConfig.DoPostConfigureComplete))]
		public static class PowerControlStationConfig_DoPostConfigureComplete_Patch
		{
			public static void Postfix(GameObject go)
			{
				go.AddOrGet<BuildingStockController>().Configure(BuildingStockController.Kind.Microchip);
			}
		}

		[HarmonyPatch(typeof(FarmStationConfig), nameof(FarmStationConfig.DoPostConfigureComplete))]
		public static class FarmStationConfig_DoPostConfigureComplete_Patch
		{
			public static void Postfix(GameObject go)
			{
				go.AddOrGet<BuildingStockController>().Configure(BuildingStockController.Kind.FarmKit);
			}
		}

		[HarmonyPatch(typeof(CompostConfig), nameof(CompostConfig.DoPostConfigureComplete))]
		public static class CompostConfig_DoPostConfigureComplete_Patch
		{
			public static void Postfix(GameObject go)
			{
				go.AddOrGet<BuildingStockController>().Configure(BuildingStockController.Kind.Compost);
			}
		}

		[HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
		public static class ModsScreen_BuildDisplay_Patch
		{
			public static void Postfix(object __instance)
			{
				try
				{
					SettingsScreen.AddModButton(__instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "ModsScreen button failed: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class DetailsScreen_OnPrefabInit_Patch
		{
			public static void Postfix(DetailsScreen __instance)
			{
				try
				{
					BuildingStockSideScreen.Register(__instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "building stock side screen failed: " + ex);
				}
			}
		}

		[HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.Sim1000ms))]
		public static class ComplexFabricator_Sim1000ms_Patch
		{
			public static void Postfix(ComplexFabricator __instance)
			{
				StockRecipeController controller = __instance.GetComponent<StockRecipeController>();
				controller?.RefreshAll(__instance);
			}
		}

		[HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.CompleteWorkingOrder))]
		public static class ComplexFabricator_CompleteWorkingOrder_Patch
		{
			public static void Postfix(ComplexFabricator __instance)
			{
				StockRecipeController controller = __instance.GetComponent<StockRecipeController>();
				controller?.RefreshAll(__instance);
			}
		}

		[HarmonyPatch(typeof(ComplexFabricator), "OnCopySettings")]
		public static class ComplexFabricator_OnCopySettings_Patch
		{
			public static void Postfix(ComplexFabricator __instance, object data)
			{
				GameObject sourceGo = data as GameObject;
				if (sourceGo == null)
					return;
				StockRecipeController dest = __instance.GetComponent<StockRecipeController>();
				StockRecipeController source = sourceGo.GetComponent<StockRecipeController>();
				if (dest == null)
					dest = __instance.gameObject.AddOrGet<StockRecipeController>();
				dest.CopyFrom(source, __instance);
			}
		}

		[HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.SetRecipeQueueCount))]
		public static class ComplexFabricator_SetRecipeQueueCount_Patch
		{
			public static bool Prefix(ComplexFabricator __instance, ComplexRecipe recipe, int count)
			{
				if (recipe == null)
					return true;
				StockRecipeController controller = __instance.GetComponent<StockRecipeController>();
				if (controller == null || controller.Applying || !controller.IsEnabled(recipe.id))
					return true;
				if (count == ComplexFabricator.QUEUE_INFINITE || count == 0)
				{
					controller.Disable(recipe.id, count);
					return false;
				}
				if (count > 0)
				{
					controller.SetTarget(recipe.id, StockRecipeUi.DisplayToStored(__instance, recipe, count));
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(SelectedRecipeQueueScreen), "OnSpawn")]
		public static class SelectedRecipeQueueScreen_OnSpawn_Patch
		{
			public static void Postfix(SelectedRecipeQueueScreen __instance)
			{
				try
				{
					StockRecipeUi.Install(__instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "recipe screen UI failed: " + ex);
				}
			}
		}

		[HarmonyPatch(typeof(SelectedRecipeQueueScreen), "RefreshQueueCountDisplay")]
		public static class SelectedRecipeQueueScreen_RefreshQueueCountDisplay_Patch
		{
			public static void Postfix(SelectedRecipeQueueScreen __instance)
			{
				try
				{
					StockRecipeUi.Refresh(__instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "recipe screen refresh failed: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(ComplexFabricatorSideScreen), "RefreshQueueCountDisplay")]
		public static class ComplexFabricatorSideScreen_RefreshQueueCountDisplay_Patch
		{
			public static void Postfix(GameObject entryGO, ComplexFabricator fabricator)
			{
				try
				{
					RefreshRecipeRow(entryGO, fabricator);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "recipe row refresh failed: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(ComplexFabricatorSideScreen), "RefreshQueueTooltip")]
		public static class ComplexFabricatorSideScreen_RefreshQueueTooltip_Patch
		{
			public static void Postfix(GameObject entryGO, ComplexFabricatorSideScreen __instance)
			{
				try
				{
					RefreshRecipeTooltip(entryGO, __instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "recipe tooltip failed: " + ex.Message);
				}
			}
		}

		private static void RefreshRecipeRow(GameObject entryGO, ComplexFabricator fabricator)
		{
			if (entryGO == null || fabricator == null)
				return;
			StockRecipeController controller = fabricator.GetComponent<StockRecipeController>();
			ComplexRecipe recipe = StockRecipeOnRow(entryGO, controller);
			if (controller == null || recipe == null)
				return;

			HierarchyReferences refs = entryGO.GetComponent<HierarchyReferences>();
			if (refs == null)
				return;
			int target = controller.GetTarget(recipe.id);
			refs.GetReference<LocText>("CountLabel").text = StockRecipeUi.FormatQueueCount(recipe, target);
			refs.GetReference<RectTransform>("InfiniteIcon").gameObject.SetActive(false);
			KButton box = refs.GetReference<KButton>("QueueBoxButton");
			if (box != null && box.bgImage != null)
			{
				ComplexFabricatorSideScreen side = entryGO.GetComponentInParent<ComplexFabricatorSideScreen>();
				if (side != null)
				{
					bool enough = StockInventory.HasEnough(fabricator, recipe, target);
					box.bgImage.colorStyleSetting = enough ? side.emptyQueueColorStyle : side.standardQueueColorStyle;
					box.bgImage.ApplyColorStyleSetting();
				}
			}
		}

		private static void RefreshRecipeTooltip(GameObject entryGO, ComplexFabricatorSideScreen side)
		{
			if (entryGO == null || side == null)
				return;
			ComplexFabricator fabricator = AccessTools.Field(typeof(ComplexFabricatorSideScreen), "targetFab")
				?.GetValue(side) as ComplexFabricator;
			StockRecipeController controller = fabricator != null
				? fabricator.GetComponent<StockRecipeController>()
				: null;
			ComplexRecipe recipe = StockRecipeOnRow(entryGO, controller);
			if (controller == null || recipe == null)
				return;

			HierarchyReferences refs = entryGO.GetComponent<HierarchyReferences>();
			if (refs == null)
				return;
			ToolTip tip = refs.GetReference<ToolTip>("QueueTooltip");
			if (tip == null)
				return;
			int target = controller.GetTarget(recipe.id);
			int current = StockInventory.GetCount(fabricator, recipe);
			tip.SetSimpleTooltip(string.Format(
				STRINGS.STOCK_PRODUCTION.QUEUE_TOOLTIP,
				StockRecipeUi.FormatQueueCount(recipe, target),
				StockInventory.Format(fabricator, recipe, current)));
		}

		private static ComplexRecipe StockRecipeOnRow(GameObject entryGO, StockRecipeController controller)
		{
			if (controller == null)
				return null;
			var mapField = AccessTools.Field(typeof(ComplexFabricatorSideScreen), "recipeCategoryToggleMap");
			if (mapField == null)
				return null;
			ComplexFabricatorSideScreen side = entryGO.GetComponentInParent<ComplexFabricatorSideScreen>();
			object map = side != null ? mapField.GetValue(side) : null;
			if (!(map is System.Collections.Generic.Dictionary<GameObject, System.Collections.Generic.List<ComplexRecipe>> dict))
				return null;
			if (!dict.TryGetValue(entryGO, out System.Collections.Generic.List<ComplexRecipe> recipes) || recipes == null)
				return null;
			for (int i = 0; i < recipes.Count; i++)
			{
				if (recipes[i] != null && controller.IsEnabled(recipes[i].id))
					return recipes[i];
			}
			return null;
		}
	}
}
