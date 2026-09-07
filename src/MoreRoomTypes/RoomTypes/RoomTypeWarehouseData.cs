using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeWarehouseData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "WarehouseRoom";

		public RoomTypeWarehouseData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.WAREHOUSE.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.WAREHOUSE.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.WAREHOUSE.EFFECT;
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.STORAGE_BUILDINGS;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomConstraints.NO_INDUSTRIAL_MACHINERY,
				RoomConstraints.MINIMUM_SIZE_24,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.Warehouse.MaxSize)
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
