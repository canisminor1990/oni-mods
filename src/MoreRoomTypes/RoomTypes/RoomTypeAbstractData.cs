using System.Collections.Generic;
using STRINGS;

namespace MoreRoomTypes
{
	public abstract class RoomTypeAbstractData
	{
		public string Id;
		public string Name;
		public string Description;
		public string Tooltip;
		public string Effect;
		public RoomTypeCategory Catergory;
		public RoomConstraints.Constraint ConstraintPrimary;
		public RoomConstraints.Constraint[] ConstrantsAdditional;
		public RoomDetails.Detail[] RoomDetails;
		public int Priority;
		public RoomType[] Upgrades;
		public bool SingleAssignee;
		public bool PriorityUse;
		public string[] Effects;
		public int SortKey;

		static HashSet<string> _moddedIds;

		static HashSet<string> ModdedIds
		{
			get
			{
				if (_moddedIds == null)
				{
					_moddedIds = new HashSet<string>
					{
						RoomTypePrivateBathroomData.RoomId,
						RoomTypeWarehouseData.RoomId,
						RoomTypeBatteryRoomData.RoomId,
						RoomTypeWasteRoomData.RoomId,
						RoomTypeWaterRoomData.RoomId,
						RoomTypeNuclearPlantData.RoomId,
						RoomTypeHallwayData.RoomId,
						RoomTypeGymData.RoomId,
						RoomTypeMuseumData.RoomId,
						RoomTypeMuseumSpaceData.RoomId,
						RoomTypeGraveyardData.RoomId
					};
				}
				return _moddedIds;
			}
		}

		protected RoomTypeCategory CreateCategory(RoomTypeCategory vanilla)
		{
			if (vanilla == null)
				return new RoomTypeCategory(string.Format("{0}Category", Id), "", Id, "unknown");
			return new RoomTypeCategory(vanilla.Id, vanilla.Name, Id, vanilla.icon);
		}

		public static bool IsModdedRoom(RoomType room)
		{
			return room != null && ModdedIds.Contains(room.Id);
		}

		public RoomType GetRoomType()
		{
			string description = string.IsNullOrEmpty(Description) ? Tooltip : Description;
			return new RoomType(
				Id,
				Name,
				description,
				Tooltip,
				Effect,
				Catergory,
				ConstraintPrimary,
				ConstrantsAdditional,
				RoomDetails,
				Priority,
				Upgrades,
				SingleAssignee,
				PriorityUse,
				Effects,
				SortKey);
		}
	}
}
