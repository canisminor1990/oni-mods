===== Database.PermitPresentationInfo =====
using STRINGS;
using UnityEngine;

namespace Database;

public struct PermitPresentationInfo
{
	public Sprite sprite;

	public string facadeFor { get; private set; }

	public static Sprite GetUnknownSprite()
	{
		return Assets.GetSprite("unknown");
	}

	public void SetFacadeForPrefabName(string prefabName)
	{
		facadeFor = UI.KLEI_INVENTORY_SCREEN.ITEM_FACADE_FOR.Replace("{ConfigProperName}", prefabName);
	}

	public void SetFacadeForPrefabID(string prefabId)
	{
		if (Assets.TryGetPrefab(prefabId) == null)
		{
			facadeFor = UI.KLEI_INVENTORY_SCREEN.ITEM_DLC_REQUIRED;
		}
		else
		{
			facadeFor = UI.KLEI_INVENTORY_SCREEN.ITEM_FACADE_FOR.Replace("{ConfigProperName}", Assets.GetPrefab(prefabId).GetProperName());
		}
	}

	public void SetFacadeForText(string text)
	{
		facadeFor = text;
	}
}

===== Def =====
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Def : ScriptableObject
{
	public string PrefabID;

	public Tag Tag;

	private static Dictionary<Tuple<KAnimFile, string, bool>, Sprite> knownUISprites = new Dictionary<Tuple<KAnimFile, string, bool>, Sprite>();

	public const string DEFAULT_SPRITE = "unknown";

	public virtual string Name => null;

	public virtual void InitDef()
	{
		Tag = TagManager.Create(PrefabID);
	}

	public static Tuple<Sprite, Color> GetUISprite(object item, string animName = "ui", bool centered = false)
	{
		if (item is Substance)
		{
			return GetUISprite(ElementLoader.FindElementByHash((item as Substance).elementID), animName, centered);
		}
		if (item is Element)
		{
			if ((item as Element).IsSolid)
			{
				return new Tuple<Sprite, Color>(GetUISpriteFromMultiObjectAnim((item as Element).substance.anim, animName, centered), Color.white);
			}
			if ((item as Element).IsLiquid)
			{
				return new Tuple<Sprite, Color>(Assets.GetSprite("element_liquid"), (item as Element).substance.uiColour);
			}
			if ((item as Element).IsGas)
			{
				return new Tuple<Sprite, Color>(Assets.GetSprite("element_gas"), (item as Element).substance.uiColour);
			}
			return new Tuple<Sprite, Color>(Assets.GetSprite("unknown_far"), Color.black);
		}
		if (item is AsteroidGridEntity)
		{
			return new Tuple<Sprite, Color>(((AsteroidGridEntity)item).GetUISprite(), Color.white);
		}
		if (item is GameObject)
		{
			GameObject gameObject = item as GameObject;
			if (ElementLoader.GetElement(gameObject.PrefabID()) != null)
			{
				return GetUISprite(ElementLoader.GetElement(gameObject.PrefabID()), animName, centered);
			}
			KPrefabID component = gameObject.GetComponent<KPrefabID>();
			CreatureBrain component2 = gameObject.GetComponent<CreatureBrain>();
			if (component2 != null)
			{
				animName = component2.symbolPrefix + "ui";
			}
			SpaceArtifact component3 = gameObject.GetComponent<SpaceArtifact>();
			if (component3 != null)
			{
				animName = component3.GetUIAnim();
			}
			MultiMinionDiningTable.Seat component4 = gameObject.GetComponent<MultiMinionDiningTable.Seat>();
			if (component4 != null)
			{
				gameObject = component4.DiningTable.gameObject;
			}
			if (component.HasTag(GameTags.Egg))
			{
				IncubationMonitor.Def def = gameObject.GetDef<IncubationMonitor.Def>();
				if (def != null)
				{
					GameObject prefab = Assets.GetPrefab(def.spawnedCreature);
					if ((bool)prefab)
					{
						component2 = prefab.GetComponent<CreatureBrain>();
						if ((bool)component2 && !string.IsNullOrEmpty(component2.symbolPrefix))
						{
							animName = component2.symbolPrefix + animName;
						}
					}
				}
			}
			if (component.HasTag(GameTags.BionicUpgrade))
			{
				animName = BionicUpgradeComponentConfig.UpgradesData[component.PrefabID()].uiAnimName;
			}
			KBatchedAnimController component5 = gameObject.GetComponent<KBatchedAnimController>();
			if ((bool)component5)
			{
				Sprite uISpriteFromMultiObjectAnim = GetUISpriteFromMultiObjectAnim(component5.AnimFiles[0], animName, centered);
				return new Tuple<Sprite, Color>(uISpriteFromMultiObjectAnim, (uISpriteFromMultiObjectAnim != null) ? Color.white : Color.clear);
			}
			if (gameObject.GetComponent<Building>() != null)
			{
				Sprite uISprite = gameObject.GetComponent<Building>().Def.GetUISprite(animName, centered);
				return new Tuple<Sprite, Color>(uISprite, (uISprite != null) ? Color.white : Color.clear);
			}
			Debug.LogWarningFormat("Can't get sprite for type {0} (no KBatchedAnimController)", item.ToString());
			return new Tuple<Sprite, Color>(Assets.GetSprite("unknown"), Color.grey);
		}
		if (item is string)
		{
			if (Db.Get().Amounts.Exists(item as string))
			{
				return new Tuple<Sprite, Color>(Assets.GetSprite(Db.Get().Amounts.Get(item as string).uiSprite), Color.white);
			}
			if (Db.Get().Attributes.Exists(item as string))
			{
				return new Tuple<Sprite, Color>(Assets.GetSprite(Db.Get().Attributes.Get(item as string).uiSprite), Color.white);
			}
			return GetUISprite((item as string).ToTag(), animName, centered);
		}
		if (item is Tag)
		{
			if (ElementLoader.GetElement((Tag)item) != null)
			{
				return GetUISprite(ElementLoader.GetElement((Tag)item), animName, centered);
			}
			if (Assets.GetPrefab((Tag)item) != null)
			{
				return GetUISprite(Assets.GetPrefab((Tag)item), animName, centered);
			}
			if (Assets.GetSprite(((Tag)item).Name) != null)
			{
				return new Tuple<Sprite, Color>(Assets.GetSprite(((Tag)item).Name), Color.white);
			}
			Tag[] array = GameTags.Creatures.Species.AllSpecies_REFLECTION();
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i] == (Tag)item))
				{
					continue;
				}
				foreach (CreatureBrain prefabsWithComponentAsListOfComponent in Assets.GetPrefabsWithComponentAsListOfComponents<CreatureBrain>())
				{
					if (prefabsWithComponentAsListOfComponent.species == (Tag)item && prefabsWithComponentAsListOfComponent.HasTag(GameTags.OriginalCreature))
					{
						return GetUISprite(prefabsWithComponentAsListOfComponent.gameObject);
					}
				}
			}
		}
		return new Tuple<Sprite, Color>(Assets.GetSprite("unknown"), Color.grey);
	}

	public static Tuple<Sprite, Color> GetUISprite(Tag prefabID, string facadeID)
	{
		if (Assets.GetPrefab(prefabID).GetComponent<Equippable>() != null && !facadeID.IsNullOrWhiteSpace())
		{
			return Db.GetEquippableFacades().Get(facadeID).GetUISprite();
		}
		return GetUISprite(prefabID);
	}

	public static Sprite GetFacadeUISprite(string facadeID)
	{
		return GetUISpriteFromMultiObjectAnim(Assets.GetAnim(Db.GetBuildingFacades().Get(facadeID).AnimFile));
	}

	public static Sprite GetUISpriteFromMultiObjectAnim(KAnimFile animFile, string animName = "ui", bool centered = false, string symbolName = "")
	{
		Tuple<KAnimFile, string, bool> key = new Tuple<KAnimFile, string, bool>(animFile, animName, centered);
		if (knownUISprites.ContainsKey(key))
		{
			return knownUISprites[key];
		}
		if (animFile == null)
		{
			DebugUtil.LogWarningArgs(animName, "missing Anim File");
			return Assets.GetSprite("unknown");
		}
		Sprite spriteFromKAnimFile = GetSpriteFromKAnimFile(animFile, null, null, null, animName, centered, symbolName);
		if (spriteFromKAnimFile == null)
		{
			return Assets.GetSprite("unknown");
		}
		spriteFromKAnimFile.name = $"{spriteFromKAnimFile.texture.name}:{animName}:{centered}";
		knownUISprites[key] = spriteFromKAnimFile;
		return spriteFromKAnimFile;
	}

	public static Sprite GetSpriteFromKAnimFile(KAnimFile animFile, KAnimFileData kafd, KAnim.Build build, KBatchGroupData batchGroupData, string animName = "ui", bool centered = false, string symbolName = "")
	{
		kafd = ((kafd == null) ? animFile.GetData() : kafd);
		if (kafd == null)
		{
			DebugUtil.LogWarningArgs(animName, "KAnimFileData is null");
			return null;
		}
		build = ((build == null) ? kafd.build : build);
		if (build == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(symbolName))
		{
			symbolName = animName;
		}
		KAnimHashedString symbol_name = new KAnimHashedString(symbolName);
		KAnim.Build.Symbol symbol = build.GetSymbol(symbol_name);
		if (symbol == null)
		{
			DebugUtil.LogWarningArgs(animFile.name, animName, "placeSymbol [", symbolName, "] is missing");
			return null;
		}
		int frame = 0;
		KAnim.Build.SymbolFrameInstance symbolFrameInstance = ((batchGroupData == null) ? symbol.GetFrame(frame) : symbol.GetFrame(frame, batchGroupData));
		Texture2D texture2D = ((batchGroupData == null) ? build.GetTexture(0) : build.GetTexture(0, batchGroupData));
		Debug.Assert(texture2D != null, "Invalid texture on " + animFile.name);
		float x = symbolFrameInstance.uvMin.x;
		float x2 = symbolFrameInstance.uvMax.x;
		float y = symbolFrameInstance.uvMax.y;
		float y2 = symbolFrameInstance.uvMin.y;
		int num = (int)((float)texture2D.width * Mathf.Abs(x2 - x));
		int num2 = (int)((float)texture2D.height * Mathf.Abs(y2 - y));
		float num3 = Mathf.Abs(symbolFrameInstance.bboxMax.x - symbolFrameInstance.bboxMin.x);
		Rect rect = default(Rect);
		rect.width = num;
		rect.height = num2;
		rect.x = (int)((float)texture2D.width * x);
		rect.y = (int)((float)texture2D.height * y);
		float pixelsPerUnit = 100f;
		if (num != 0)
		{
			pixelsPerUnit = 100f / (num3 / (float)num);
		}
		Sprite sprite = Sprite.Create(texture2D, rect, centered ? new Vector2(0.5f, 0.5f) : Vector2.zero, pixelsPerUnit, 0u, SpriteMeshType.FullRect);
		sprite.name = $"{texture2D.name}:{animName}:{centered}";
		return sprite;
	}

	public static KAnimFile GetAnimFileFromPrefabWithTag(GameObject prefab, string desiredAnimName, out string animName)
	{
		animName = desiredAnimName;
		if (prefab == null)
		{
			return null;
		}
		CreatureBrain component = prefab.GetComponent<CreatureBrain>();
		if (component != null)
		{
			animName = component.symbolPrefix + animName;
		}
		SpaceArtifact component2 = prefab.GetComponent<SpaceArtifact>();
		if (component2 != null)
		{
			animName = component2.GetUIAnim();
		}
		if (prefab.HasTag(GameTags.Egg))
		{
			IncubationMonitor.Def def = prefab.GetDef<IncubationMonitor.Def>();
			if (def != null)
			{
				GameObject prefab2 = Assets.GetPrefab(def.spawnedCreature);
				if ((bool)prefab2)
				{
					component = prefab2.GetComponent<CreatureBrain>();
					if ((bool)component && !string.IsNullOrEmpty(component.symbolPrefix))
					{
						animName = component.symbolPrefix + animName;
					}
				}
			}
		}
		return prefab.GetComponent<KBatchedAnimController>().AnimFiles[0];
	}

	public static KAnimFile GetAnimFileFromPrefabWithTag(Tag prefabID, string desiredAnimName, out string animName)
	{
		return GetAnimFileFromPrefabWithTag(Assets.GetPrefab(prefabID), desiredAnimName, out animName);
	}
}

