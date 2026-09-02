using Database;
using HarmonyLib;
using UnityEngine;

namespace DrywallTileSkins
{
	public static class Patches
	{
		[HarmonyPatch(typeof(KAnimFile), nameof(KAnimFile.FinalizeLoading))]
		public static class KAnimFile_FinalizeLoading_Patch
		{
			public static void Prefix(KAnimFile __instance)
			{
				if (__instance == null)
					return;
				string name = __instance.name;
				if (name != "walls_kanim" && name != "walls")
					return;

				Mod.WallsKanim = __instance;
				if (__instance.animBytes != null)
					Mod.WallsAnimBytes = __instance.animBytes;
				if (__instance.buildBytes != null)
					Mod.WallsBuildBytes = __instance.buildBytes;
				if (__instance.textureList != null && __instance.textureList.Count > 0)
					Mod.WallsTexture = __instance.textureList[0];
			}
		}

		[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
		{
			public static void Postfix()
			{
				EnsureTemplateCaptured();
				PatternRegistry.Collect();
				FacadeRegistrar.RegisterPending("LoadGeneratedBuildings");
			}
		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Prefix()
			{
				EnsureTemplateCaptured();
				if (!PatternRegistry.HasCollected)
					PatternRegistry.Collect();

				var facadesType = AccessTools.TypeByName("Database.BuildingFacades");
				var ctor = AccessTools.Constructor(facadesType, new[] { typeof(ResourceSet) });
				Mod.HarmonyInstance.Patch(ctor, postfix: new HarmonyMethod(typeof(BuildingFacades_Ctor_Patch), nameof(BuildingFacades_Ctor_Patch.Postfix)));

				var inventoryType = AccessTools.TypeByName("InventoryOrganization");
				var generate = AccessTools.Method(inventoryType, "GenerateSubcategories");
				Mod.HarmonyInstance.Patch(generate, postfix: new HarmonyMethod(typeof(InventoryOrganization_GenerateSubcategories_Patch), nameof(InventoryOrganization_GenerateSubcategories_Patch.Postfix)));
			}

			public static void Postfix()
			{
				EnsureTemplateCaptured();
				PatternRegistry.Collect();
				FacadeRegistrar.RegisterPending("Db.Initialize");
			}
		}

		[HarmonyPatch(typeof(BuildingFacades), nameof(BuildingFacades.PostProcess))]
		public static class BuildingFacades_PostProcess_Patch
		{
			public static void Prefix()
			{
				FacadeRegistrar.RegisterPending("BuildingFacades.PostProcess");
			}
		}

		public static class BuildingFacades_Ctor_Patch
		{
			public static void Postfix(object __instance)
			{
				FacadeRegistrar.RegisterPending("BuildingFacades.ctor", (BuildingFacades)__instance);
			}
		}

		public static class InventoryOrganization_GenerateSubcategories_Patch
		{
			public static void Postfix()
			{
				FacadeRegistrar.AddAllToInventory();
			}
		}

		[HarmonyPatch(typeof(BuildingFacadeResource), nameof(BuildingFacadeResource.GetPermitPresentationInfo))]
		public static class BuildingFacadeResource_GetPermitPresentationInfo_Patch
		{
			public static void Postfix(BuildingFacadeResource __instance, ref PermitPresentationInfo __result)
			{
				TilePattern pattern = PatternRegistry.Find(__instance.Id);
				if (pattern == null || pattern.UISprite == null)
					return;
				__result.sprite = pattern.UISprite;
			}
		}

		[HarmonyPatch(typeof(Def), nameof(Def.GetUISpriteFromMultiObjectAnim))]
		public static class Def_GetUISpriteFromMultiObjectAnim_Patch
		{
			public static bool Prefix(KAnimFile animFile, string animName, bool centered, string symbolName, ref Sprite __result)
			{
				if (animFile == null)
					return true;
				if (!IsUiRequest(animName) || !IsUiRequest(symbolName))
					return true;

				TilePattern pattern = PatternRegistry.FindByKanim(animFile);
				if (pattern == null || pattern.UISprite == null || pattern.UISprite.texture == null)
					return true;
				if (pattern.UISprite.texture.name.IndexOf("_wallpaper_ui", System.StringComparison.Ordinal) < 0)
					return true;

				__result = pattern.UISprite;
				return false;
			}

			private static bool IsUiRequest(string name)
			{
				return string.IsNullOrEmpty(name) || name == "ui";
			}
		}

		[HarmonyPatch(typeof(AnimTileable), "UpdateEndCaps")]
		public static class AnimTileable_UpdateEndCaps_Patch
		{
			private static readonly KAnimHashedString[] Caps =
			{
				new KAnimHashedString("cap_left"),
				new KAnimHashedString("cap_left_fg"),
				new KAnimHashedString("cap_left_place"),
				new KAnimHashedString("cap_right"),
				new KAnimHashedString("cap_right_fg"),
				new KAnimHashedString("cap_right_place"),
				new KAnimHashedString("cap_top"),
				new KAnimHashedString("cap_top_fg"),
				new KAnimHashedString("cap_top_place"),
				new KAnimHashedString("cap_bottom"),
				new KAnimHashedString("cap_bottom_fg"),
				new KAnimHashedString("cap_bottom_place")
			};

			public static void Postfix(AnimTileable __instance)
			{
				BuildingFacade facade = __instance.GetComponent<BuildingFacade>();
				if (facade == null || !Mod.IsOurFacade(facade.CurrentFacade))
					return;

				KBatchedAnimController[] controllers = __instance.GetComponentsInChildren<KBatchedAnimController>();
				for (int i = 0; i < controllers.Length; i++)
				{
					for (int c = 0; c < Caps.Length; c++)
						controllers[i].SetSymbolVisiblity(Caps[c], false);
				}
			}
		}

		[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
		public static class Localization_Initialize_Patch
		{
			public static void Postfix()
			{
				I18n.Register();
			}
		}

		private static void EnsureTemplateCaptured()
		{
			if (Mod.WallsKanim == null)
				Mod.WallsKanim = Assets.GetAnim("walls_kanim");

			if (Mod.WallsKanim == null)
				return;

			if (Mod.WallsTexture == null && Mod.WallsKanim.textureList != null && Mod.WallsKanim.textureList.Count > 0)
				Mod.WallsTexture = Mod.WallsKanim.textureList[0];

			if (Mod.WallsAnimBytes == null)
				Mod.WallsAnimBytes = Mod.WallsKanim.animBytes;
			if (Mod.WallsBuildBytes == null)
				Mod.WallsBuildBytes = Mod.WallsKanim.buildBytes;

			if (Mod.WallsAnimBytes == null || Mod.WallsBuildBytes == null || Mod.WallsTexture == null)
				Debug.LogWarning("[DrywallTileSkins] walls_kanim data was not captured; tile skins cannot be created");
		}
	}
}
