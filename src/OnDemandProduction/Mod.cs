using HarmonyLib;
using KMod;
using UnityEngine;

namespace StockProduction
{
	public class Mod : UserMod2
	{
		public const string LogPrefix = "[OnDemandProduction] ";
		public const string StaticId = "CanisMinor.OnDemandProduction";

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
