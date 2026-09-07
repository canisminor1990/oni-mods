using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Nuclear
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (!Settings.Instance.NuclearPlant.IncludeRoom)
				return;

			__instance.Add(RoomTypes_AllModded.NuclearPlant);
			RoomConstraintTags.AddStompInConflict(RoomTypes_AllModded.NuclearPlant, __instance.PowerPlant);
		}

		[HarmonyPatch(typeof(NuclearReactorConfig), nameof(NuclearReactorConfig.ConfigureBuildingTemplate))]
		public static class NuclearReactorConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.NuclearReactorTag);
		}

		[HarmonyPatch(typeof(SteamTurbineConfig2), nameof(SteamTurbineConfig2.ConfigureBuildingTemplate))]
		public static class SteamTurbineConfig2_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.SteamTurbineTag);
		}

		[HarmonyPatch(typeof(SteamTurbineConfig), nameof(SteamTurbineConfig.DoPostConfigureComplete))]
		public static class SteamTurbineConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.SteamTurbineTag);
		}

		[HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.IsInCorrectRoom))]
		public static class RoomTracker_IsInCorrectRoom_Patch
		{
			public static void Postfix(RoomTracker __instance, ref bool __result)
			{
				if (__result || !Settings.Instance.NuclearPlant.IncludeRoom)
					return;
				if (__instance.room == null || __instance.room.roomType == null)
					return;
				if (__instance.requiredRoomType != Db.Get().RoomTypes.PowerPlant.Id)
					return;
				if (__instance.room.roomType.Id == RoomTypeNuclearPlantData.RoomId)
					__result = true;
			}
		}

		[HarmonyPatch(typeof(RoomTracker), "OnUpdateRoom")]
		public static class RoomTracker_OnUpdateRoom_Patch
		{
			public static void Prefix(RoomTracker __instance, object data, ref string __state)
			{
				__state = null;
				if (!Settings.Instance.NuclearPlant.IncludeRoom)
					return;
				Room room = data as Room;
				if (room == null || room.roomType == null)
					return;
				if (__instance.requiredRoomType != Db.Get().RoomTypes.PowerPlant.Id)
					return;
				if (room.roomType.Id != RoomTypeNuclearPlantData.RoomId)
					return;
				__state = __instance.requiredRoomType;
				__instance.requiredRoomType = room.roomType.Id;
			}

			public static void Postfix(RoomTracker __instance, string __state)
			{
				if (__state != null)
					__instance.requiredRoomType = __state;
			}
		}
	}
}
