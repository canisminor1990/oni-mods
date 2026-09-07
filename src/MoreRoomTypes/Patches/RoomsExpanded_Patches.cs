using System;
using System.Collections.Generic;
using System.Reflection;
using Database;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace MoreRoomTypes
{
	public class RoomsExpanded_Patches
	{
		[HarmonyPatch(typeof(Db))]
		[HarmonyPatch("Initialize")]
		public class Db_Initialize_Patch
		{
			static bool Patched;

			public static void Prefix()
			{
				if (Patched)
					return;

				// Manual RoomTypes ctor patch — attribute patching locks English constraint translations.
				Harmony harmony = new Harmony(Mod.StaticId);
				Debug.Log($"{Mod.Namespace}: Trying to get RoomTypes type...");
				Type roomTypesType = Type.GetType("Database.RoomTypes, Assembly-CSharp", false);
				if (roomTypesType == null)
				{
					Debug.LogError($"{Mod.Namespace}: Error - RoomTypes type is null...");
					return;
				}

				ConstructorInfo original = roomTypesType.GetConstructor(new Type[] { typeof(ResourceSet) });
				MethodInfo prefix = typeof(RoomTypes_Constructor_Patch).GetMethod(nameof(RoomTypes_Constructor_Patch.Prefix));
				MethodInfo postfix = typeof(RoomTypes_Constructor_Patch).GetMethod(nameof(RoomTypes_Constructor_Patch.Postfix));

				if (original == null || prefix == null || postfix == null)
					Debug.LogError($"{Mod.Namespace}: Error - unable to patch RoomTypes constructor - at least one method is null...");
				else
				{
					harmony.Patch(original, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
					Patched = true;
					Debug.Log($"{Mod.Namespace}: patched Database.RoomTypes constructor.");
				}
			}

			public static void Postfix()
			{
				RoomsExpanded_Patches_Hallway.RegisterEffects();
				RoomsExpanded_Patches_PrivateBathroom.RegisterEffects();
			}
		}

		public class RoomTypes_Constructor_Patch
		{
			public static void Prefix()
			{
			}

			public static void Postfix(ref RoomTypes __instance)
			{
				SortingCounter.Init();

				Debug.Log($"{Mod.Namespace}: RoomTypes_Constructor_Patch Postfix");
				RoomsExpanded_Patches_Gym.AddRoom(ref __instance);
				RoomsExpanded_Patches_Graveyard.AddRoom(ref __instance);
				RoomsExpanded_Patches_PrivateBathroom.AddRoom(ref __instance);
				RoomsExpanded_Patches_Warehouse.AddRoom(ref __instance);
				RoomsExpanded_Patches_Battery.AddRoom(ref __instance);
				RoomsExpanded_Patches_Waste.AddRoom(ref __instance);
				RoomsExpanded_Patches_Water.AddRoom(ref __instance);
				RoomsExpanded_Patches_Nuclear.AddRoom(ref __instance);
				RoomsExpanded_Patches_Museum.AddRoom(ref __instance);
				RoomsExpanded_Patches_MuseumSpace.AddRoom(ref __instance);
				RoomsExpanded_Patches_Hallway.AddRoom(ref __instance);
				RoomConstraintTags.ApplyFunctionalRoomStomps(__instance);
				RoomConstraintTags.ResizeRooms(ref __instance);
			}
		}

		[HarmonyPatch(typeof(RoomProber), MethodType.Constructor)]
		public static class RoomProber_Constructor_Patch
		{
			public static void Postfix()
			{
				TuningData<RoomProber.Tuning>.Get().maxRoomSize = Settings.Instance.GetMaxRoomSize();
			}
		}

		[HarmonyPatch(typeof(RoomType), nameof(RoomType.GetRoomEffectsString))]
		public static class RoomType_GetRoomEffectsString_Patch
		{
			public static void Postfix(RoomType __instance, ref string __result)
			{
				if (!RoomTypeAbstractData.IsModdedRoom(__instance))
					return;

				if (__instance.Id == RoomTypePrivateBathroomData.RoomId)
				{
					if (string.IsNullOrEmpty(__result))
						__result = (string)ROOMS.EFFECTS.HEADER;
					string shower = MoreRoomTypes.STRINGS.ROOMS.EFFECTS.PRIVATEBATHROOM.SHOWER;
					if (!__result.Contains(shower))
						__result += "\n    • " + shower;
					return;
				}

				if (!string.IsNullOrEmpty(__result))
					return;
				if (string.IsNullOrEmpty(__instance.effect))
					return;
				__result = (string)ROOMS.EFFECTS.HEADER + "\n    " + __instance.effect.Trim();
			}
		}

		[HarmonyPatch(typeof(OverlayModes.Rooms))]
		[HarmonyPatch("GetCustomLegendData")]
		public static class Rooms_GetCustomLegendData_Patch
		{
			public static void Postfix(ref List<LegendEntry> __result)
			{
				if (__result == null)
					return;

				if (Settings.Instance.HideLegendEffect)
				{
					foreach (LegendEntry entry in __result)
						entry.name = entry.name.Split('\n')[0];
				}

				List<RoomType> roomTypeList = new List<RoomType>(Db.Get().RoomTypes.resources);
				foreach (RoomType roomType in roomTypeList)
				{
					if (!RoomTypeAbstractData.IsModdedRoom(roomType) || string.IsNullOrEmpty(roomType.effect))
						continue;

					string effects = roomType.GetRoomEffectsString();
					if (string.IsNullOrEmpty(effects))
						continue;

					for (int i = 0; i < __result.Count; i++)
					{
						string legendName = __result[i].name ?? "";
						if (legendName != roomType.Name && !legendName.StartsWith(roomType.Name + "\n"))
							continue;
						if (!string.IsNullOrEmpty(__result[i].desc) && __result[i].desc.Contains(effects))
							continue;
						__result[i].desc += "\n\n" + effects;
					}
				}
			}
		}

		[HarmonyPatch(typeof(ColorSet))]
		[HarmonyPatch("Init")]
		public static class ColorSet_Init_Patch
		{
			public static void Postfix(ColorSet __instance)
			{
				Dictionary<string, Color32> namedLookup = Traverse.Create(__instance).Field("namedLookup").GetValue<Dictionary<string, Color32>>();
				if (namedLookup == null)
					return;

				SetColor(namedLookup, RoomTypeGymData.RoomId, Settings.Instance.Gym.RoomColor);
				SetColor(namedLookup, RoomTypeGraveyardData.RoomId, Settings.Instance.Graveyard.RoomColor);
				SetColor(namedLookup, RoomTypeMuseumData.RoomId, Settings.Instance.Museum.RoomColor);
				SetColor(namedLookup, RoomTypeMuseumSpaceData.RoomId, Settings.Instance.MuseumSpace.RoomColor);
				SetColor(namedLookup, RoomTypePrivateBathroomData.RoomId, Settings.Instance.PrivateBathroom.RoomColor);
				SetColor(namedLookup, RoomTypeWarehouseData.RoomId, Settings.Instance.Warehouse.RoomColor);
				SetColor(namedLookup, RoomTypeBatteryRoomData.RoomId, Settings.Instance.BatteryRoom.RoomColor);
				SetColor(namedLookup, RoomTypeWasteRoomData.RoomId, Settings.Instance.WasteRoom.RoomColor);
				SetColor(namedLookup, RoomTypeWaterRoomData.RoomId, Settings.Instance.WaterRoom.RoomColor);
				SetColor(namedLookup, RoomTypeNuclearPlantData.RoomId, Settings.Instance.NuclearPlant.RoomColor);
				SetColor(namedLookup, RoomTypeHallwayData.RoomId, Settings.Instance.Hallway.RoomColor);
			}

			static void SetColor(Dictionary<string, Color32> namedLookup, string id, Color32 color)
			{
				if (!namedLookup.ContainsKey(id))
					namedLookup.Add(id, color);
				else
					namedLookup[id] = color;
			}
		}
	}
}
