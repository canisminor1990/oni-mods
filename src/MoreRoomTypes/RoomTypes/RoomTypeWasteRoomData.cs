using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeWasteRoomData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "WasteRoom";

		public RoomTypeWasteRoomData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.WASTEROOM.NAME;
			Description = STRINGS.ROOMS.TYPES.WASTEROOM.DESCRIPTION;
			Tooltip = STRINGS.ROOMS.TYPES.WASTEROOM.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.WASTEROOM.EFFECT;
			Catergory = CreateCategory(Db.Get().RoomTypeCategories.Industrial);
			ConstraintPrimary = RoomModdedConstraints.COMPOST;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomModdedConstraints.WASTE_PROCESSOR,
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.WasteRoom.MaxSize)
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
