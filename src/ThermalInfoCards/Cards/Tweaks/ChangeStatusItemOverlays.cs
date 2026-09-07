using HarmonyLib;
using System.Reflection;

namespace BetterInfoCards;

[HarmonyPatch]
class ChangeStatusItemOverlays
{
	static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(Database.MiscStatusItems), "CreateStatusItems");
	}

	static void Postfix(Database.MiscStatusItems __instance)
	{
		__instance.OreTemp.status_overlays &= ~(int)StatusItem.StatusItemOverlays.Temperature;
		__instance.PickupableUnreachable.status_overlays = 0;

		if (Options.Opts.HideElementCategories)
			__instance.ElementalCategory.status_overlays = 0;
	}
}
