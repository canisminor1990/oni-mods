using System;
using STRINGS;

namespace MoreRoomTypes
{
	class RoomTypeNuclearPlantData : RoomTypeAbstractData
	{
		public static readonly string RoomId = "NuclearPlantRoom";

		public RoomTypeNuclearPlantData()
		{
			Id = RoomId;
			Name = STRINGS.ROOMS.TYPES.NUCLEARPLANT.NAME;
			Tooltip = STRINGS.ROOMS.TYPES.NUCLEARPLANT.TOOLTIP;
			Effect = STRINGS.ROOMS.TYPES.NUCLEARPLANT.EFFECT;
			Catergory = CreateCategory();
			ConstraintPrimary = RoomModdedConstraints.NUCLEAR_REACTOR;
			ConstrantsAdditional = new RoomConstraints.Constraint[]
			{
				RoomModdedConstraints.STEAM_TURBINE,
				RoomConstraints.MINIMUM_SIZE_24,
				RoomConstraintTags.GetMaxSizeConstraint(Settings.Instance.NuclearPlant.MaxSize)
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
