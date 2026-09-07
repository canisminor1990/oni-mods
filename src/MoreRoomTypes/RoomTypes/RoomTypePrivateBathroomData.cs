using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypePrivateBathroomData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "PrivateBathroomRoom";

		public RoomTypePrivateBathroomData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.PRIVATEBATHROOM.NAME;
			Description = STRINGS.ROOMS.TYPES.PRIVATEBATHROOM.DESCRIPTION;
			Tooltip = STRINGS.ROOMS.TYPES.PRIVATEBATHROOM.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.PRIVATEBATHROOM.EFFECT;
			Catergory = CreateCategory(Db.Get().RoomTypeCategories.Bathroom);
			ConstraintPrimary = RoomModdedConstraints.FLUSH_TOILET_EXACTLY_ONE;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomModdedConstraints.SINK_EXACTLY_ONE,
				RoomModdedConstraints.SHOWER_EXACTLY_ONE,
				RoomConstraints.DECORATIVE_ITEM,
				RoomConstraints.IS_BACKWALLED,
				RoomConstraints.NO_OUTHOUSES,
				RoomConstraints.NO_INDUSTRIAL_MACHINERY,
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.PrivateBathroom.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = 2;
			Upgrades = null;
			SingleAssignee = true;
			PriorityUse = false;
			Effects = new string[]
			{
				RoomsExpanded_Patches_PrivateBathroom.ExtraMoraleEffectId
			};
			SortKey = SortingCounter.GetAndIncrement(SortingCounter.PlumbedBathroomSortKey);
		}
	}
}
