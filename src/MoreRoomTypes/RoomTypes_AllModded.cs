namespace MoreRoomTypes
{
	class RoomTypes_AllModded
	{
		static RoomType prv_Graveyard;
		static RoomType prv_gym;
		static RoomType prv_Museum;
		static RoomType prv_MuseumSpace;
		static RoomType prv_PrivateBathroom;
		static RoomType prv_Warehouse;
		static RoomType prv_Battery;
		static RoomType prv_Waste;
		static RoomType prv_Water;
		static RoomType prv_Nuclear;
		static RoomType prv_Hallway;

		public static RoomType GraveyardRoom
		{
			get
			{
				if (prv_Graveyard == null)
					prv_Graveyard = new RoomTypeGraveyardData().GetRoomType();
				return prv_Graveyard;
			}
		}

		public static RoomType GymRoom
		{
			get
			{
				if (prv_gym == null)
					prv_gym = new RoomTypeGymData().GetRoomType();
				return prv_gym;
			}
		}

		public static RoomType Museum
		{
			get
			{
				if (prv_Museum == null)
					prv_Museum = new RoomTypeMuseumData().GetRoomType();
				return prv_Museum;
			}
		}

		public static RoomType MuseumSpace
		{
			get
			{
				if (prv_MuseumSpace == null)
					prv_MuseumSpace = new RoomTypeMuseumSpaceData().GetRoomType();
				return prv_MuseumSpace;
			}
		}

		public static RoomType PrivateBathroom
		{
			get
			{
				if (prv_PrivateBathroom == null)
					prv_PrivateBathroom = new RoomTypePrivateBathroomData().GetRoomType();
				return prv_PrivateBathroom;
			}
		}

		public static RoomType Warehouse
		{
			get
			{
				if (prv_Warehouse == null)
					prv_Warehouse = new RoomTypeWarehouseData().GetRoomType();
				return prv_Warehouse;
			}
		}

		public static RoomType BatteryRoom
		{
			get
			{
				if (prv_Battery == null)
					prv_Battery = new RoomTypeBatteryRoomData().GetRoomType();
				return prv_Battery;
			}
		}

		public static RoomType WasteRoom
		{
			get
			{
				if (prv_Waste == null)
					prv_Waste = new RoomTypeWasteRoomData().GetRoomType();
				return prv_Waste;
			}
		}

		public static RoomType WaterRoom
		{
			get
			{
				if (prv_Water == null)
					prv_Water = new RoomTypeWaterRoomData().GetRoomType();
				return prv_Water;
			}
		}

		public static RoomType Hallway
		{
			get
			{
				if (prv_Hallway == null)
					prv_Hallway = new RoomTypeHallwayData().GetRoomType();
				return prv_Hallway;
			}
		}

		public static RoomType NuclearPlant
		{
			get
			{
				if (prv_Nuclear == null)
					prv_Nuclear = new RoomTypeNuclearPlantData().GetRoomType();
				return prv_Nuclear;
			}
		}

		public static bool IsInTheRoom(KMonoBehaviour item, string roomId)
		{
			if (item == null || Game.Instance == null || Game.Instance.roomProber == null)
				return false;
			CavityInfo info = Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(item));
			if (info == null || info.room == null || info.room.roomType == null)
				return false;
			return info.room.roomType.Id == roomId;
		}
	}
}
