using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CustomChineseFonts
{
	public static class Patches
	{
		private static readonly HashSet<TextStyleSetting> HeadingStyles = new HashSet<TextStyleSetting>();
		private static readonly HashSet<LocText> HeadingTexts = new HashSet<LocText>();

		[HarmonyPatch(typeof(Localization), nameof(Localization.GetFont))]
		public static class Localization_GetFont_Patch
		{
			public static void Postfix(string fontname, ref TMP_FontAsset __result)
			{
				try
				{
					FontLoader.EnsureLoaded();
					if (!FontLoader.IsReady)
						return;
					if (!FontLoader.ShouldReplace(fontname, __result))
						return;
					__result = FontLoader.Pick(fontname);
				}
				catch (Exception e)
				{
					Debug.LogWarning(Mod.LogPrefix + "GetFont patch: " + e.Message);
				}
			}
		}

		[HarmonyPatch(typeof(Localization), nameof(Localization.SwapToLocalizedFont), new Type[] { })]
		public static class Localization_SwapToLocalizedFont_NoArg_Patch
		{
			public static void Prefix()
			{
				CaptureHeadings();
			}

			public static void Postfix()
			{
				Apply();
			}
		}

		[HarmonyPatch(typeof(Localization), nameof(Localization.SwapToLocalizedFont), new Type[] { typeof(string) })]
		public static class Localization_SwapToLocalizedFont_Patch
		{
			public static void Prefix()
			{
				CaptureHeadings();
			}

			public static void Postfix()
			{
				Apply();
			}
		}

		[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
		public static class Db_Initialize_Patch
		{
			public static void Prefix()
			{
				CaptureHeadings();
			}

			public static void Postfix()
			{
				Apply();
			}
		}

		[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
		public static class Localization_Initialize_Patch
		{
			public static void Postfix()
			{
				I18n.Register();
			}
		}

		[HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
		public static class ModsScreen_BuildDisplay_Patch
		{
			public static void Postfix(object __instance)
			{
				try
				{
					SettingsScreen.AddModButton(__instance);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "ModsScreen button failed: " + ex.Message);
				}
			}
		}

		internal static void CaptureHeadings()
		{
			try
			{
				FontLoader.EnsureLoaded();
				TextStyleSetting[] styles = Resources.FindObjectsOfTypeAll<TextStyleSetting>();
				for (int i = 0; i < styles.Length; i++)
				{
					TextStyleSetting style = styles[i];
					if (style?.sdfFont != null && FontLoader.IsHeadingFont(style.sdfFont))
						HeadingStyles.Add(style);
				}

				LocText[] texts = Resources.FindObjectsOfTypeAll<LocText>();
				for (int i = 0; i < texts.Length; i++)
				{
					LocText locText = texts[i];
					if (locText?.font != null && FontLoader.IsHeadingFont(locText.font))
						HeadingTexts.Add(locText);
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning(Mod.LogPrefix + "CaptureHeadings: " + e.Message);
			}
		}

		internal static void Apply()
		{
			try
			{
				ModSettings.EnsureLoaded();
				FontLoader.EnsureLoaded();
				if (!FontLoader.IsReady)
					return;

				InjectFallbacks();
				RestoreHeadingFonts();
				ReplaceStyleFonts();
				ReplaceLocTexts();
				SetDefaultFontAsset();
			}
			catch (Exception e)
			{
				Debug.LogWarning(Mod.LogPrefix + "Apply: " + e);
			}
		}

		private static void InjectFallbacks()
		{
			if (ModSettings.ReplaceEnglish)
				return;

			TMP_FontAsset title = FontLoader.TitleFont;
			if (title == null)
				return;

			TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			int count = 0;
			for (int i = 0; i < fonts.Length; i++)
			{
				TMP_FontAsset gameFont = fonts[i];
				if (gameFont == null || FontLoader.IsOurs(gameFont) || !FontLoader.IsHeadingName(gameFont.name))
					continue;

				List<TMP_FontAsset> table = gameFont.fallbackFontAssetTable;
				if (table == null)
				{
					gameFont.fallbackFontAssetTable = new List<TMP_FontAsset> { title };
					count++;
					continue;
				}
				if (table.Contains(title))
					continue;
				table.Insert(0, title);
				count++;
			}
			if (count > 0)
				Debug.Log(Mod.LogPrefix + "added title fallback to " + count + " heading fonts");
		}

		private static void RestoreHeadingFonts()
		{
			TMP_FontAsset title = FontLoader.TitleFont;
			if (title == null)
				return;

			bool replaceEnglish = ModSettings.ReplaceEnglish;
			int styleCount = 0;
			foreach (TextStyleSetting style in HeadingStyles)
			{
				if (style == null)
					continue;
				if (!replaceEnglish && FontLoader.IsGraystroke(style.sdfFont))
					continue;
				if (style.sdfFont == title)
					continue;
				style.sdfFont = title;
				styleCount++;
			}

			int textCount = 0;
			foreach (LocText locText in HeadingTexts)
			{
				if (locText == null)
					continue;
				if (!replaceEnglish && FontLoader.IsGraystroke(locText.font))
					continue;
				if (locText.font == title)
					continue;
				locText.font = title;
				textCount++;
			}

			if (styleCount > 0 || textCount > 0)
				Debug.Log(Mod.LogPrefix + "applied title font to " + styleCount + " heading styles, " + textCount + " LocTexts");
		}

		private static void ReplaceStyleFonts()
		{
			TextStyleSetting[] styles = Resources.FindObjectsOfTypeAll<TextStyleSetting>();
			int count = 0;
			for (int i = 0; i < styles.Length; i++)
			{
				TextStyleSetting style = styles[i];
				if (style == null || style.sdfFont == null)
					continue;
				if (HeadingStyles.Contains(style) || FontLoader.IsHeadingFont(style.sdfFont))
				{
					if (ModSettings.ReplaceEnglish || !FontLoader.IsGraystroke(style.sdfFont))
					{
						if (style.sdfFont != FontLoader.TitleFont)
						{
							style.sdfFont = FontLoader.TitleFont;
							count++;
						}
					}
					continue;
				}
				if (!FontLoader.ShouldReplace(style.sdfFont.name, style.sdfFont))
					continue;
				TMP_FontAsset next = FontLoader.PickFor(style.sdfFont);
				if (style.sdfFont == next)
					continue;
				style.sdfFont = next;
				count++;
			}
			if (count > 0)
				Debug.Log(Mod.LogPrefix + "replaced " + count + " TextStyleSettings");
		}

		private static void ReplaceLocTexts()
		{
			LocText[] texts = Resources.FindObjectsOfTypeAll<LocText>();
			int count = 0;
			for (int i = 0; i < texts.Length; i++)
			{
				LocText locText = texts[i];
				if (locText == null)
					continue;
				TMP_FontAsset current = locText.font;
				if (current == null)
					continue;
				if (HeadingTexts.Contains(locText) || FontLoader.IsHeadingFont(current))
				{
					if (ModSettings.ReplaceEnglish || !FontLoader.IsGraystroke(current))
					{
						if (current != FontLoader.TitleFont)
						{
							locText.font = FontLoader.TitleFont;
							count++;
						}
					}
					continue;
				}
				if (!FontLoader.ShouldReplace(current.name, current))
					continue;
				TMP_FontAsset next = FontLoader.PickFor(current);
				if (current == next)
					continue;
				locText.font = next;
				count++;
			}
			if (count > 0)
				Debug.Log(Mod.LogPrefix + "replaced " + count + " LocTexts");
		}

		private static void SetDefaultFontAsset()
		{
			if (FontLoader.Regular == null)
				return;
			FieldInfo field = typeof(Localization).GetField(
				"sFontAsset", BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null)
				return;
			TMP_FontAsset current = field.GetValue(null) as TMP_FontAsset;
			if (!FontLoader.ShouldReplace(current != null ? current.name : null, current)
				&& !FontLoader.IsOurs(current))
				return;
			if (FontLoader.IsOurs(current) && current == FontLoader.Regular)
				return;
			if (!ModSettings.ReplaceEnglish && current != null && !FontLoader.IsCjkFont(current) && !FontLoader.IsOurs(current))
				return;
			field.SetValue(null, FontLoader.Regular);
		}
	}
}
