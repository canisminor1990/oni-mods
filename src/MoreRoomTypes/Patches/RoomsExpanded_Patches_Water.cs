using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Water
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (Settings.Instance.WaterRoom.IncludeRoom)
				__instance.Add(RoomTypes_AllModded.WaterRoom);
		}

		[HarmonyPatch(typeof(WaterPurifierConfig), nameof(WaterPurifierConfig.ConfigureBuildingTemplate))]
		public static class WaterPurifierConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.WaterTreatmentTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(DesalinatorConfig), nameof(DesalinatorConfig.ConfigureBuildingTemplate))]
		public static class DesalinatorConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.WaterTreatmentTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}
	}
}
