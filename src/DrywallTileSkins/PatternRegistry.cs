using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class PatternRegistry
	{
		public static readonly List<TilePattern> Patterns = new List<TilePattern>();
		private static readonly HashSet<string> RegisteredIds = new HashSet<string>();
		private static bool collected;

		[Serializable]
		private class CustomMeta
		{
			public string Name;
		}

		public static void Collect()
		{
			if (Assets.BuildingDefs == null)
				return;

			ModSettings.DiscoverBuildingGroups();
			int before = Patterns.Count;
			foreach (BuildingDef def in Assets.BuildingDefs)
			{
				if (def == null || def.BlockTileAtlas == null)
					continue;
				if (def.PrefabID == ExteriorWallConfig.ID)
					continue;
				if (def.BuildingComplete != null
					&& def.BuildingComplete.HasTag(TagManager.Create(Mod.NoBackwallTag))
					&& def.PrefabID != TileConfig.ID
					&& !ModSettings.IsStainedGlass(def.PrefabID))
					continue;

				string groupId = ModSettings.ResolveGroupId(def.PrefabID);
				ModSettings.RegisterGroup(groupId, ModSettings.GroupNameFor(def));
				if (!ModSettings.IsGroupEnabled(groupId))
					continue;

				TryAdd(Sanitize(def.PrefabID), def.Name, STRINGS.DRYWALL_TILE_SKINS.FACADE_DESC, TextureUtil.ExtractInterior(def.BlockTileAtlas), def.GetUISprite(), groupId);
			}

			LoadBuiltinWalls();
			LoadCustomWalls();
			TrueTilesIntegration.ImportExisting();
			collected = true;
			Debug.Log("[DrywallTileSkins] collected " + (Patterns.Count - before) + " new patterns, total " + Patterns.Count);
		}

		public static bool HasCollected => collected;

		private static void LoadBuiltinWalls()
		{
			ModSettings.RegisterGroup(ModSettings.BuiltinGroupId, STRINGS.DRYWALL_TILE_SKINS.GROUP_BUILTIN);
			if (!ModSettings.IsGroupEnabled(ModSettings.BuiltinGroupId))
				return;

			List<string> folders = new List<string>();
			if (!string.IsNullOrEmpty(Mod.ContentPath))
				folders.Add(Path.Combine(Mod.ContentPath, "builtin_walls"));

			string assemblyDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
			if (!string.IsNullOrEmpty(assemblyDir))
				folders.Add(Path.Combine(assemblyDir, "builtin_walls"));

			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int loaded = 0;
			foreach (string folder in folders)
			{
				if (!Directory.Exists(folder))
					continue;
				string full = Path.GetFullPath(folder);
				if (!seen.Add(full))
					continue;
				loaded += LoadPngFolder(full, "builtin_", STRINGS.DRYWALL_TILE_SKINS.BUILTIN_DESC, ModSettings.BuiltinGroupId);
			}

			Debug.Log("[DrywallTileSkins] builtin walls: loaded " + loaded + " from " + seen.Count + " folder(s)");
		}

		private static void LoadCustomWalls()
		{
			ModSettings.RegisterGroup(ModSettings.CustomGroupId, STRINGS.DRYWALL_TILE_SKINS.GROUP_CUSTOM);
			if (!ModSettings.IsGroupEnabled(ModSettings.CustomGroupId))
				return;

			string[] folders =
			{
				ModSettings.CustomWallsDir,
				ModSettings.LegacyCustomWallsDir,
				Path.Combine(Util.RootFolder(), "mods", "config", "backwalls", "custom_walls")
			};

			foreach (string folder in folders)
			{
				if (!Directory.Exists(folder))
					continue;
				LoadPngFolder(folder, "custom_", STRINGS.DRYWALL_TILE_SKINS.CUSTOM_DESC, ModSettings.CustomGroupId);
			}
		}

		private static int LoadPngFolder(string folder, string idPrefix, string description, string groupId)
		{
			int added = 0;
			foreach (string png in Directory.GetFiles(folder, "*.png"))
			{
				string stem = Path.GetFileNameWithoutExtension(png);
				string id = idPrefix + Sanitize(stem);
				string name = stem;
				if (groupId == ModSettings.BuiltinGroupId)
				{
					string loc = I18n.BuiltinName(stem);
					if (!string.IsNullOrEmpty(loc))
						name = loc;
				}
				else
				{
					string metaPath = Path.ChangeExtension(png, ".metadata.json");
					if (!File.Exists(metaPath))
						metaPath = Path.Combine(Path.GetDirectoryName(png) ?? folder, stem + ".metadata.json");
					if (File.Exists(metaPath))
					{
						try
						{
							CustomMeta meta = JsonUtility.FromJson<CustomMeta>(File.ReadAllText(metaPath));
							if (meta != null && !string.IsNullOrEmpty(meta.Name))
								name = meta.Name;
						}
						catch (Exception ex)
						{
							Debug.LogWarning("[DrywallTileSkins] metadata parse failed for " + metaPath + ": " + ex.Message);
						}
					}
				}

				Texture2D tex = TextureUtil.LoadPng(png);
				if (tex == null)
					continue;
				int before = Patterns.Count;
				TryAdd(id, name, description, tex, TextureUtil.MakeSprite(tex), groupId);
				if (Patterns.Count > before)
					added++;
			}
			return added;
		}

		public static bool AddPattern(string id, string name, string description, Texture2D interior, Sprite uiSprite, string groupId)
		{
			int before = Patterns.Count;
			TryAdd(id, name, description, interior, uiSprite, groupId);
			return Patterns.Count > before;
		}

		public static string Sanitize(string value)
		{
			if (string.IsNullOrEmpty(value))
				return "pattern";
			return Regex.Replace(value, @"[^A-Za-z0-9_]+", "_");
		}

		private static void TryAdd(string id, string name, string description, Texture2D interior, Sprite uiSprite, string groupId)
		{
			if (string.IsNullOrEmpty(id) || interior == null || !RegisteredIds.Add(id))
				return;
			if (string.IsNullOrEmpty(groupId))
				groupId = id;
			if (!ModSettings.IsGroupEnabled(groupId))
			{
				RegisteredIds.Remove(id);
				return;
			}

			string kanimName = "dts_" + id + "_kanim";
			Texture2D wallpaperUi = WallpaperUi.ComposeTexture(interior);
			KAnimFile kanim = KAnimFactory.Create(kanimName, interior, wallpaperUi);
			if (kanim == null)
			{
				Debug.LogWarning("[DrywallTileSkins] skipped pattern " + id + " because kanim creation failed");
				RegisteredIds.Remove(id);
				return;
			}

			if (wallpaperUi != null)
				uiSprite = TextureUtil.MakeSprite(wallpaperUi);
			else if (uiSprite == null)
				uiSprite = TextureUtil.MakeSprite(interior);

			Patterns.Add(new TilePattern
			{
				Id = id,
				Name = name,
				Description = description,
				GroupId = groupId,
				Interior = interior,
				UISprite = uiSprite,
				KanimName = kanimName,
				Kanim = kanim
			});
		}

		public static TilePattern Find(string facadeId)
		{
			if (!Mod.IsOurFacade(facadeId))
				return null;
			string id = facadeId.Substring(Mod.FacadePrefix.Length);
			for (int i = 0; i < Patterns.Count; i++)
			{
				if (Patterns[i].Id == id)
					return Patterns[i];
			}
			return null;
		}

		public static TilePattern FindByKanim(KAnimFile file)
		{
			if (file == null)
				return null;
			string name = file.name;
			for (int i = 0; i < Patterns.Count; i++)
			{
				TilePattern pattern = Patterns[i];
				if (pattern.Kanim == file || pattern.KanimName == name)
					return pattern;
			}
			return null;
		}
	}
}
