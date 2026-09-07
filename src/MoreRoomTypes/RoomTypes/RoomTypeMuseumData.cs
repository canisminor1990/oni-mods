using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeMuseumData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "MuseumRoom";
		public static readonly string EffectId = "MuseumEffectId";

		public RoomTypeMuseumData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.MUSEUM.NAME;
			Description = STRINGS.ROOMS.TYPES.MUSEUM.DESCRIPTION;
			Tooltip = STRINGS.ROOMS.TYPES.MUSEUM.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.MUSEUM.EFFECT;
			Catergory = CreateCategory(Db.Get().RoomTypeCategories.Recreation);
			ConstraintPrimary = RoomModdedConstraints.PEDESTAL;
			ConstrantsAdditional = new RoomConstraints.Constraint[5]
			{
				RoomModdedConstraints.MASTERPIECES,
				RoomConstraints.ORNAMENTDISPLAYED,
				RoomConstraints.NO_INDUSTRIAL_MACHINERY,
				RoomConstraints.MINIMUM_SIZE_32,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.Museum.MaxSize)
			};
			RoomDetails = new RoomDetails.Detail[2]
			{
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.SIZE.NAME, room.cavity.NumCells)),
				new RoomDetails.Detail(room => string.Format((string)ROOMS.DETAILS.BUILDING_COUNT.NAME, room.buildings.Count))
			};
			Priority = 1;
			Upgrades = new RoomType[] { RoomTypes_AllModded.MuseumSpace };
			SingleAssignee = false;
			PriorityUse = false;
			Effects = null;
			SortKey = SortingCounter.GetAndIncrement(SortingCounter.MuseumSortKey);
		}
	}
}
