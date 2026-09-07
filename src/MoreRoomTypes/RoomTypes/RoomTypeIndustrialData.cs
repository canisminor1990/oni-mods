using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeIndustrialData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "IndustrialRoom";

		public RoomTypeIndustrialData(RoomType[] upgr = null)
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.INDUSTRIAL.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.INDUSTRIAL.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.INDUSTRIAL.EFFECT;
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.INDUSTRIAL;
			ConstrantsAdditional = new RoomConstraints.Constraint[2]
			{
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.Industrial.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = -1;
			Upgrades = upgr;
			SingleAssignee = true;
			PriorityUse = true;
			Effects = null;
			SortKey = SortingCounter.GetAndIncrement();
		}
	}
}
