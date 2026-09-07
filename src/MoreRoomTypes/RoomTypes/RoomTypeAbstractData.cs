using System;
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

		protected RoomTypeCategory CreateCategory()
		{
			string categoryId = string.Format("{0}Category", Id);
			return new RoomTypeCategory(categoryId, "", Id, "unknown");
		}

		public RoomType GetRoomType()
		{
			return new RoomType(
				Id,
				Name,
				Tooltip,
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
