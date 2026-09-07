using HarmonyLib;
using UnityEngine;

namespace MoreRoomTypes
{
	class RoomsExpanded_Translation_Patches
	{
		[HarmonyPatch(typeof(Localization), "Initialize")]
		public class Localization_Initialize_Patch
		{
			public static void Postfix()
			{
				CanisMinor.Shared.LocalePo.Register(typeof(STRINGS), Mod.ContentPath, Mod.Namespace + ": ");
				Debug.Log($"{Mod.Namespace}: using translation done by {STRINGS.TRANSLATION.AUTHOR.NAME}");
			}
		}
	}
}
