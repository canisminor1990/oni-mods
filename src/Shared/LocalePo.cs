using KMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CanisMinor.Shared
{
	public static class LocalePo
	{
		public static void Register(Type stringsRoot, string contentPath, string logPrefix, bool writeTemplate = true)
		{
			if (stringsRoot == null)
				return;
			if (string.IsNullOrEmpty(logPrefix))
				logPrefix = "[LocalePo] ";

			Localization.RegisterForTranslation(stringsRoot);
			Load(contentPath, logPrefix);
			LocString.CreateLocStringKeys(stringsRoot, null);

			if (!writeTemplate)
				return;
			try
			{
				string templateDir = Path.Combine(Manager.GetDirectory(), "strings_templates");
				Directory.CreateDirectory(templateDir);
				Localization.GenerateStringsTemplate(stringsRoot, templateDir);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(logPrefix + "failed to write strings template: " + ex.Message);
			}
		}

		public static bool Load(string contentPath, string logPrefix)
		{
			if (string.IsNullOrEmpty(logPrefix))
				logPrefix = "[LocalePo] ";

			string folder = TranslationsFolder(contentPath);
			List<string> codes = LocaleCodes();
			Debug.Log(logPrefix + "i18n folder=" + folder + " codes=" + string.Join(",", codes.ToArray()));

			if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
			{
				Debug.LogWarning(logPrefix + "translations folder missing: " + folder);
				return false;
			}

			foreach (string code in codes)
			{
				string path = Path.Combine(folder, code + ".po");
				if (!File.Exists(path))
					continue;
				try
				{
					Dictionary<string, string> strings = Localization.LoadStringsFile(path, false);
					Localization.OverloadStrings(strings);
					Debug.Log(logPrefix + "loaded translations " + path + " (" + strings.Count + " strings)");
					return true;
				}
				catch (Exception ex)
				{
					Debug.LogWarning(logPrefix + "failed to load " + path + ": " + ex.Message);
				}
			}

			Debug.LogWarning(logPrefix + "no matching .po for locale; tried " + string.Join(",", codes.ToArray()));
			return false;
		}

		public static string TranslationsFolder(string contentPath)
		{
			if (!string.IsNullOrEmpty(contentPath))
			{
				string fromMod = Path.Combine(contentPath, "translations");
				if (Directory.Exists(fromMod))
					return fromMod;
			}

			string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (!string.IsNullOrEmpty(assemblyDir))
				return Path.Combine(assemblyDir, "translations");
			return null;
		}

		public static List<string> LocaleCodes()
		{
			List<string> codes = new List<string>();
			Localization.Locale locale = Localization.GetLocale();
			if (locale != null)
			{
				AddCode(codes, locale.Code);
				AddLanguageId(codes, locale.Lang);
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

			AddSystemLanguage(codes, Application.systemLanguage);
			AddScriptHints(codes);
			return codes;
		}

		private static void AddLanguageId(List<string> codes, Localization.Language lang)
		{
			if (lang == Localization.Language.Chinese)
				AddCode(codes, "zh");
			else if (lang == Localization.Language.Japanese)
				AddCode(codes, "ja");
			else if (lang == Localization.Language.Korean)
				AddCode(codes, "ko");
			else if (lang == Localization.Language.Russian)
				AddCode(codes, "ru");
		}

		private static void AddSystemLanguage(List<string> codes, SystemLanguage sys)
		{
			if (sys == SystemLanguage.Chinese
				|| sys == SystemLanguage.ChineseSimplified
				|| sys == SystemLanguage.ChineseTraditional)
				AddCode(codes, "zh");
			else if (sys == SystemLanguage.Japanese)
				AddCode(codes, "ja");
			else if (sys == SystemLanguage.Korean)
				AddCode(codes, "ko");
			else if (sys == SystemLanguage.Russian)
				AddCode(codes, "ru");
		}

		private static void AddScriptHints(List<string> codes)
		{
			string text = VanillaUiSample();
			if (string.IsNullOrEmpty(text))
				return;
			if (HasRange(text, '\uAC00', '\uD7AF'))
				AddCode(codes, "ko");
			if (HasRange(text, '\u3040', '\u30FF'))
				AddCode(codes, "ja");
			if (HasRange(text, '\u0400', '\u04FF'))
				AddCode(codes, "ru");
			if (HasRange(text, '\u4E00', '\u9FFF') && !HasRange(text, '\u3040', '\u30FF'))
				AddCode(codes, "zh");
		}

		private static string VanillaUiSample()
		{
			try
			{
				return (TryVanilla("STRINGS.UI.FRONTEND.MODS.TITLE") ?? "")
					+ (TryVanilla("STRINGS.UI.FRONTEND.MAINMENU.NEWGAME") ?? "");
			}
			catch
			{
				return null;
			}
		}

		private static string TryVanilla(string key)
		{
			if (!Strings.TryGet(key, out StringEntry entry) || string.IsNullOrEmpty(entry.String))
				return null;
			return entry.String;
		}

		private static bool HasRange(string text, char start, char end)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] >= start && text[i] <= end)
					return true;
			}
			return false;
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
				AddCode(codes, "zh");
			if (normalized == "japanese" || normalized == "jp" || normalized == "ja_jp"
				|| normalized.StartsWith("ja"))
				AddCode(codes, "ja");
			if (normalized == "korean" || normalized == "kor" || normalized == "kr"
				|| normalized == "ko_kr" || normalized.StartsWith("ko"))
				AddCode(codes, "ko");
			if (normalized == "russian" || normalized == "rus" || normalized == "ru_ru"
				|| normalized.StartsWith("ru"))
				AddCode(codes, "ru");
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
