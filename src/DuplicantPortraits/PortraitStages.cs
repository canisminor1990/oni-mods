using Database;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DuplicantPortraits
{
	internal static class PortraitStages
	{
		public static readonly List<string> StageIds = new List<string>();
		private static bool registered;

		public static void Register(string reason)
		{
			if (registered && StageIds.Count > 0)
				return;

			try
			{
				RegisterInternal(reason);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "stage register failed (" + reason + "): " + ex);
			}
		}

		private static void RegisterInternal(string reason)
		{
			Db db = Db.Get();
			if (db == null || db.Permits == null || db.Personalities == null)
			{
				Debug.Log(Mod.LogPrefix + "skip stage register (" + reason + "): db not ready");
				return;
			}

			ArtableStages stages = db.Permits.ArtableStages;
			List<Personality> personalities = GetPersonalities(db);
			if (stages == null || personalities == null || personalities.Count == 0)
			{
				Debug.Log(Mod.LogPrefix + "skip stage register (" + reason + "): personalities not ready");
				return;
			}

			ResolveVanillaAnim(stages, out string fallbackKanim, out string fallbackClip);
			bool canCustom = CanvasKanimFactory.EnsureTemplate();
			if (!canCustom)
				Debug.LogWarning(Mod.LogPrefix + "custom portrait kanim unavailable; using vanilla " + fallbackKanim + "/" + fallbackClip);

			int added = 0;
			int skipped = 0;
			int custom = 0;
			for (int i = 0; i < personalities.Count; i++)
			{
				Personality personality = personalities[i];
				if (!IsAvailable(personality))
				{
					skipped++;
					continue;
				}

				string stageId = Mod.StageIdFor(personality.Id);
				if (stages.Exists(stageId))
				{
					if (!StageIds.Contains(stageId))
						StageIds.Add(stageId);
					continue;
				}

				string kanimName = fallbackKanim;
				string clip = fallbackClip;
				Texture2D portrait = PortraitUtil.GetPortrait(personality);
				if (canCustom && portrait != null)
				{
					string customName = Mod.KanimNameFor(personality.Id);
					if (Assets.GetAnim(customName) != null || CanvasKanimFactory.Create(customName, portrait) != null)
					{
						kanimName = customName;
						clip = CanvasKanimFactory.ClipName;
						custom++;
					}
				}

				string duplicantName = string.IsNullOrEmpty(personality.Name) ? personality.Id : personality.Name;
				string stageName = string.Format(STRINGS.DUPLICANT_PORTRAITS.STAGE_NAME, duplicantName);
				string stageDesc = string.Format(STRINGS.DUPLICANT_PORTRAITS.STAGE_DESC, duplicantName);

				ArtableStage stage = stages.Add(
					stageId,
					stageName,
					stageDesc,
					PermitRarity.Universal,
					kanimName,
					clip,
					TUNING.DECOR.BONUS.TIER3.amount,
					true,
					ArtableStatuses.ArtableStatusType.LookingGreat.ToString(),
					CanvasConfig.ID,
					"",
					new string[0],
					new string[0]);

				if (stage != null && db.Permits.TryGet(stageId) == null)
					db.Permits.Add(stage);

				if (!StageIds.Contains(stageId))
					StageIds.Add(stageId);
				added++;
			}

			registered = StageIds.Count > 0;
			Debug.Log(Mod.LogPrefix + "stages (" + reason + "): added " + added
				+ ", customKanim=" + custom
				+ ", total " + StageIds.Count + ", skipped " + skipped);
		}

		public static void AddToInventory()
		{
			try
			{
				if (StageIds.Count == 0)
					return;
				if (!InventoryOrganization.subcategoryIdToPermitIdsMap.TryGetValue(
					InventoryOrganization.PermitSubcategories.BUILDING_CANVAS_STANDARD, out List<string> ids))
					return;

				int added = 0;
				for (int i = 0; i < StageIds.Count; i++)
				{
					string id = StageIds[i];
					if (!ids.Contains(id))
					{
						ids.Add(id);
						added++;
					}
				}

				if (added > 0)
					Debug.Log(Mod.LogPrefix + "inventory added " + added + " canvas permits");
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "inventory add failed: " + ex.Message);
			}
		}

		private static void ResolveVanillaAnim(ArtableStages stages, out string kanim, out string clip)
		{
			kanim = "painting_art_b";
			clip = "art_b";
			ArtableStage good = stages.TryGet("Canvas_Good");
			if (good == null)
				return;

			string fromFile = ReadString(good, "animFile", "AnimFile", "kanim");
			string fromClip = ReadString(good, "anim", "Anim", "animation");
			if (!string.IsNullOrEmpty(fromFile))
				kanim = fromFile;
			if (!string.IsNullOrEmpty(fromClip))
				clip = fromClip;
			Debug.Log(Mod.LogPrefix + "vanilla Canvas_Good anim=" + kanim + " clip=" + clip);
		}

		private static string ReadString(object obj, params string[] names)
		{
			for (int i = 0; i < names.Length; i++)
			{
				var field = AccessTools.Field(obj.GetType(), names[i]);
				if (field != null && field.FieldType == typeof(string))
					return field.GetValue(obj) as string;
				var prop = AccessTools.Property(obj.GetType(), names[i]);
				if (prop != null && prop.PropertyType == typeof(string))
					return prop.GetValue(obj, null) as string;
			}
			return null;
		}

		private static List<Personality> GetPersonalities(Db db)
		{
			try
			{
				if (db.Personalities.resources == null || db.Personalities.resources.Count == 0)
					return null;
				return new List<Personality>(db.Personalities.resources);
			}
			catch (System.Exception ex)
			{
				Debug.Log(Mod.LogPrefix + "personalities not ready: " + ex.Message);
				return null;
			}
		}

		private static bool IsAvailable(Personality personality)
		{
			if (personality == null || personality.Disabled)
				return false;
			if (string.IsNullOrEmpty(personality.Id))
				return false;
			if (string.IsNullOrEmpty(personality.requiredDlcId))
				return true;
			try
			{
				return DlcManager.IsContentSubscribed(personality.requiredDlcId);
			}
			catch
			{
				return true;
			}
		}
	}
}
