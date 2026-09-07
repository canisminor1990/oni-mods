using Database;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_Hallway
	{
		public const float Athletics = 3f;

		public static void AddRoom(ref RoomTypes __instance)
		{
			if (!Settings.Instance.Hallway.IncludeRoom)
				return;

			__instance.Add(RoomTypes_AllModded.Hallway);
			Stomp(__instance.PlumbedBathroom);
			Stomp(__instance.Latrine);
			Stomp(__instance.MessHall);
			Stomp(__instance.GreatHall);
			Stomp(__instance.Kitchen);
			Stomp(__instance.Barracks);
			Stomp(__instance.Bedroom);
			Stomp(__instance.PrivateBedroom);
			Stomp(__instance.Hospital);
			Stomp(__instance.RecRoom);
			Stomp(__instance.PowerPlant);
			Stomp(__instance.Farm);
			Stomp(__instance.CreaturePen);
			Stomp(__instance.Laboratory);
			Stomp(__instance.Park);
			Stomp(__instance.NatureReserve);
			Stomp(RoomConstraintTags.FindRoomType(__instance, "BanquetHall"));
			Stomp(__instance.MassageClinic);
			Stomp(RoomConstraintTags.FindRoomType(__instance, RoomTypeIndustrialData.RoomId));
			if (Settings.Instance.PrivateBathroom.IncludeRoom)
				Stomp(RoomTypes_AllModded.PrivateBathroom);
			if (Settings.Instance.Warehouse.IncludeRoom)
				Stomp(RoomTypes_AllModded.Warehouse);
			if (Settings.Instance.BatteryRoom.IncludeRoom)
				Stomp(RoomTypes_AllModded.BatteryRoom);
			if (Settings.Instance.OxygenRoom.IncludeRoom)
				Stomp(RoomTypes_AllModded.OxygenRoom);
			if (Settings.Instance.WasteRoom.IncludeRoom)
				Stomp(RoomTypes_AllModded.WasteRoom);
			if (Settings.Instance.WaterRoom.IncludeRoom)
				Stomp(RoomTypes_AllModded.WaterRoom);
			if (Settings.Instance.NuclearPlant.IncludeRoom)
				Stomp(RoomTypes_AllModded.NuclearPlant);
			if (Settings.Instance.Gym.IncludeRoom)
				Stomp(RoomTypes_AllModded.GymRoom);
			if (Settings.Instance.Graveyard.IncludeRoom)
				Stomp(RoomTypes_AllModded.GraveyardRoom);
			if (Settings.Instance.Museum.IncludeRoom)
				Stomp(RoomTypes_AllModded.Museum);
			if (Settings.Instance.MuseumSpace.IncludeRoom)
				Stomp(RoomTypes_AllModded.MuseumSpace);
		}

		static void Stomp(RoomType stomping)
		{
			RoomConstraintTags.AddStompInConflict(stomping, RoomTypes_AllModded.Hallway);
		}

		public static void RegisterEffects()
		{
			var dbEffects = Db.Get().effects;
			if (dbEffects.Exists(RoomTypeHallwayData.EffectId))
				return;

			Effect hallway = new Effect(
				RoomTypeHallwayData.EffectId,
				STRINGS.ROOMS.EFFECTS.HALLWAY.NAME,
				STRINGS.ROOMS.EFFECTS.HALLWAY.DESCRIPTION,
				0f,
				true,
				false,
				false);
			hallway.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, Athletics, STRINGS.ROOMS.EFFECTS.HALLWAY.NAME));
			dbEffects.Add(hallway);
		}

		[HarmonyPatch(typeof(LadderConfig), nameof(LadderConfig.ConfigureBuildingTemplate))]
		public static class LadderConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.LadderBuildingTag);
		}

		[HarmonyPatch(typeof(FirePoleConfig), nameof(FirePoleConfig.ConfigureBuildingTemplate))]
		public static class FirePoleConfig_Patch
		{
			public static void Postfix(GameObject go) => RoomConstraintTags.AddBuildingTag(go, RoomConstraintTags.FirePoleBuildingTag);
		}

		[HarmonyPatch(typeof(MinionIdentity), "OnSpawn")]
		public static class MinionIdentity_OnSpawn_Patch
		{
			public static void Postfix(MinionIdentity __instance)
			{
				if (__instance != null)
					__instance.gameObject.AddOrGet<HallwayBonusTracker>();
			}
		}
	}
}
