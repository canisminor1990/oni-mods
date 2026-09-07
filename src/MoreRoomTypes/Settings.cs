using UnityEngine;

namespace MoreRoomTypes
{
	public class Settings
	{
		public class RoomSettings
		{
			public bool IncludeRoom;
			public int MaxSize;
			public float Bonus;
			public Color32 RoomColor;

			public RoomSettings(bool include, int max, Color32 color, float bonus)
			{
				IncludeRoom = include;
				MaxSize = max;
				Bonus = bonus;
				RoomColor = color;
			}
		}

		public class PlainRoomSettings
		{
			public bool IncludeRoom;
			public int MaxSize;
			public Color32 RoomColor;

			public PlainRoomSettings(bool include, int max, Color32 color)
			{
				IncludeRoom = include;
				MaxSize = max;
				RoomColor = color;
			}
		}

		static Settings _instance;

		public static Settings Instance
		{
			get
			{
				if (_instance == null)
					_instance = new Settings();
				return _instance;
			}
		}

		public Settings()
		{
			HideLegendEffect = true;

			Industrial = new PlainRoomSettings(true, 96, ColorPalette.RoomIndustrial);
			Graveyard = new RoomSettings(true, 96, ColorPalette.RoomPark, 0.2f);
			Gym = new RoomSettings(true, 64, ColorPalette.RoomRecreation, 0.1f);
			Museum = new RoomSettings(true, 120, ColorPalette.RoomHospital, 0.3f);
			MuseumSpace = new RoomSettings(true, 120, ColorPalette.RoomRecreation, 0.3f);
			PrivateBathroom = new PlainRoomSettings(true, 32, ColorPalette.RoomBathroom);
			Warehouse = new PlainRoomSettings(true, 120, ColorPalette.RoomWarehouse);
			BatteryRoom = new PlainRoomSettings(true, 120, ColorPalette.RoomBattery);
			OxygenRoom = new PlainRoomSettings(true, 64, ColorPalette.RoomOxygen);
			WasteRoom = new PlainRoomSettings(true, 64, ColorPalette.RoomWaste);
			WaterRoom = new PlainRoomSettings(true, 64, ColorPalette.RoomWater);
			NuclearPlant = new PlainRoomSettings(true, 120, ColorPalette.RoomNuclear);
			Hallway = new PlainRoomSettings(true, 240, ColorPalette.RoomHallway);

			ResizeMinRoomSize12 = 12;
			ResizeMinRoomSize24 = 24;
			ResizeMinRoomSize32 = 32;
			ResizeMaxRoomSize64 = 64;
			ResizeMaxRoomSize96 = 96;
			ResizeMaxRoomSize120 = 120;
		}

		public bool HideLegendEffect;
		public PlainRoomSettings Industrial;
		public RoomSettings Graveyard;
		public RoomSettings Gym;
		public RoomSettings Museum;
		public RoomSettings MuseumSpace;
		public PlainRoomSettings PrivateBathroom;
		public PlainRoomSettings Warehouse;
		public PlainRoomSettings BatteryRoom;
		public PlainRoomSettings OxygenRoom;
		public PlainRoomSettings WasteRoom;
		public PlainRoomSettings WaterRoom;
		public PlainRoomSettings NuclearPlant;
		public PlainRoomSettings Hallway;
		public int ResizeMinRoomSize12;
		public int ResizeMinRoomSize24;
		public int ResizeMinRoomSize32;
		public int ResizeMaxRoomSize64;
		public int ResizeMaxRoomSize96;
		public int ResizeMaxRoomSize120;

		public int GetMaxRoomSize()
		{
			int max = 128;
			max = System.Math.Max(max, ResizeMaxRoomSize64);
			max = System.Math.Max(max, ResizeMaxRoomSize96);
			max = System.Math.Max(max, ResizeMaxRoomSize120);
			max = System.Math.Max(max, Industrial.MaxSize);
			max = System.Math.Max(max, Graveyard.MaxSize);
			max = System.Math.Max(max, Gym.MaxSize);
			max = System.Math.Max(max, Museum.MaxSize);
			max = System.Math.Max(max, MuseumSpace.MaxSize);
			max = System.Math.Max(max, PrivateBathroom.MaxSize);
			max = System.Math.Max(max, Warehouse.MaxSize);
			max = System.Math.Max(max, BatteryRoom.MaxSize);
			max = System.Math.Max(max, OxygenRoom.MaxSize);
			max = System.Math.Max(max, WasteRoom.MaxSize);
			max = System.Math.Max(max, WaterRoom.MaxSize);
			max = System.Math.Max(max, NuclearPlant.MaxSize);
			max = System.Math.Max(max, Hallway.MaxSize);
			return max;
		}
	}
}
