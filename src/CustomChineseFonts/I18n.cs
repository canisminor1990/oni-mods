using KMod;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomChineseFonts
{
	internal static class I18n
	{
		public static void Register()
		{
			Localization.RegisterForTranslation(typeof(STRINGS));
			LoadTranslations();
			LocString.CreateLocStringKeys(typeof(STRINGS), null);

			try
			{
				string templateDir = Path.Combine(Manager.GetDirectory(), "strings_templates");
				Directory.CreateDirectory(templateDir);
				Localization.GenerateStringsTemplate(typeof(STRINGS), templateDir);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "failed to write strings template: " + ex.Message);
			}
		}

		private static void LoadTranslations()
		{
			string folder = TranslationsFolder();
			if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
				return;

			foreach (string code in LocaleCodes())
			{
				string path = Path.Combine(folder, code + ".po");
				if (!File.Exists(path))
					continue;
				try
				{
					Dictionary<string, string> strings = Localization.LoadStringsFile(path, false);
					Localization.OverloadStrings(strings);
					Debug.Log(Mod.LogPrefix + "loaded translations " + path);
					return;
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "failed to load " + path + ": " + ex.Message);
				}
			}
		}

		private static string TranslationsFolder()
		{
			if (!string.IsNullOrEmpty(Mod.ContentPath))
			{
				string fromMod = Path.Combine(Mod.ContentPath, "translations");
				if (Directory.Exists(fromMod))
					return fromMod;
			}

			string assemblyDir = Path.GetDirectoryName(typeof(Mod).Assembly.Location);
			if (!string.IsNullOrEmpty(assemblyDir))
				return Path.Combine(assemblyDir, "translations");
			return null;
		}

		private static List<string> LocaleCodes()
		{
			List<string> codes = new List<string>();
			Localization.Locale locale = Localization.GetLocale();
			if (locale != null && !string.IsNullOrEmpty(locale.Code))
			{
				AddCode(codes, locale.Code);
				AddCode(codes, locale.Code.Replace('-', '_'));
				int dash = locale.Code.IndexOf('-');
				int under = locale.Code.IndexOf('_');
				int split = dash >= 0 ? dash : under;
				if (split > 0)
					AddCode(codes, locale.Code.Substring(0, split));
				if (locale.Lang == Localization.Language.Chinese)
					AddCode(codes, "zh");
			}
			return codes;
		}

		private static void AddCode(List<string> codes, string code)
		{
			if (string.IsNullOrEmpty(code))
				return;
			for (int i = 0; i < codes.Count; i++)
			{
				if (string.Equals(codes[i], code, StringComparison.OrdinalIgnoreCase))
					return;
			}
			codes.Add(code);
		}
	}
}
