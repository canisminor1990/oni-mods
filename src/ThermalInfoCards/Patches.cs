using HarmonyLib;
using System;
using UnityEngine;

namespace ThermalInfoCards
{
	public static class Patches
	{
		internal static HoverTooltip Tooltip { get; private set; }

		private static BuildTooltip buildTooltip = new BuildTooltip();

		internal static void SetupTooltips()
		{
			if (Tooltip != null)
				return;
			try
			{
				Tooltip = new HoverTooltip();
				Debug.Log(Mod.LogPrefix + "hover tooltip ready");
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "SetupTooltips failed: " + ex.Message);
			}
		}

		internal static void CleanupTooltips()
		{
			buildTooltip?.ClearDef();
			Tooltip = null;
		}

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

		[HarmonyPatch(typeof(Game), "OnSpawn")]
		public static class Game_OnSpawn_Patch
		{
			public static void Postfix()
			{
				SetupTooltips();
			}
		}

		[HarmonyPatch(typeof(Game), "OnDestroy")]
		public static class Game_OnDestroy_Patch
		{
			public static void Prefix()
			{
				CleanupTooltips();
			}
		}

		[HarmonyPatch(typeof(MaterialSelector), "SetEffects")]
		public static class MaterialSelector_SetEffects_Patch
		{
			[HarmonyPriority(Priority.Low)]
			public static bool Prefix(MaterialSelector __instance, Tag element)
			{
				try
				{
					bool hasInfo = buildTooltip != null
						&& __instance.selectorIndex == 0
						&& __instance.MaterialDescriptionPane != null
						&& __instance.MaterialEffectsPane != null;
					if (!hasInfo)
						return true;
					buildTooltip.AddThermalInfo(__instance.MaterialEffectsPane, element);
					return false;
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "SetEffects failed: " + ex.Message);
					return true;
				}
			}
		}

		[HarmonyPatch(typeof(ProductInfoScreen), nameof(ProductInfoScreen.Close))]
		public static class ProductInfoScreen_Close_Patch
		{
			public static void Postfix()
			{
				buildTooltip?.ClearDef();
			}
		}

		[HarmonyPatch(typeof(ProductInfoScreen), nameof(ProductInfoScreen.SetMaterials))]
		public static class ProductInfoScreen_SetMaterials_Patch
		{
			public static void Prefix(BuildingDef def)
			{
				if (buildTooltip != null)
					buildTooltip.Def = def;
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
	}
}
