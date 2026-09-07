using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using static Localization;

namespace MoreRoomTypes
{
	class RoomsExpanded_Translation_Patches
	{
		[HarmonyPatch(typeof(Localization), "Initialize")]
		public class Localization_Initialize_Patch
		{
			public static void Postfix() => Translate(typeof(STRINGS));

			public static void Translate(Type root)
			{
				RegisterForTranslation(root);
				LoadStrings();
				LocString.CreateLocStringKeys(root, null);
				GenerateStringsTemplate(root, GetTranslationDir());
				Debug.Log($"{Mod.Namespace}: using translation done by {STRINGS.TRANSLATION.AUTHOR.NAME}");
			}

			static string GetTranslationDir()
			{
				string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "translations");
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				return dir;
			}

			static void LoadStrings()
			{
				string code = GetLocale()?.Code;
				if (string.IsNullOrEmpty(code))
					code = GetCurrentLanguageCode();

				string path = Path.Combine(GetTranslationDir(), code + ".po");
				Debug.Log($"{Mod.Namespace}: Loading translation file: {path}");
				if (File.Exists(path))
				{
					OverloadStrings(LoadStringsFile(path, false));
					return;
				}

				string zhPath = Path.Combine(GetTranslationDir(), "zh.po");
				if (!string.Equals(code, "zh", StringComparison.OrdinalIgnoreCase) && File.Exists(zhPath))
				{
					Debug.Log($"{Mod.Namespace}: locale '{code}' missing; falling back to zh.po");
					OverloadStrings(LoadStringsFile(zhPath, false));
					return;
				}

				Debug.Log($"{Mod.Namespace}: Translation file not found, using default strings.");
			}
		}
	}
}
