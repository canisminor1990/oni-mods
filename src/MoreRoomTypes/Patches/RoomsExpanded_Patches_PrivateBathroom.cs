using System;
using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_PrivateBathroom
	{
		public const float ShowerTimeBonus = 0.2f;
		public const string VanillaBathroomEffectId = "RoomBathroom";

		public static void AddRoom(ref RoomTypes __instance)
		{
			if (!Settings.Instance.PrivateBathroom.IncludeRoom)
				return;

			__instance.Add(RoomTypes_AllModded.PrivateBathroom);
			RoomConstraintTags.AddStompInConflict(RoomTypes_AllModded.PrivateBathroom, __instance.PlumbedBathroom);
			RoomConstraintTags.AddStompInConflict(RoomTypes_AllModded.PrivateBathroom, __instance.Latrine);
			try
			{
				Traverse.Create(__instance.Latrine).Property("upgrade_paths").SetValue(new RoomType[] { __instance.PlumbedBathroom, RoomTypes_AllModded.PrivateBathroom });
				Traverse.Create(__instance.PlumbedBathroom).Property("upgrade_paths").SetValue(new RoomType[] { RoomTypes_AllModded.PrivateBathroom });
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"{Mod.Namespace}: failed to wire bathroom upgrades: {ex.Message}");
			}
		}

		[HarmonyPatch(typeof(WashSinkConfig), nameof(WashSinkConfig.ConfigureBuildingTemplate))]
		public static class WashSinkConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.SinkBuildingTag);
		}

		[HarmonyPatch(typeof(ShowerConfig), nameof(ShowerConfig.ConfigureBuildingTemplate))]
		public static class ShowerConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.ShowerBuildingTag);
		}

		[HarmonyPatch(typeof(Shower), "OnWorkTick")]
		public static class Shower_OnWorkTick_Patch
		{
			public static void Postfix(Shower __instance, float dt)
			{
				if (!Settings.Instance.PrivateBathroom.IncludeRoom)
					return;
				if (!RoomTypes_AllModded.IsInTheRoom(__instance, RoomTypePrivateBathroomData.RoomId))
					return;
				__instance.WorkTimeRemaining -= dt * ShowerTimeBonus;
			}
		}
	}
}
