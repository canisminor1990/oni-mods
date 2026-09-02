using HarmonyLib;
using KMod;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModListPreviews
{
	public static class Patches
	{
		[HarmonyPatch(typeof(ModsScreen), "BuildDisplay")]
		[HarmonyPriority(Priority.Last)]
		public static class ModsScreen_BuildDisplay_Patch
		{
			public static void Postfix(object __instance)
			{
				try
				{
					ModsScreen screen = __instance as ModsScreen;
					PreviewListUI.Apply(screen);
					SettingsScreen.AddModButtons(screen);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "BuildDisplay failed: " + ex);
				}
			}
		}

		[HarmonyPatch(typeof(ModsScreen), "OnDeactivate")]
		public static class ModsScreen_OnDeactivate_Patch
		{
			public static void Prefix()
			{
				PreviewListUI.OnScreenClosed();
			}
		}

		[HarmonyPatch(typeof(KMod.Manager), nameof(KMod.Manager.Sanitize))]
		public static class Manager_Sanitize_Patch
		{
			public static void Prefix()
			{
				try
				{
					SteamPreviewSync.MarkLoadedSteamModsSubscribed();
				}
				catch (Exception ex)
				{
					Debug.LogWarning(Mod.LogPrefix + "Steam subscribe sync failed: " + ex.Message);
				}
			}
		}

		[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
		public static class Localization_Initialize_Patch
		{
			public static void Postfix()
			{
				I18n.Register();
			}
		}
	}

	internal static class SteamPreviewSync
	{
		public static void MarkLoadedSteamModsSubscribed()
		{
			List<KMod.Mod> mods = Global.Instance?.modManager?.mods;
			if (mods == null)
				return;

			SteamUGCService ugc = SteamUGCService.Instance;
			for (int i = 0; i < mods.Count; i++)
			{
				KMod.Mod mod = mods[i];
				if (mod == null || mod.is_subscribed)
					continue;
				if (mod.label.distribution_platform != Label.DistributionPlatform.Steam)
					continue;
				if (!ulong.TryParse(mod.label.id, out ulong steamId))
					continue;

				PublishedFileId_t id = new PublishedFileId_t(steamId);
				bool subscribed = ugc != null && ugc.IsSubscribed(id);
				if (!subscribed)
				{
					try
					{
						subscribed = (SteamUGC.GetItemState(id) & (uint)EItemState.k_EItemStateSubscribed) != 0;
					}
					catch
					{
					}
				}
				if (subscribed)
					mod.is_subscribed = true;
			}
		}
	}
}
