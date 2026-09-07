using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Waste
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (Settings.Instance.WasteRoom.IncludeRoom)
				__instance.Add(RoomTypes_AllModded.WasteRoom);
		}

		[HarmonyPatch(typeof(CompostConfig), nameof(CompostConfig.ConfigureBuildingTemplate))]
		public static class CompostConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.CompostBuildingTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(FertilizerMakerConfig), nameof(FertilizerMakerConfig.ConfigureBuildingTemplate))]
		public static class FertilizerMakerConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.WasteProcessorTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(SludgePressConfig), nameof(SludgePressConfig.ConfigureBuildingTemplate))]
		public static class SludgePressConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.WasteProcessorTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}
	}
}
