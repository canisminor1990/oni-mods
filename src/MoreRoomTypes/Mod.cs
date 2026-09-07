using HarmonyLib;
using KMod;
using UnityEngine;

namespace MoreRoomTypes
{
	public class Mod : UserMod2
	{
		public const string StaticId = "CanisMinor.MoreRoomTypes";

		public static string Namespace { get; private set; }
		public static Harmony HarmonyInstance;
		public static string ContentPath;

		public override void OnLoad(Harmony harmony)
		{
			HarmonyInstance = harmony;
			Namespace = GetType().Namespace;
			ContentPath = path;
			Settings.EnsureLoaded();
			base.OnLoad(harmony);

			Debug.Log($"{Namespace}: Loaded from: {ContentPath}");
			if (mod != null && mod.packagedModInfo != null)
			{
				Debug.Log($"{Namespace}: Mod version: {mod.packagedModInfo.version} " +
					$"supporting game build {mod.packagedModInfo.minimumSupportedBuild}");
			}
		}
	}
}
