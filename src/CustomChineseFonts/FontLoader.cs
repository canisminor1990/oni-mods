using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace CustomChineseFonts
{
	public static class FontLoader
	{
		private const string RegularFile = "HarmonyOS_SansSC_Regular.ttf";
		private const string BoldFile = "HarmonyOS_SansSC_Bold.ttf";
		private const string JinzisheFile = "JinzisheDezheng-Regular.otf";

		private static readonly string SeedChars =
			" !\"#$%&'()*+,-./0123456789:;<=>?@" +
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
			"abcdefghijklmnopqrstuvwxyz{|}~" +
			"，。、；：？！「」『』（）【】《》—…·•°℃％" +
			"的一是不了人我在有他这为之大来以个中上们";

		private static bool loadAttempted;

		public static TMP_FontAsset Regular { get; private set; }
		public static TMP_FontAsset Bold { get; private set; }
		public static TMP_FontAsset Jinzishe { get; private set; }
		public static TMP_FontAsset CustomBody { get; private set; }
		public static TMP_FontAsset CustomTitle { get; private set; }

		public static bool IsReady => BodyFont != null;

		public static TMP_FontAsset BodyFont => CustomBody ?? Regular;

		public static TMP_FontAsset TitleFont
		{
			get
			{
				if (ModSettings.UseJinzisheTitle && Jinzishe != null)
					return Jinzishe;
				if (ModSettings.UseCustomTitle && CustomTitle != null)
					return CustomTitle;
				return Bold ?? Regular;
			}
		}

		public static void EnsureLoaded()
		{
			if (loadAttempted)
				return;
			loadAttempted = true;

			ModSettings.EnsureCustomFontsFolder();

			string fontsDir = Path.Combine(Mod.ContentPath ?? "", "fonts");
			Regular = LoadFontFile(Path.Combine(fontsDir, RegularFile), "HarmonyOS Sans SC");
			Bold = LoadFontFile(Path.Combine(fontsDir, BoldFile), "HarmonyOS Sans SC Bold");
			Jinzishe = LoadFontFile(Path.Combine(fontsDir, JinzisheFile), "JinzisheDezheng");
			if (Bold == null)
				Bold = Regular;

			CustomBody = LoadCustomStem("body", "Custom body");
			CustomTitle = LoadCustomStem("title", "Custom title");
			AddFallback(CustomBody, Regular);
			AddFallback(CustomTitle, Regular);

			if (BodyFont != null)
				Debug.Log(Mod.LogPrefix + "loaded HarmonyOS Sans SC Regular/Bold"
					+ (Jinzishe != null ? " + JinzisheDezheng" : "")
					+ (CustomBody != null ? " + custom body" : "")
					+ (CustomTitle != null ? " + custom title" : ""));
			else
				Debug.LogWarning(Mod.LogPrefix + "failed to load HarmonyOS Sans SC from " + fontsDir);
		}

		public static bool IsOurs(TMP_FontAsset font)
		{
			return font != null && (font == Regular || font == Bold || font == Jinzishe
				|| font == CustomBody || font == CustomTitle);
		}

		public static bool IsCjkName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;
			return name.IndexOf("CJK", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("NotoSansCJKsc", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("Source Han", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("SourceHan", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool IsCjkFont(TMP_FontAsset font)
		{
			return font != null && IsCjkName(font.name);
		}

		public static bool IsHeadingName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;
			return name.StartsWith("GRAYSTROKE", StringComparison.OrdinalIgnoreCase)
				|| name.IndexOf("NAME SDF", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("ITEM DROP", StringComparison.OrdinalIgnoreCase) >= 0
				|| name.IndexOf("OUTLINE SDF", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public static bool IsHeadingFont(TMP_FontAsset font)
		{
			if (font == null)
				return false;
			if (font == TitleFont || font == Bold || font == Jinzishe || font == CustomTitle)
				return true;
			return IsHeadingName(font.name);
		}

		public static bool IsGraystroke(TMP_FontAsset font)
		{
			return font != null && IsGraystrokeName(font.name);
		}

		public static bool IsGraystrokeName(string name)
		{
			return !string.IsNullOrEmpty(name)
				&& name.StartsWith("GRAYSTROKE", StringComparison.OrdinalIgnoreCase);
		}

		public static bool ShouldReplace(string fontName, TMP_FontAsset font)
		{
			if (IsOurs(font))
				return false;
			if (IsCjkName(fontName) || IsCjkFont(font))
				return true;
			return ModSettings.ReplaceEnglish;
		}

		public static TMP_FontAsset Pick(string gameFontName)
		{
			if (IsHeadingName(gameFontName))
				return TitleFont;
			if (!string.IsNullOrEmpty(gameFontName)
				&& gameFontName.IndexOf("Bold", StringComparison.OrdinalIgnoreCase) >= 0)
				return TitleFont;
			return BodyFont;
		}

		public static TMP_FontAsset PickFor(TMP_FontAsset gameFont)
		{
			if (gameFont == null)
				return BodyFont;
			if (IsHeadingFont(gameFont))
				return TitleFont;
			return Pick(gameFont.name);
		}

		public static TMP_FontAsset PickFallback(TMP_FontAsset gameFont)
		{
			if (gameFont != null && IsHeadingFont(gameFont))
				return TitleFont;
			return BodyFont;
		}

		private static TMP_FontAsset LoadCustomStem(string stem, string displayName)
		{
			string path = FindCustomFont(stem);
			if (path == null)
				return null;
			return LoadFontFile(path, displayName);
		}

		private static string FindCustomFont(string stem)
		{
			string dir = ModSettings.CustomFontsDir;
			if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
				return null;
			string ttf = Path.Combine(dir, stem + ".ttf");
			if (File.Exists(ttf))
				return ttf;
			string otf = Path.Combine(dir, stem + ".otf");
			if (File.Exists(otf))
				return otf;
			return null;
		}

		private static void AddFallback(TMP_FontAsset font, TMP_FontAsset fallback)
		{
			if (font == null || fallback == null || font == fallback)
				return;
			if (font.fallbackFontAssetTable == null)
				font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
			if (!font.fallbackFontAssetTable.Contains(fallback))
				font.fallbackFontAssetTable.Add(fallback);
		}

		private static TMP_FontAsset LoadFontFile(string fontPath, string displayName)
		{
			try
			{
				if (!File.Exists(fontPath))
				{
					Debug.LogWarning(Mod.LogPrefix + "missing font file: " + fontPath);
					return null;
				}

				TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(
					fontPath, 0, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048);

				if (tmpFont == null)
					tmpFont = CreateFromUnityFont(fontPath);

				if (tmpFont == null)
				{
					Debug.LogWarning(Mod.LogPrefix + "CreateFontAsset failed for " + displayName);
					return null;
				}

				tmpFont.name = displayName;
				tmpFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
				tmpFont.hideFlags = HideFlags.HideAndDontSave;
				UnityEngine.Object.DontDestroyOnLoad(tmpFont);
				ProtectAtlas(tmpFont);
				tmpFont.TryAddCharacters(SeedChars, out _);
				return tmpFont;
			}
			catch (Exception e)
			{
				Debug.LogError(Mod.LogPrefix + "load error " + displayName + ": " + e);
				return null;
			}
		}

		private static TMP_FontAsset CreateFromUnityFont(string fontPath)
		{
			var font = new Font();
			var create = typeof(Font).GetMethod(
				"Internal_CreateFontFromPath",
				BindingFlags.Static | BindingFlags.NonPublic);
			if (create == null)
				return null;
			create.Invoke(null, new object[] { font, fontPath });

			return TMP_FontAsset.CreateFontAsset(
				font, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048,
				AtlasPopulationMode.Dynamic, true);
		}

		private static void ProtectAtlas(TMP_FontAsset tmpFont)
		{
			if (tmpFont.material != null)
				tmpFont.material.hideFlags = HideFlags.HideAndDontSave;
			Texture2D[] atlases = tmpFont.atlasTextures;
			if (atlases == null)
				return;
			for (int i = 0; i < atlases.Length; i++)
			{
				if (atlases[i] != null)
					atlases[i].hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}
}
