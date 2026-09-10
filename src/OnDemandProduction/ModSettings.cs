using System;
using System.IO;
using UnityEngine;

namespace StockProduction
{
	internal static class ModSettings
	{
		[Serializable]
		public class Data
		{
			public bool extraBuildingsOff;
		}

		private static Data data;

		public static string FilePath
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "OnDemandProduction", "settings.json");
			}
		}

		public static bool ExtraBuildings
		{
			get
			{
				EnsureLoaded();
				return !data.extraBuildingsOff;
			}
			set
			{
				EnsureLoaded();
				bool off = !value;
				if (data.extraBuildingsOff == off)
					return;
				data.extraBuildingsOff = off;
				Save();
				BuildingStockController.NotifySettingsChanged();
			}
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
	}
}
