using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace MoreRoomTypes
{
	public class RoomConstraintTags
	{
		public static Tag GravestoneTag = nameof(GravestoneTag).ToTag();
		public static Tag WaterCoolerTag = nameof(WaterCoolerTag).ToTag();
		public static Tag RunningWheelGeneratorTag = nameof(RunningWheelGeneratorTag).ToTag();
		public static Tag ItemPedestalTag = nameof(ItemPedestalTag).ToTag();
		public static Tag StorageBuildingTag = nameof(StorageBuildingTag).ToTag();
		public static Tag SinkBuildingTag = nameof(SinkBuildingTag).ToTag();
		public static Tag ShowerBuildingTag = nameof(ShowerBuildingTag).ToTag();
		public static Tag BatteryBuildingTag = nameof(BatteryBuildingTag).ToTag();
		public static Tag OxygenProducerTag = nameof(OxygenProducerTag).ToTag();
		public static Tag GasPumpBuildingTag = nameof(GasPumpBuildingTag).ToTag();
		public static Tag CompostBuildingTag = nameof(CompostBuildingTag).ToTag();
		public static Tag WasteProcessorTag = nameof(WasteProcessorTag).ToTag();
		public static Tag WaterTreatmentTag = nameof(WaterTreatmentTag).ToTag();
		public static Tag NuclearReactorTag = nameof(NuclearReactorTag).ToTag();
		public static Tag SteamTurbineTag = nameof(SteamTurbineTag).ToTag();
		public static Tag LadderBuildingTag = nameof(LadderBuildingTag).ToTag();
		public static Tag FirePoleBuildingTag = nameof(FirePoleBuildingTag).ToTag();

		public static void AddBuildingTag(GameObject go, Tag tag)
		{
			if (go == null)
				return;
			KPrefabID id = go.GetComponent<KPrefabID>();
			if (id != null)
				id.AddTag(tag);
		}

		public static bool HasBuildingTag(Component component, Tag tag)
		{
			if (component == null)
				return false;
			KPrefabID id = component.GetComponent<KPrefabID>();
			return id != null && id.HasTag(tag);
		}

		public static void ToggleEffect(Klei.AI.Effects target, string effectId, bool shouldHave)
		{
			if (target == null)
				return;
			if (shouldHave)
			{
				if (!target.HasEffect(effectId))
					target.Add(effectId, true);
			}
			else if (target.HasEffect(effectId))
				target.Remove(effectId);
		}

		public static int CountBuildings(Room room, Tag tag)
		{
			int count = 0;
			if (room == null || room.buildings == null)
				return 0;
			for (int i = 0; i < room.buildings.Count; i++)
			{
				KPrefabID building = room.buildings[i];
				if (building != null && building.HasTag(tag))
					count++;
			}
			return count;
		}

		public static RoomConstraints.Constraint GetMaxSizeConstraint(int maxSize)
		{
			if (maxSize == 32)
			{
				return new RoomConstraints.Constraint(
					(Func<KPrefabID, bool>)null,
					(Func<Room, bool>)(room => room.cavity.NumCells <= 32),
					name: string.Format((string)ROOMS.CRITERIA.MAXIMUM_SIZE.NAME, "32"),
					description: string.Format((string)ROOMS.CRITERIA.MAXIMUM_SIZE.DESCRIPTION, "32"));
			}
			if (maxSize == 64)
				return RoomConstraints.MAXIMUM_SIZE_64;
			if (maxSize == 96)
				return RoomConstraints.MAXIMUM_SIZE_96;
			if (maxSize == 120)
				return RoomConstraints.MAXIMUM_SIZE_120;

			return new RoomConstraints.Constraint(
				(Func<KPrefabID, bool>)null,
				(Func<Room, bool>)(room => room.cavity.NumCells <= maxSize),
				name: string.Format((string)ROOMS.CRITERIA.MAXIMUM_SIZE.NAME, maxSize.ToString()),
				description: string.Format((string)ROOMS.CRITERIA.MAXIMUM_SIZE.DESCRIPTION, maxSize.ToString()));
		}

		public static RoomConstraints.Constraint GetMinSizeConstraint(int minSize)
		{
			if (minSize == 12)
				return RoomConstraints.MINIMUM_SIZE_12;
			if (minSize == 24)
				return RoomConstraints.MINIMUM_SIZE_24;
			if (minSize == 32)
				return RoomConstraints.MINIMUM_SIZE_32;

			return new RoomConstraints.Constraint(
				(Func<KPrefabID, bool>)null,
				(Func<Room, bool>)(room => room.cavity.NumCells >= minSize),
				name: string.Format((string)ROOMS.CRITERIA.MINIMUM_SIZE.NAME, minSize.ToString()),
				description: string.Format((string)ROOMS.CRITERIA.MINIMUM_SIZE.DESCRIPTION, minSize.ToString()));
		}

		public static void AddStompInConflict(RoomConstraints.Constraint stomping, RoomConstraints.Constraint stomped)
		{
			if (stomping == null || stomped == null)
				return;
			if (stomping.stomp_in_conflict == null)
				stomping.stomp_in_conflict = new List<RoomConstraints.Constraint>();
			stomping.stomp_in_conflict.Add(stomped);
		}

		public static void AddStompInConflict(RoomType stomping, RoomType stomped)
		{
			if (stomping == null || stomped == null || stomping.primary_constraint == null || stomped.primary_constraint == null)
				return;
			AddStompInConflict(stomping.primary_constraint, stomped.primary_constraint);
		}

		public static RoomType FindRoomType(Database.RoomTypes db, string id)
		{
			if (db == null || string.IsNullOrEmpty(id))
				return null;
			for (int i = 0; i < db.Count; i++)
			{
				if (db[i] != null && db[i].Id == id)
					return db[i];
			}
			return null;
		}

		public static void ResizeRooms(ref Database.RoomTypes __instance)
		{
			for (int i = 0; i < __instance.Count; i++)
			{
				if (__instance[i] == null || __instance[i].additional_constraints == null)
					continue;

				for (int add = 0; add < __instance[i].additional_constraints.Length; add++)
				{
					if (__instance[i].additional_constraints[add] == RoomConstraints.MINIMUM_SIZE_12)
						__instance[i].additional_constraints[add] = GetMinSizeConstraint(Settings.Instance.ResizeMinRoomSize12);
					else if (__instance[i].additional_constraints[add] == RoomConstraints.MINIMUM_SIZE_24)
						__instance[i].additional_constraints[add] = GetMinSizeConstraint(Settings.Instance.ResizeMinRoomSize24);
					else if (__instance[i].additional_constraints[add] == RoomConstraints.MINIMUM_SIZE_32)
						__instance[i].additional_constraints[add] = GetMinSizeConstraint(Settings.Instance.ResizeMinRoomSize32);
					else if (__instance[i].additional_constraints[add] == RoomConstraints.MAXIMUM_SIZE_64)
						__instance[i].additional_constraints[add] = GetMaxSizeConstraint(Settings.Instance.ResizeMaxRoomSize64);
					else if (__instance[i].additional_constraints[add] == RoomConstraints.MAXIMUM_SIZE_96)
						__instance[i].additional_constraints[add] = GetMaxSizeConstraint(Settings.Instance.ResizeMaxRoomSize96);
					else if (__instance[i].additional_constraints[add] == RoomConstraints.MAXIMUM_SIZE_120)
						__instance[i].additional_constraints[add] = GetMaxSizeConstraint(Settings.Instance.ResizeMaxRoomSize120);
				}
			}
		}
	}
}
