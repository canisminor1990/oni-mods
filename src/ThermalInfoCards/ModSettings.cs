using System;
using System.IO;
using UnityEngine;

namespace ThermalInfoCards
{
	internal static class ModSettings
	{
		[Serializable]
		public class Data
		{
			public bool allUnits;
			public bool onlyOnThermalOverlay;
			public int infoCardOpacity = 80;
			public bool hideElementCategories;
			public bool useBaseSelection;
			public bool forceFirstSelectionToHover = true;
			public float temperatureBandWidth = 10f;
			public bool overrideCardSize = true;
			public int fontSizeChange = -2;
			public int lineSpacing = 3;
			public int iconSizeChange = -3;
			public int yPadding = 6;
			public int settingsVersion;
		}

		private static Data data;
		private static bool displayAllTempsChecked;
		private static bool displayAllTempsPresent;

		public static string FilePath
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "ThermalInfoCards", "settings.json");
			}
		}

		public static bool AllUnits
		{
			get
			{
				EnsureLoaded();
				if (HasDisplayAllTemps())
					return false;
				return data.allUnits;
			}
			set { Set(ref data.allUnits, value); }
		}

		public static bool AllUnitsStored
		{
			get
			{
				EnsureLoaded();
				return data.allUnits;
			}
		}

		public static bool OnlyOnThermalOverlay
		{
			get { EnsureLoaded(); return data.onlyOnThermalOverlay; }
			set { Set(ref data.onlyOnThermalOverlay, value); }
		}

		public static int InfoCardOpacity
		{
			get { EnsureLoaded(); return data.infoCardOpacity; }
		}

		public static bool HideElementCategories
		{
			get { EnsureLoaded(); return data.hideElementCategories; }
			set { Set(ref data.hideElementCategories, value); }
		}

		public static bool UseBaseSelection
		{
			get { EnsureLoaded(); return data.useBaseSelection; }
			set { Set(ref data.useBaseSelection, value); }
		}

		public static bool ForceFirstSelectionToHover
		{
			get { EnsureLoaded(); return data.forceFirstSelectionToHover; }
			set { Set(ref data.forceFirstSelectionToHover, value); }
		}

		public static float TemperatureBandWidth
		{
			get { EnsureLoaded(); return data.temperatureBandWidth; }
		}

		public static bool OverrideCardSize
		{
			get { EnsureLoaded(); return data.overrideCardSize; }
			set { Set(ref data.overrideCardSize, value); }
		}

		public static int FontSizeChange
		{
			get { EnsureLoaded(); return data.fontSizeChange; }
		}

		public static int LineSpacing
		{
			get { EnsureLoaded(); return data.lineSpacing; }
		}

		public static int IconSizeChange
		{
			get { EnsureLoaded(); return data.iconSizeChange; }
		}

		public static int YPadding
		{
			get { EnsureLoaded(); return data.yPadding; }
		}

		public static void EnsureLoaded()
		{
			if (data != null)
				return;

			data = new Data();
			try
			{
				if (File.Exists(FilePath))
				{
					Data loaded = JsonUtility.FromJson<Data>(File.ReadAllText(FilePath));
					if (loaded != null)
						data = loaded;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to read settings: " + ex.Message);
			}

			if (data.settingsVersion < 2)
			{
				if (data.infoCardOpacity <= 0)
					data.infoCardOpacity = 80;
				if (data.temperatureBandWidth <= 0f)
					data.temperatureBandWidth = 10f;
				if (data.lineSpacing <= 0)
					data.lineSpacing = 3;
				if (data.yPadding <= 0)
					data.yPadding = 6;
				data.settingsVersion = 2;
				Save();
			}
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
				Debug.LogWarning(Mod.LogPrefix + "failed to save settings: " + ex.Message);
			}
		}

		private static void Set(ref bool field, bool value)
		{
			EnsureLoaded();
			if (field == value)
				return;
			field = value;
			Save();
		}

		private static bool HasDisplayAllTemps()
		{
			if (displayAllTempsChecked)
				return displayAllTempsPresent;
			displayAllTempsChecked = true;
			displayAllTempsPresent = Type.GetType("DisplayAllTemps.State, DisplayAllTemps") != null;
			if (displayAllTempsPresent)
				Debug.Log(Mod.LogPrefix + "DisplayAllTemps compatibility: all-units display disabled");
			return displayAllTempsPresent;
		}
	}
}
