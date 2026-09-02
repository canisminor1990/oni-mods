using HarmonyLib;
using KMod;
using UnityEngine;

namespace ModListPreviews
{
	public class Mod : UserMod2
	{
		public const string LogPrefix = "[ModListPreviews] ";
		public const string StaticId = "DarrenLee.ModListPreviews";

		public static Harmony HarmonyInstance;
		public static string ContentPath;

		public override void OnLoad(Harmony harmony)
		{
			HarmonyInstance = harmony;
			ContentPath = path;
			base.OnLoad(harmony);
			Debug.Log(LogPrefix + "loaded, content=" + ContentPath);
		}
	}
}
