using Database;
using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Oxygen
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (Settings.Instance.OxygenRoom.IncludeRoom)
				__instance.Add(RoomTypes_AllModded.OxygenRoom);
		}

		[HarmonyPatch(typeof(ElectrolyzerConfig), nameof(ElectrolyzerConfig.ConfigureBuildingTemplate))]
		public static class ElectrolyzerConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.OxygenProducerTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(MineralDeoxidizerConfig), nameof(MineralDeoxidizerConfig.ConfigureBuildingTemplate))]
		public static class MineralDeoxidizerConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.OxygenProducerTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(RustDeoxidizerConfig), nameof(RustDeoxidizerConfig.ConfigureBuildingTemplate))]
		public static class RustDeoxidizerConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.OxygenProducerTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(GasPumpConfig), nameof(GasPumpConfig.DoPostConfigureComplete))]
		public static class GasPumpConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.GasPumpBuildingTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}

		[HarmonyPatch(typeof(GasMiniPumpConfig), nameof(GasMiniPumpConfig.DoPostConfigureComplete))]
		public static class GasMiniPumpConfig_Patch
		{
			public static void Postfix(GameObject go)
			{
				RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.GasPumpBuildingTag);
				EquipmentEfficiencyTracker.Attach(go);
			}
		}
	}
}
