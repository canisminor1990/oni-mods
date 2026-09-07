namespace MoreRoomTypes
{
	public static class SortingCounter
	{
		public static readonly int PlumbedBathroomSortKey = 2;
		public static readonly int PowerPlantSortKey = 12;
		public static readonly int RecreationRoom = 17;
		public static readonly int ParkSortKey = 18;
		public static readonly int MuseumSortKey = ParkSortKey + 2;

		static int value;

		public static void Init(int startingValue = 16)
		{
			value = startingValue;
		}

		public static int GetAndIncrement(int forced = -1)
		{
			value++;
			if (forced != -1)
				return forced;
			return value;
		}
	}
}
