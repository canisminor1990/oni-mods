using System.Collections.Generic;
using Database;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Industrial
	{
		public static void AddRoom(ref RoomTypes __instance)
		{
			if (!Settings.Instance.Industrial.IncludeRoom)
				return;

			List<RoomType> upgrades = new List<RoomType>
			{
				__instance.PowerPlant,
				__instance.Farm,
				__instance.CreaturePen
			};
			if (Settings.Instance.Gym.IncludeRoom)
				upgrades.Add(RoomTypes_AllModded.GymRoom);
			if (Settings.Instance.BatteryRoom.IncludeRoom)
				upgrades.Add(RoomTypes_AllModded.BatteryRoom);
			if (Settings.Instance.OxygenRoom.IncludeRoom)
				upgrades.Add(RoomTypes_AllModded.OxygenRoom);
			if (Settings.Instance.WasteRoom.IncludeRoom)
				upgrades.Add(RoomTypes_AllModded.WasteRoom);
			if (Settings.Instance.WaterRoom.IncludeRoom)
				upgrades.Add(RoomTypes_AllModded.WaterRoom);
			if (Settings.Instance.NuclearPlant.IncludeRoom)
				upgrades.Add(RoomTypes_AllModded.NuclearPlant);

			__instance.Add(RoomTypes_AllModded.IndustrialRoom(upgrades.ToArray()));
		}
	}
}
