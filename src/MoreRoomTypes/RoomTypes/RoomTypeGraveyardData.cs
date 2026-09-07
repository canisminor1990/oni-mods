using System;
using STRINGS;

namespace MoreRoomTypes
{
	public class RoomTypeGraveyardData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "GraveyardRoom";
		public static readonly string EffectId = "GraveyardEffectId";

		public RoomTypeGraveyardData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.GRAVEYARD.NAME;
			Description = STRINGS.ROOMS.TYPES.GRAVEYARD.DESCRIPTION;
			Tooltip = STRINGS.ROOMS.TYPES.GRAVEYARD.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.GRAVEYARD.EFFECT;
			Catergory = CreateCategory(Db.Get().RoomTypeCategories.Park);
			ConstraintPrimary = RoomModdedConstraints.GRAVESTONE;
			ConstrantsAdditional = new RoomConstraints.Constraint[4]
			{
				RoomConstraints.WILDPLANT,
				RoomConstraints.NO_INDUSTRIAL_MACHINERY,
				RoomConstraints.MINIMUM_SIZE_12,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.Graveyard.MaxSize)
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
			SortKey = SortingCounter.GetAndIncrement(SortingCounter.ParkSortKey);
		}
	}
}
