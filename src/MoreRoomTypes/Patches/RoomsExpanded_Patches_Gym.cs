using HarmonyLib;
using UnityEngine;
using Klei.AI;
using TUNING;
using Database;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Gym
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (!Settings.Instance.Gym.IncludeRoom)
				return;

			if (__instance.PowerPlant.primary_constraint.stomp_in_conflict == null)
				__instance.PowerPlant.primary_constraint.stomp_in_conflict = new System.Collections.Generic.List<RoomConstraints.Constraint>();
			__instance.PowerPlant.primary_constraint.stomp_in_conflict.Add(RoomTypes_AllModded.GymRoom.primary_constraint);

			__instance.Add(RoomTypes_AllModded.GymRoom);
		}

		[HarmonyPatch(typeof(ManualGeneratorConfig))]
		[HarmonyPatch("DoPostConfigureComplete")]
		public static class ManualGeneratorConfig_DoPostConfigureComplete_Patch
		{
			public static void Postfix(GameObject go)
			{
				if (!Settings.Instance.Gym.IncludeRoom)
					return;
				go.GetComponent<KPrefabID>().AddTag(RoomConstraintTags.RunningWheelGeneratorTag);
			}
		}

		[HarmonyPatch(typeof(ManualGenerator))]
		[HarmonyPatch("OnWorkTick")]
		public static class ManualGenerator_OnWorkTick_Patch
		{
			static string AthleticsId = "";

			public static void Postfix(ref WorkerBase worker, float dt, ref ManualGenerator __instance)
			{
				if (!Settings.Instance.Gym.IncludeRoom)
					return;
				if (!RoomTypes_AllModded.IsInTheRoom(__instance, RoomTypeGymData.RoomId))
					return;

				if (string.IsNullOrEmpty(AthleticsId))
					AthleticsId = Db.Get().Attributes.Athletics.Id;

				AttributeLevels component = worker.GetComponent<AttributeLevels>();
				if (component != null)
				{
					component.AddExperience(
						AthleticsId,
						dt * Settings.Instance.Gym.Bonus,
						DUPLICANTSTATS.ATTRIBUTE_LEVELING.ALL_DAY_EXPERIENCE);
				}
			}
		}

		[HarmonyPatch(typeof(WaterCoolerConfig))]
		[HarmonyPatch("ConfigureBuildingTemplate")]
		public static class WaterCoolerConfig_ConfigureBuildingTemplate_Patch
		{
			public static void Postfix(GameObject go)
			{
				if (!Settings.Instance.Gym.IncludeRoom)
					return;
				go.GetComponent<KPrefabID>().AddTag(RoomConstraintTags.WaterCoolerTag);
			}
		}
	}
}
