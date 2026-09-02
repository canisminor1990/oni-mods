using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class ModSettings
	{
		public const string CustomGroupId = "custom";
		public const string BuiltinGroupId = "builtin";
		public const string StainedGlassGroupId = "DecorPackA_DefaultStainedGlassTile";

		[Serializable]
		public class GroupEntry
		{
			public string id;
			public string name;
			public bool enabled = true;
		}

		[Serializable]
		public class Data
		{
			public List<GroupEntry> groups = new List<GroupEntry>();
		}

		private static Data data;
		private static readonly Dictionary<string, GroupEntry> byId = new Dictionary<string, GroupEntry>(StringComparer.OrdinalIgnoreCase);

		public static Data Current
		{
			get
			{
				EnsureLoaded();
				return data;
			}
		}

		public static string FilePath
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "drywall_tile_skins", "settings.json");
			}
		}

		public static void EnsureLoaded()
		{
			if (data != null)
				return;

			data = new Data();
			byId.Clear();
			try
			{
				if (File.Exists(FilePath))
				{
					Data loaded = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
					if (loaded != null && loaded.groups != null)
						data = loaded;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[DrywallTileSkins] failed to read settings: " + ex.Message);
			}

			Index();
			PruneMergedGroups();
			RegisterGroup(BuiltinGroupId, STRINGS.DRYWALL_TILE_SKINS.GROUP_BUILTIN);
			RegisterGroup(CustomGroupId, STRINGS.DRYWALL_TILE_SKINS.GROUP_CUSTOM);
		}

		public static bool IsStainedGlass(string prefabId)
		{
			return !string.IsNullOrEmpty(prefabId)
				&& prefabId.IndexOf("StainedGlassTile", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static string ResolveGroupId(string prefabId)
		{
			if (IsStainedGlass(prefabId))
				return StainedGlassGroupId;
			return prefabId;
		}

		public static string GroupNameFor(BuildingDef def)
		{
			if (def == null)
				return "";
			if (IsStainedGlass(def.PrefabID))
				return StainedGlassDisplayName();
			return string.IsNullOrEmpty(def.Name) ? def.PrefabID : def.Name;
		}

		public static string StainedGlassDisplayName()
		{
			try
			{
				BuildingDef def = Assets.GetBuildingDef(StainedGlassGroupId);
				if (def != null && !string.IsNullOrEmpty(def.Name))
					return def.Name;
			}
			catch
			{
			}
			return STRINGS.DRYWALL_TILE_SKINS.GROUP_STAINED_GLASS;
		}

		public static bool IsGroupEnabled(string groupId)
		{
			EnsureLoaded();
			if (string.IsNullOrEmpty(groupId))
				return true;
			groupId = ResolveGroupId(groupId);
			if (byId.TryGetValue(groupId, out GroupEntry entry))
				return entry.enabled;
			return true;
		}

		public static void SetGroupEnabled(string groupId, bool enabled)
		{
			EnsureLoaded();
			if (string.IsNullOrEmpty(groupId))
				return;
			groupId = ResolveGroupId(groupId);
			if (!byId.TryGetValue(groupId, out GroupEntry entry))
			{
				entry = new GroupEntry { id = groupId, name = DisplayName(groupId), enabled = enabled };
				data.groups.Add(entry);
				byId[groupId] = entry;
			}
			else
				entry.enabled = enabled;
			Save();
		}

		public static void RegisterGroup(string groupId, string name)
		{
			EnsureLoaded();
			if (string.IsNullOrEmpty(groupId))
				return;
			groupId = ResolveGroupId(groupId);
			if (IsStainedGlass(groupId))
				name = StainedGlassDisplayName();
			if (!byId.TryGetValue(groupId, out GroupEntry entry))
			{
				entry = new GroupEntry { id = groupId, name = name, enabled = true };
				data.groups.Add(entry);
				byId[groupId] = entry;
			}
			else if (!string.IsNullOrEmpty(name) && (IsStainedGlass(groupId) || string.IsNullOrEmpty(entry.name) || entry.name == entry.id))
				entry.name = name;
		}

		public static void DiscoverBuildingGroups()
		{
			EnsureLoaded();
			if (Assets.BuildingDefs == null)
				return;

			bool changed = false;
			foreach (BuildingDef def in Assets.BuildingDefs)
			{
				if (def == null || def.BlockTileAtlas == null)
					continue;
				if (def.PrefabID == ExteriorWallConfig.ID)
					continue;

				string id = ResolveGroupId(def.PrefabID);
				string name = GroupNameFor(def);
				if (!byId.TryGetValue(id, out GroupEntry entry))
				{
					entry = new GroupEntry { id = id, name = name, enabled = true };
					data.groups.Add(entry);
					byId[id] = entry;
					changed = true;
				}
				else if (!IsStainedGlass(id) && !string.IsNullOrEmpty(name))
					entry.name = name;
			}

			RegisterGroup(BuiltinGroupId, STRINGS.DRYWALL_TILE_SKINS.GROUP_BUILTIN);
			RegisterGroup(CustomGroupId, STRINGS.DRYWALL_TILE_SKINS.GROUP_CUSTOM);
			if (PruneMergedGroups())
				changed = true;
			if (changed)
				Save();
		}

		public static List<GroupEntry> SortedGroups()
		{
			EnsureLoaded();
			DiscoverBuildingGroups();
			List<GroupEntry> list = new List<GroupEntry>();
			for (int i = 0; i < data.groups.Count; i++)
			{
				GroupEntry entry = data.groups[i];
				if (entry == null || string.IsNullOrEmpty(entry.id))
					continue;
				if (ResolveGroupId(entry.id) != entry.id)
					continue;
				list.Add(entry);
			}
			list.Sort(CompareGroups);
			return list;
		}

		public static int GroupRank(string groupId)
		{
			if (groupId == "Tile")
				return 0;
			if (groupId == "CarpetTile")
				return 1;
			if (groupId == "InsulationTile")
				return 2;
			if (groupId == "PlasticTile")
				return 3;
			if (groupId == "WoodTile")
				return 4;
			if (groupId == "MetalTile")
				return 5;
			if (groupId == "GlassTile" || groupId == StainedGlassGroupId)
				return 6;
			if (groupId == "MeshTile")
				return 7;
			if (groupId == "GasPermeableMembrane")
				return 8;
			if (groupId == BuiltinGroupId)
				return 9;
			if (groupId == CustomGroupId)
				return 1000;
			return 100;
		}

		public static string SubcategoryId(string groupId)
		{
			return "DTS_WALLPAPER_" + PatternRegistry.Sanitize(groupId).ToUpperInvariant();
		}

		public static string DisplayName(string groupId)
		{
			EnsureLoaded();
			if (groupId == BuiltinGroupId)
				return STRINGS.DRYWALL_TILE_SKINS.GROUP_BUILTIN;
			if (groupId == CustomGroupId)
				return STRINGS.DRYWALL_TILE_SKINS.GROUP_CUSTOM;
			if (groupId == StainedGlassGroupId)
				return StainedGlassDisplayName();
			BuildingDef def = Assets.GetBuildingDef(groupId);
			if (def != null && !string.IsNullOrEmpty(def.Name))
				return def.Name;
			if (byId.TryGetValue(groupId, out GroupEntry entry) && !string.IsNullOrEmpty(entry.name))
				return entry.name;
			return groupId;
		}

		public static void Save()
		{
			EnsureLoaded();
			try
			{
				string dir = Path.GetDirectoryName(FilePath);
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);
				File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[DrywallTileSkins] failed to save settings: " + ex.Message);
			}
		}

		private static bool PruneMergedGroups()
		{
			bool changed = false;
			bool stainedEnabled = true;
			bool sawStained = false;
			for (int i = data.groups.Count - 1; i >= 0; i--)
			{
				GroupEntry entry = data.groups[i];
				if (entry == null || string.IsNullOrEmpty(entry.id))
					continue;
				if (entry.id == StainedGlassGroupId)
				{
					stainedEnabled = entry.enabled;
					sawStained = true;
					continue;
				}
				if (ResolveGroupId(entry.id) == entry.id)
					continue;
				if (IsStainedGlass(entry.id))
				{
					if (!sawStained)
					{
						stainedEnabled = entry.enabled;
						sawStained = true;
					}
					else
						stainedEnabled = stainedEnabled || entry.enabled;
				}
				data.groups.RemoveAt(i);
				byId.Remove(entry.id);
				changed = true;
			}

			if (sawStained)
				RegisterGroup(StainedGlassGroupId, StainedGlassDisplayName());
			if (byId.TryGetValue(StainedGlassGroupId, out GroupEntry stained))
				stained.enabled = stainedEnabled;
			return changed;
		}

		private static void Index()
		{
			byId.Clear();
			if (data.groups == null)
				data.groups = new List<GroupEntry>();
			for (int i = data.groups.Count - 1; i >= 0; i--)
			{
				GroupEntry entry = data.groups[i];
				if (entry == null || string.IsNullOrEmpty(entry.id) || byId.ContainsKey(entry.id))
				{
					data.groups.RemoveAt(i);
					continue;
				}
				byId[entry.id] = entry;
			}
		}

		private static int CompareGroups(GroupEntry a, GroupEntry b)
		{
			int ra = GroupRank(a.id).CompareTo(GroupRank(b.id));
			if (ra != 0)
				return ra;
			return string.Compare(DisplayName(a.id), DisplayName(b.id), StringComparison.OrdinalIgnoreCase);
		}
	}
}
