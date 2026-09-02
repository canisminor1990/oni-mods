using Database;
using System.Collections.Generic;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class FacadeRegistrar
	{
		public static void RegisterPending(string reason, BuildingFacades facades = null)
		{
			if (PatternRegistry.Patterns.Count == 0)
			{
				Debug.Log("[DrywallTileSkins] skip facade register (" + reason + "): no patterns yet");
				return;
			}

			PermitResources permits = null;
			try
			{
				Db db = Db.Get();
				if (db != null)
					permits = db.Permits;
				if (facades == null && permits != null)
					facades = permits.BuildingFacades;
			}
			catch (System.Exception ex)
			{
				Debug.Log("[DrywallTileSkins] skip facade register (" + reason + "): " + ex.Message);
				return;
			}

			if (facades == null)
			{
				Debug.Log("[DrywallTileSkins] skip facade register (" + reason + "): BuildingFacades not ready");
				return;
			}

			int added = 0;
			int permitsAdded = 0;
			foreach (TilePattern pattern in PatternRegistry.Patterns)
			{
				string id = Mod.FacadeIdFor(pattern.Id);
				BuildingFacadeResource resource = facades.TryGet(id);
				if (resource == null)
				{
					facades.Add(
						id,
						pattern.Name,
						I18n.DescriptionForGroup(pattern.GroupId),
						PermitRarity.Universal,
						ExteriorWallConfig.ID,
						pattern.KanimName,
						null,
						null,
						null,
						null);
					resource = facades.TryGet(id);
					added++;
				}

				if (resource == null)
					continue;

				if (permits != null && permits.TryGet(id) == null)
				{
					permits.Add(resource);
					permitsAdded++;
				}

				resource.Init();
				AddToInventory(pattern);
			}

			BuildingDef wall = Assets.GetBuildingDef(ExteriorWallConfig.ID);
			int available = wall != null && wall.AvailableFacades != null ? wall.AvailableFacades.Count : -1;
			Debug.Log("[DrywallTileSkins] facades (" + reason + "): added " + added
				+ ", permitsAdded " + permitsAdded
				+ ", patterns " + PatternRegistry.Patterns.Count
				+ ", ExteriorWall.AvailableFacades=" + available);
		}

		public static void AddToInventory(TilePattern pattern)
		{
			if (pattern == null)
				return;
			string facadeId = Mod.FacadeIdFor(pattern.Id);
			string groupId = string.IsNullOrEmpty(pattern.GroupId) ? pattern.Id : pattern.GroupId;
			string subcategoryId = ModSettings.SubcategoryId(groupId);
			EnsureSubcategory(subcategoryId, groupId);
			AddPermit(subcategoryId, facadeId);
		}

		public static void AddAllToInventory()
		{
			foreach (TilePattern pattern in PatternRegistry.Patterns)
				AddToInventory(pattern);
		}

		private static void EnsureSubcategory(string subcategoryId, string groupId)
		{
			if (!InventoryOrganization.subcategoryIdToPermitIdsMap.ContainsKey(subcategoryId))
				InventoryOrganization.subcategoryIdToPermitIdsMap[subcategoryId] = new List<string>();

			if (!InventoryOrganization.subcategoryIdToPresentationDataMap.ContainsKey(subcategoryId))
			{
				Sprite icon = GroupIcon(groupId);
				int sortKey = 800 + ModSettings.GroupRank(groupId);
				InventoryOrganization.subcategoryIdToPresentationDataMap[subcategoryId] =
					new InventoryOrganization.SubcategoryPresentationData(subcategoryId, icon, sortKey);
			}

			if (InventoryOrganization.categoryIdToSubcategoryIdsMap.TryGetValue("WALLPAPERS", out List<string> wallpapers)
				&& !wallpapers.Contains(subcategoryId))
				wallpapers.Add(subcategoryId);

			string display = ModSettings.DisplayName(groupId);
			Strings.Add("STRINGS.UI.KLEI_INVENTORY_SCREEN.SUBCATEGORIES." + subcategoryId.ToUpperInvariant(), display);
		}

		private static void AddPermit(string subcategoryId, string facadeId)
		{
			if (!InventoryOrganization.subcategoryIdToPermitIdsMap.TryGetValue(subcategoryId, out List<string> ids))
				return;
			if (!ids.Contains(facadeId))
				ids.Add(facadeId);

			if (InventoryOrganization.subcategoryIdToPermitIdsMap.TryGetValue("BUILDING_WALLPAPER_PRINTS", out List<string> prints))
				prints.Remove(facadeId);
		}

		private static Sprite GroupIcon(string groupId)
		{
			if (groupId == ModSettings.BuiltinGroupId || groupId == ModSettings.CustomGroupId)
			{
				Sprite wallpaper = Assets.GetSprite("icon_inventory_patterned_wallpapers");
				if (wallpaper != null)
					return wallpaper;
			}

			BuildingDef def = Assets.GetBuildingDef(groupId);
			if (def == null && groupId == ModSettings.StainedGlassGroupId && Assets.BuildingDefs != null)
			{
				foreach (BuildingDef candidate in Assets.BuildingDefs)
				{
					if (candidate != null && ModSettings.IsStainedGlass(candidate.PrefabID))
					{
						def = candidate;
						break;
					}
				}
			}
			if (def != null)
			{
				Sprite sprite = def.GetUISprite();
				if (sprite != null)
					return sprite;
			}

			return Assets.GetSprite("icon_inventory_patterned_wallpapers");
		}
	}
}
