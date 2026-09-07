using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MoreRoomTypes
{
	public class Settings
	{
		public const string IdGraveyard = "graveyard";
		public const string IdGym = "gym";
		public const string IdMuseum = "museum";
		public const string IdMuseumSpace = "museumSpace";
		public const string IdPrivateBathroom = "privateBathroom";
		public const string IdWarehouse = "warehouse";
		public const string IdBatteryRoom = "batteryRoom";
		public const string IdWasteRoom = "wasteRoom";
		public const string IdWaterRoom = "waterRoom";
		public const string IdNuclearPlant = "nuclearPlant";
		public const string IdHallway = "hallway";

		public static readonly string[] ToggleIds =
		{
			IdPrivateBathroom,
			IdWarehouse,
			IdBatteryRoom,
			IdWasteRoom,
			IdWaterRoom,
			IdNuclearPlant,
			IdHallway,
			IdGym,
			IdMuseum,
			IdMuseumSpace,
			IdGraveyard
		};

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

		[Serializable]
		class FileData
		{
			public string[] disabled = new string[0];
		}

		static Settings _instance;
		FileData file = new FileData();

		public static Settings Instance
		{
			get
			{
				EnsureLoaded();
				return _instance;
			}
		}

		public static void EnsureLoaded()
		{
			if (_instance != null)
				return;
			_instance = new Settings();
			_instance.LoadFile();
		}

		public static string FilePath
		{
			get
			{
				return Path.Combine(Util.RootFolder(), "mods", "config", "MoreRoomTypes", "settings.json");
			}
		}

		Settings()
		{
			HideLegendEffect = false;

			Graveyard = new RoomSettings(true, 96, ColorPalette.Park, 0.2f);
			Gym = new RoomSettings(true, 64, ColorPalette.Recreation, 0.1f);
			Museum = new RoomSettings(true, 120, ColorPalette.Museum, 0.3f);
			MuseumSpace = new RoomSettings(true, 120, ColorPalette.Museum, 0.3f);
			PrivateBathroom = new PlainRoomSettings(true, 32, ColorPalette.Bathroom);
			Warehouse = new PlainRoomSettings(true, 120, ColorPalette.Industrial);
			BatteryRoom = new PlainRoomSettings(true, 120, ColorPalette.Industrial);
			WasteRoom = new PlainRoomSettings(true, 64, ColorPalette.Utility);
			WaterRoom = new PlainRoomSettings(true, 64, ColorPalette.Utility);
			NuclearPlant = new PlainRoomSettings(true, 120, ColorPalette.Industrial);
			Hallway = new PlainRoomSettings(true, 240, ColorPalette.Recreation);

			ResizeMinRoomSize12 = 12;
			ResizeMinRoomSize24 = 24;
			ResizeMinRoomSize32 = 32;
			ResizeMaxRoomSize64 = 64;
			ResizeMaxRoomSize96 = 96;
			ResizeMaxRoomSize120 = 120;
		}

		public bool HideLegendEffect;
		public RoomSettings Graveyard;
		public RoomSettings Gym;
		public RoomSettings Museum;
		public RoomSettings MuseumSpace;
		public PlainRoomSettings PrivateBathroom;
		public PlainRoomSettings Warehouse;
		public PlainRoomSettings BatteryRoom;
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

		public bool IsRoomEnabled(string id)
		{
			if (file.disabled == null)
				return true;
			for (int i = 0; i < file.disabled.Length; i++)
			{
				if (file.disabled[i] == id)
					return false;
			}
			return true;
		}

		public void SetRoomEnabled(string id, bool enabled)
		{
			List<string> list = new List<string>();
			if (file.disabled != null)
			{
				for (int i = 0; i < file.disabled.Length; i++)
				{
					if (file.disabled[i] != id)
						list.Add(file.disabled[i]);
				}
			}
			if (!enabled)
				list.Add(id);
			file.disabled = list.ToArray();
			ApplyIncludeFlags();
			Save();
		}

		public void SetAllRoomsEnabled(bool enabled)
		{
			file.disabled = enabled ? new string[0] : (string[])ToggleIds.Clone();
			ApplyIncludeFlags();
			Save();
		}

		public string LabelFor(string id)
		{
			switch (id)
			{
				case IdPrivateBathroom: return STRINGS.ROOMS.TYPES.PRIVATEBATHROOM.NAME;
				case IdWarehouse: return STRINGS.ROOMS.TYPES.WAREHOUSE.NAME;
				case IdBatteryRoom: return STRINGS.ROOMS.TYPES.BATTERYROOM.NAME;
				case IdWasteRoom: return STRINGS.ROOMS.TYPES.WASTEROOM.NAME;
				case IdWaterRoom: return STRINGS.ROOMS.TYPES.WATERROOM.NAME;
				case IdNuclearPlant: return STRINGS.ROOMS.TYPES.NUCLEARPLANT.NAME;
				case IdHallway: return STRINGS.ROOMS.TYPES.HALLWAY.NAME;
				case IdGym: return STRINGS.ROOMS.TYPES.GYMROOM.NAME;
				case IdMuseum: return STRINGS.ROOMS.TYPES.MUSEUM.NAME;
				case IdMuseumSpace: return STRINGS.ROOMS.TYPES.MUSEUMSPACE.NAME;
				case IdGraveyard: return STRINGS.ROOMS.TYPES.GRAVEYARD.NAME;
				default: return id;
			}
		}

		public int GetMaxRoomSize()
		{
			int max = 128;
			max = Math.Max(max, ResizeMaxRoomSize64);
			max = Math.Max(max, ResizeMaxRoomSize96);
			max = Math.Max(max, ResizeMaxRoomSize120);
			max = Math.Max(max, Graveyard.MaxSize);
			max = Math.Max(max, Gym.MaxSize);
			max = Math.Max(max, Museum.MaxSize);
			max = Math.Max(max, MuseumSpace.MaxSize);
			max = Math.Max(max, PrivateBathroom.MaxSize);
			max = Math.Max(max, Warehouse.MaxSize);
			max = Math.Max(max, BatteryRoom.MaxSize);
			max = Math.Max(max, WasteRoom.MaxSize);
			max = Math.Max(max, WaterRoom.MaxSize);
			max = Math.Max(max, NuclearPlant.MaxSize);
			max = Math.Max(max, Hallway.MaxSize);
			return max;
		}

		void LoadFile()
		{
			try
			{
				if (File.Exists(FilePath))
				{
					FileData loaded = JsonUtility.FromJson<FileData>(File.ReadAllText(FilePath));
					if (loaded != null)
						file = loaded;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"{Mod.Namespace}: failed to read settings: {ex.Message}");
			}
			if (file.disabled == null)
				file.disabled = new string[0];
			ApplyIncludeFlags();
		}

		void ApplyIncludeFlags()
		{
			Graveyard.IncludeRoom = IsRoomEnabled(IdGraveyard);
			Gym.IncludeRoom = IsRoomEnabled(IdGym);
			Museum.IncludeRoom = IsRoomEnabled(IdMuseum);
			MuseumSpace.IncludeRoom = IsRoomEnabled(IdMuseumSpace);
			PrivateBathroom.IncludeRoom = IsRoomEnabled(IdPrivateBathroom);
			Warehouse.IncludeRoom = IsRoomEnabled(IdWarehouse);
			BatteryRoom.IncludeRoom = IsRoomEnabled(IdBatteryRoom);
			WasteRoom.IncludeRoom = IsRoomEnabled(IdWasteRoom);
			WaterRoom.IncludeRoom = IsRoomEnabled(IdWaterRoom);
			NuclearPlant.IncludeRoom = IsRoomEnabled(IdNuclearPlant);
			Hallway.IncludeRoom = IsRoomEnabled(IdHallway);
		}

		void Save()
		{
			try
			{
				string dir = Path.GetDirectoryName(FilePath);
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);
				File.WriteAllText(FilePath, JsonUtility.ToJson(file, true));
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"{Mod.Namespace}: failed to save settings: {ex.Message}");
			}
		}
	}
}
