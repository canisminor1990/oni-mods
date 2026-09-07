using HarmonyLib;

namespace MoreRoomTypes
{
	class RoomsExpanded_Patches_EquipmentEfficiency
	{
		[HarmonyPatch(typeof(Workable), nameof(Workable.GetEfficiencyMultiplier))]
		public static class Workable_GetEfficiencyMultiplier_Patch
		{
			public static void Postfix(Workable __instance, ref float __result)
			{
				if (EquipmentEfficiencyTracker.ShouldBoost(__instance))
					__result *= 1f + EquipmentEfficiencyTracker.Bonus;
			}
		}
	}
}
