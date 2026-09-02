using HarmonyLib;
using KMod;
using UnityEngine;

namespace CustomChineseFonts
{
	public class Mod : UserMod2
	{
		public const string LogPrefix = "[CustomChineseFonts] ";
		public const string StaticId = "DarrenLee.CustomChineseFonts";

		public static Harmony HarmonyInstance;
		public static string ContentPath;

		public override void OnLoad(Harmony harmony)
		{
			HarmonyInstance = harmony;
			ContentPath = path;
			base.OnLoad(harmony);
			ModSettings.EnsureLoaded();
			Debug.Log(LogPrefix + "loaded, content=" + ContentPath);
			Debug.Log(LogPrefix + "uses HarmonyOS Sans Fonts.");
		}
	}
}
