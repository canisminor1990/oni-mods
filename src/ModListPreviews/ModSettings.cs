using System;
using System.IO;
using UnityEngine;

namespace ModListPreviews
{
	internal static class ModSettings
	{
		public const int MinThumbSize = 32;
		public const int MaxThumbSize = 128;
		public const int DefaultThumbSize = 56;
		public const int SizeStep = 8;

		[Serializable]
		public class Data
		{
			public int thumbSize = DefaultThumbSize;
		}

		private static Data data;

		public static int ThumbSize
		{
			get
			{
				EnsureLoaded();
				return ClampSize(data.thumbSize);
			}
		}

		public static string FilePath
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "ModListPreviews", "settings.json");
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

			data.thumbSize = ClampSize(data.thumbSize);
		}

		public static void ChangeSize(int delta)
		{
			EnsureLoaded();
			int next = ClampSize(data.thumbSize + delta);
			if (next == data.thumbSize)
				return;
			data.thumbSize = next;
			Save();
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

		private static int ClampSize(int size)
		{
			if (size < MinThumbSize)
				return MinThumbSize;
			if (size > MaxThumbSize)
				return MaxThumbSize;
			return size;
		}
	}
}
