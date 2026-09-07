using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Warehouse
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (Settings.Instance.Warehouse.IncludeRoom)
				__instance.Add(RoomTypes_AllModded.Warehouse);
		}

		[HarmonyPatch(typeof(StorageLockerConfig), nameof(StorageLockerConfig.ConfigureBuildingTemplate))]
		public static class StorageLockerConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.StorageBuildingTag);
		}

		[HarmonyPatch(typeof(StorageLockerSmartConfig), nameof(StorageLockerSmartConfig.DoPostConfigureComplete))]
		public static class StorageLockerSmartConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.StorageBuildingTag);
		}
	}
}
