using KMod;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DuplicantPortraits
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

			List<string> codes = LocaleCodes();
			foreach (string code in codes)
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
			if (locale != null)
			{
				AddCode(codes, locale.Code);
				if (locale.Lang == Localization.Language.Chinese)
					AddCode(codes, "zh");
			}

			try
			{
				AddCode(codes, Localization.GetCurrentLanguageCode());
			}
			catch
			{
			}

			List<string> snapshot = new List<string>(codes);
			for (int i = 0; i < snapshot.Count; i++)
				AddAliases(codes, snapshot[i]);

			return codes;
		}

		private static void AddAliases(List<string> codes, string code)
		{
			if (string.IsNullOrEmpty(code))
				return;

			string normalized = code.Replace('-', '_').ToLowerInvariant();
			AddCode(codes, normalized);

			int split = normalized.IndexOf('_');
			if (split > 0)
				AddCode(codes, normalized.Substring(0, split));

			if (normalized == "schinese" || normalized == "tchinese"
				|| normalized == "chs" || normalized == "cht" || normalized == "cn"
				|| normalized == "zh_cn" || normalized == "zh_tw"
				|| normalized == "zh_hans" || normalized == "zh_hant"
				|| normalized.StartsWith("zh"))
			{
				AddCode(codes, "zh");
			}
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
