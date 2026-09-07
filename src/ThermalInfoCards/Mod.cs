using HarmonyLib;
using KMod;
using UnityEngine;

namespace ThermalInfoCards
{
	public class Mod : UserMod2
	{
		public const string LogPrefix = "[ThermalInfoCards] ";
		public const string StaticId = "DarrenLee.ThermalInfoCards";

		public static Harmony HarmonyInstance;
		public static string ContentPath;

		public override void OnLoad(Harmony harmony)
		{
			HarmonyInstance = harmony;
			ContentPath = path;
			try
			{
				ModSettings.EnsureLoaded();
				base.OnLoad(harmony);
				Debug.Log(LogPrefix + "loaded, content=" + ContentPath);
			}
			catch (System.Exception ex)
			{
				Debug.LogError(LogPrefix + "OnLoad failed: " + ex);
			}
		}
	}
}
