using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeGymData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "GymRoom";

		public RoomTypeGymData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.GYMROOM.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.GYMROOM.TOOLTIP;
			Effect = string.Format(STRINGS.ROOMS.TYPES.GYMROOM.EFFECT, MiscUtils.Percent(Settings.Instance.Gym.Bonus));
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.RUNNING_WHEEL;
			ConstrantsAdditional = new RoomConstraints.Constraint[4]
			{
				RoomModdedConstraints.WATER_COOLER,
				RoomConstraints.DECORATIVE_ITEM,
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.Gym.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = 0;
			Upgrades = null;
			SingleAssignee = false;
			PriorityUse = false;
			Effects = null;
			SortKey = SortingCounter.GetAndIncrement(SortingCounter.RecreationRoom);
		}
	}
}
