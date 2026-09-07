using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeHallwayData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "HallwayRoom";
		public static readonly string EffectId = "HallwayTransit";

		public RoomTypeHallwayData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.HALLWAY.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.HALLWAY.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.HALLWAY.EFFECT;
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.LADDER;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomModdedConstraints.FIRE_POLE,
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.Hallway.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = -2;
			SortKey = SortingCounter.GetAndIncrement();
		}
	}
}
