using HarmonyLib;
using KMod;
using UnityEngine;

namespace DuplicantPortraits
{
	public class Mod : UserMod2
	{
		public const string LogPrefix = "[DuplicantPortraits] ";
		public const string StaticId = "DarrenLee.DuplicantPortraits";
		public const string StagePrefix = "DPP_Canvas_";
		public const string KanimSuffix = "_kanim";

		public static Harmony HarmonyInstance;
		public static string ContentPath;

		public override void OnLoad(Harmony harmony)
		{
			HarmonyInstance = harmony;
			ContentPath = path;
			base.OnLoad(harmony);
			Debug.Log(LogPrefix + "loaded, content=" + ContentPath);
		}

		public static string StageIdFor(string personalityId)
		{
			return StagePrefix + personalityId;
		}

		public static string KanimNameFor(string personalityId)
		{
			return StagePrefix + personalityId + KanimSuffix;
		}

		public static bool IsOurStage(string stageId)
		{
			return !string.IsNullOrEmpty(stageId) && stageId.StartsWith(StagePrefix);
		}
	}
}
