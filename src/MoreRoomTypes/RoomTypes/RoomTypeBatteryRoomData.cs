using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeBatteryRoomData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "BatteryRoom";

		public RoomTypeBatteryRoomData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.BATTERYROOM.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.BATTERYROOM.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.BATTERYROOM.EFFECT;
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.BATTERIES;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomConstraints.MINIMUM_SIZE_24,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.BatteryRoom.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = 1;
			SortKey = SortingCounter.GetAndIncrement(SortingCounter.PowerPlantSortKey);
		}
	}
}
