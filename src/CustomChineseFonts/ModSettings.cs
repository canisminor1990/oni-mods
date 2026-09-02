using System;
using System.IO;
using UnityEngine;

namespace CustomChineseFonts
{
	internal static class ModSettings
	{
		public const string TitleHarmonyOS = "harmonyos_bold";
		public const string TitleJinzishe = "jinzishe";
		public const string TitleCustom = "custom";

		[Serializable]
		public class Data
		{
			public bool replaceEnglish;
			public string titleFont = TitleHarmonyOS;
		}

		private static Data data;

		public static Data Current
		{
			get
			{
				EnsureLoaded();
				return data;
			}
		}

		public static bool ReplaceEnglish
		{
			get { return Current.replaceEnglish; }
			set
			{
				EnsureLoaded();
				if (data.replaceEnglish == value)
					return;
				data.replaceEnglish = value;
				Save();
			}
		}

		public static string TitleFontId
		{
			get
			{
				EnsureLoaded();
				string id = NormalizeTitle(data.titleFont);
				if (id == TitleCustom && FontLoader.CustomTitle == null)
					return TitleHarmonyOS;
				return id;
			}
			set
			{
				EnsureLoaded();
				string next = NormalizeTitle(value);
				if (data.titleFont == next)
					return;
				data.titleFont = next;
				Save();
			}
		}

		public static bool UseJinzisheTitle
		{
			get { return TitleFontId == TitleJinzishe; }
		}

		public static bool UseCustomTitle
		{
			get { return TitleFontId == TitleCustom; }
		}

		public static string ConfigDir
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "CustomChineseFonts");
			}
		}

		public static string CustomFontsDir
		{
			get
			{
				return Path.Combine(ConfigDir, "custom_fonts");
			}
		}

		public static string FilePath
		{
			get
			{
				return Path.Combine(ConfigDir, "settings.json");
			}
		}

		private static string LegacyFilePath
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "HarmonyOSSansSC", "settings.json");
			}
		}

		public static void EnsureLoaded()
		{
			if (data != null)
				return;

			data = new Data();
			try
			{
				string path = File.Exists(FilePath) ? FilePath : LegacyFilePath;
				if (File.Exists(path))
				{
					Data loaded = JsonUtility.FromJson<Data>(File.ReadAllText(path));
					if (loaded != null)
						data = loaded;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to read settings: " + ex.Message);
			}

			data.titleFont = NormalizeTitle(data.titleFont);
			if (!File.Exists(FilePath) && File.Exists(LegacyFilePath))
				Save();
			EnsureCustomFontsFolder();
		}

		public static void EnsureCustomFontsFolder()
		{
			try
			{
				Directory.CreateDirectory(CustomFontsDir);
				string readme = Path.Combine(CustomFontsDir, "README.txt");
				if (!File.Exists(readme))
				{
					File.WriteAllText(readme,
						"把 TTF 或 OTF 字体放到这个文件夹，然后完全退出游戏再进。\n" +
						"\n" +
						"body.ttf / body.otf   替换中文正文（默认 HarmonyOS Sans SC Regular）\n" +
						"title.ttf / title.otf 出现在模组设置的标题字体选项里，名称是「自定义」\n" +
						"\n" +
						"Put a TTF or OTF font here, then fully quit the game and relaunch.\n" +
						"\n" +
						"body.ttf / body.otf   replaces Chinese body text\n" +
						"title.ttf / title.otf extra title option named Custom\n");
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to create custom fonts folder: " + ex.Message);
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

		private static string NormalizeTitle(string id)
		{
			if (string.Equals(id, TitleJinzishe, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(id, "dezheng", StringComparison.OrdinalIgnoreCase))
				return TitleJinzishe;
			if (string.Equals(id, TitleCustom, StringComparison.OrdinalIgnoreCase))
				return TitleCustom;
			return TitleHarmonyOS;
		}
	}
}
