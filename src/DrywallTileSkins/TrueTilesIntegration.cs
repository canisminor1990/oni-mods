using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class TrueTilesIntegration
	{
		private static bool patched;
		private static int imported;

		public static void Patch(Harmony harmony)
		{
			if (patched)
				return;

			Type tileAssets = AccessTools.TypeByName("TrueTiles.Cmps.TileAssets");
			if (tileAssets == null)
			{
				Debug.Log("[DrywallTileSkins] True Tiles is not loaded");
				return;
			}

			MethodInfo add = AccessTools.Method(tileAssets, "Add");
			if (add != null)
				harmony.Patch(add, postfix: new HarmonyMethod(typeof(TrueTilesIntegration), nameof(AddPostfix)));
			else
				Debug.LogWarning("[DrywallTileSkins] TrueTiles.TileAssets.Add was not found");

			Type loader = AccessTools.TypeByName("TrueTiles.Cmps.TileAssetLoader");
			if (loader != null)
			{
				string[] methods = { "LoadOverrides", "Reload", "ReloadAssets" };
				for (int i = 0; i < methods.Length; i++)
				{
					MethodInfo method = AccessTools.Method(loader, methods[i]);
					if (method != null)
						harmony.Patch(method, postfix: new HarmonyMethod(typeof(TrueTilesIntegration), nameof(AfterTrueTilesTexturesReady)));
				}
			}

			MethodInfo assetsInit = AccessTools.Method(typeof(Assets), "OnPrefabInit");
			if (assetsInit != null)
				harmony.Patch(assetsInit, postfix: new HarmonyMethod(typeof(TrueTilesIntegration), nameof(AfterTrueTilesTexturesReady)));

			patched = true;
			Debug.Log("[DrywallTileSkins] hooked True Tiles texture registration");
		}

		public static void AfterTrueTilesTexturesReady()
		{
			ImportExisting("TrueTiles ready");
			if (imported > 0)
				FacadeRegistrar.RegisterPending("TrueTiles ready");
		}

		public static void ImportExisting(string reason = "Collect")
		{
			Type tileAssets = AccessTools.TypeByName("TrueTiles.Cmps.TileAssets");
			if (tileAssets == null)
				return;

			object instance = AccessTools.Property(tileAssets, "Instance")?.GetValue(null, null);
			if (instance == null)
			{
				Debug.Log("[DrywallTileSkins] True Tiles import (" + reason + "): TileAssets.Instance is not ready");
				return;
			}

			object map = AccessTools.Field(tileAssets, "textureAssets")?.GetValue(instance);
			if (!(map is IDictionary byDef))
			{
				Debug.Log("[DrywallTileSkins] True Tiles import (" + reason + "): textureAssets map missing");
				return;
			}

			int before = imported;
			int seen = 0;
			foreach (DictionaryEntry defEntry in byDef)
			{
				string def = defEntry.Key as string;
				if (!(defEntry.Value is IDictionary byMaterial))
					continue;

				foreach (DictionaryEntry matEntry in byMaterial)
				{
					seen++;
					if (!(matEntry.Key is SimHashes material))
						continue;
					AddAsset(def, material, matEntry.Value);
				}
			}

			Debug.Log("[DrywallTileSkins] True Tiles import (" + reason + "): saw " + seen
				+ " assets, added " + (imported - before) + ", total " + imported);
		}

		public static void AddPostfix(object[] __args)
		{
			if (__args == null || __args.Length < 3 || !(__args[1] is SimHashes material))
				return;
			AddAsset(__args[0] as string, material, __args[2]);
		}

		private static void AddAsset(string def, SimHashes material, object asset)
		{
			if (asset == null || string.IsNullOrEmpty(def))
				return;

			Texture2D main = AccessTools.Field(asset.GetType(), "main")?.GetValue(asset) as Texture2D;
			if (main == null)
				return;

			BuildingDef buildingDef = Assets.GetBuildingDef(def);
			string groupId = ModSettings.ResolveGroupId(def);
			if (buildingDef != null)
				ModSettings.RegisterGroup(groupId, ModSettings.GroupNameFor(buildingDef));
			else
				ModSettings.RegisterGroup(groupId, groupId);
			if (!ModSettings.IsGroupEnabled(groupId))
				return;

			TextureAtlas layout = buildingDef != null ? buildingDef.BlockTileAtlas : null;
			Texture2D interior = TextureUtil.ExtractInterior(layout, main);
			if (interior == null)
				return;

			string id = PatternRegistry.Sanitize(def + "_" + material);
			string buildingName = buildingDef != null ? buildingDef.Name : def;
			string materialName = MaterialName(material);
			string name = string.IsNullOrEmpty(materialName) ? buildingName : buildingName + " (" + materialName + ")";
			Sprite ui = buildingDef != null ? buildingDef.GetUISprite() : TextureUtil.MakeSprite(interior);

			if (PatternRegistry.AddPattern(id, name, STRINGS.DRYWALL_TILE_SKINS.FACADE_DESC, interior, ui, groupId))
				imported++;
		}

		private static string MaterialName(SimHashes material)
		{
			try
			{
				Element element = ElementLoader.FindElementByHash(material);
				if (element != null && !string.IsNullOrEmpty(element.name))
					return element.name;
			}
			catch
			{
			}

			return material.ToString();
		}
	}
}
