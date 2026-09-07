using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeOxygenRoomData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "OxygenRoom";

		public RoomTypeOxygenRoomData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.OXYGENROOM.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.OXYGENROOM.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.OXYGENROOM.EFFECT;
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.OXYGEN_PRODUCER;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomModdedConstraints.GAS_PUMP,
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.OxygenRoom.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = 0;
			SortKey = SortingCounter.GetAndIncrement();
		}
	}
}
