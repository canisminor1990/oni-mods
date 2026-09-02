using Database;
using HarmonyLib;

namespace DuplicantPortraits
{
	public static class Patches
	{
		[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
		public static class Localization_Initialize_Patch
		{
			public static void Postfix()
			{
				I18n.Register();
			}
		}

		[HarmonyPatch(typeof(KAnimFile), nameof(KAnimFile.FinalizeLoading))]
		public static class KAnimFile_FinalizeLoading_Patch
		{
			public static void Prefix(KAnimFile __instance)
			{
				CanvasKanimFactory.CaptureIfTemplate(__instance);
			}
		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Postfix()
			{
				PortraitStages.Register("Db.Initialize");
				PortraitStages.AddToInventory();
			}
		}

		[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
		{
			public static void Postfix()
			{
				PortraitStages.Register("LoadGeneratedBuildings");
				PortraitStages.AddToInventory();
			}
		}

		[HarmonyPatch(typeof(InventoryOrganization), "GenerateSubcategories")]
		public static class InventoryOrganization_GenerateSubcategories_Patch
		{
			public static void Postfix()
			{
				PortraitStages.AddToInventory();
			}
		}
	}
}
