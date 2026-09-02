using HarmonyLib;
using KMod;
using System.Collections.Generic;
using UnityEngine;

namespace DrywallTileSkins
{
	public class Mod : UserMod2
	{
		public const string FacadePrefix = "DTS_ExteriorWall_";
		public const string NoBackwallTag = "BW_NoBackwall";

		public static Harmony HarmonyInstance;
		public static byte[] WallsAnimBytes;
		public static byte[] WallsBuildBytes;
		public static Texture2D WallsTexture;
		public static KAnimFile WallsKanim;
		public static string ContentPath;

		public override void OnLoad(Harmony harmony)
		{
			HarmonyInstance = harmony;
			base.OnLoad(harmony);
			ContentPath = path;
			ModSettings.EnsureLoaded();
			Debug.Log("[DrywallTileSkins] loaded, content=" + ContentPath);
		}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<KMod.Mod> mods)
		{
			base.OnAllModsLoaded(harmony, mods);
			TrueTilesIntegration.Patch(harmony);
			PatternRegistry.Collect();
			FacadeRegistrar.RegisterPending("OnAllModsLoaded");
		}

		public static bool IsOurFacade(string facadeId)
		{
			return !string.IsNullOrEmpty(facadeId) && facadeId.StartsWith(FacadePrefix);
		}

		public static string FacadeIdFor(string patternId)
		{
			return FacadePrefix + patternId;
		}
	}
}
