using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeWaterRoomData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "WaterRoom";

		public RoomTypeWaterRoomData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.WATERROOM.NAME;
			Description = STRINGS.ROOMS.TYPES.WATERROOM.DESCRIPTION;
			Tooltip = STRINGS.ROOMS.TYPES.WATERROOM.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.WATERROOM.EFFECT;
			Catergory = CreateCategory(Db.Get().RoomTypeCategories.Industrial);
			ConstraintPrimary = RoomModdedConstraints.WATER_TREATMENT;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.WaterRoom.MaxSize)
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
