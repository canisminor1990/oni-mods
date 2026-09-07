using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Battery
	{
		public const float LossFactor = 0.5f;

		public static void AddRoom(ref RoomTypes __instance)
		{
			if (!Settings.Instance.BatteryRoom.IncludeRoom)
				return;

			__instance.Add(RoomTypes_AllModded.BatteryRoom);
			RoomConstraintTags.AddStompInConflict(RoomTypes_AllModded.BatteryRoom, __instance.PowerPlant);
		}

		public static void ApplyLateStomps(RoomTypes db)
		{
			if (!Settings.Instance.BatteryRoom.IncludeRoom)
				return;
			RoomConstraintTags.AddStompInConflict(
				RoomTypes_AllModded.BatteryRoom,
				RoomConstraintTags.FindRoomType(db, RoomTypeIndustrialData.RoomId));
		}

		[HarmonyPatch(typeof(BatteryConfig), nameof(BatteryConfig.ConfigureBuildingTemplate))]
		public static class BatteryConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.BatteryBuildingTag);
		}

		[HarmonyPatch(typeof(BatteryMediumConfig), nameof(BatteryMediumConfig.ConfigureBuildingTemplate))]
		public static class BatteryMediumConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.BatteryBuildingTag);
		}

		[HarmonyPatch(typeof(BatterySmartConfig), nameof(BatterySmartConfig.ConfigureBuildingTemplate))]
		public static class BatterySmartConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.BatteryBuildingTag);
		}

		[HarmonyPatch(typeof(BatteryModuleConfig), nameof(BatteryModuleConfig.DoPostConfigureComplete))]
		public static class BatteryModuleConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.BatteryBuildingTag);
		}

		[HarmonyPatch(typeof(Battery), nameof(Battery.EnergySim200ms))]
		public static class Battery_EnergySim200ms_Patch
		{
			public static void Prefix(Battery __instance, ref float __state)
			{
				__state = __instance.joulesLostPerSecond;
				if (RoomTypes_AllModded.IsInTheRoom(__instance, RoomTypeBatteryRoomData.RoomId))
					__instance.joulesLostPerSecond *= LossFactor;
			}

			public static void Postfix(Battery __instance, float __state)
			{
				__instance.joulesLostPerSecond = __state;
			}
		}
	}
}
