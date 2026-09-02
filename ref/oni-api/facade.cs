===== Database.BuildingFacadeResource =====
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Database;

public class BuildingFacadeResource : PermitResource
{
	public string PrefabID;

	public string AnimFile;

	public Dictionary<string, string> InteractFile;

	public Dictionary<string, string> Data;

	[Obsolete("Please use constructor with dlcIds parameter")]
	public BuildingFacadeResource(string Id, string Name, string Description, PermitRarity Rarity, string PrefabID, string AnimFile, Dictionary<string, string> workables = null)
		: this(Id, Name, Description, Rarity, PrefabID, AnimFile, workables, null, null)
	{
	}

	[Obsolete("Please use constructor with dlcIds parameter")]
	public BuildingFacadeResource(string Id, string Name, string Description, PermitRarity Rarity, string PrefabID, string AnimFile, string[] dlcIds, Dictionary<string, string> workables = null)
		: this(Id, Name, Description, Rarity, PrefabID, AnimFile, workables, null, null)
	{
	}

	[Obsolete("Please use the one with data parameter")]
	public BuildingFacadeResource(string Id, string Name, string Description, PermitRarity Rarity, string PrefabID, string AnimFile, Dictionary<string, string> workables = null, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
		: this(Id, Name, Description, Rarity, PrefabID, AnimFile, workables, requiredDlcIds, forbiddenDlcIds, null)
	{
	}

	public BuildingFacadeResource(string Id, string Name, string Description, PermitRarity Rarity, string PrefabID, string AnimFile, Dictionary<string, string> workables = null, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null, Dictionary<string, string> data = null)
		: base(Id, Name, Description, PermitCategory.Building, Rarity, requiredDlcIds, forbiddenDlcIds)
	{
		base.Id = Id;
		this.PrefabID = PrefabID;
		this.AnimFile = AnimFile;
		InteractFile = workables;
		Data = data;
	}

	public void Init()
	{
		GameObject gameObject = Assets.TryGetPrefab(PrefabID);
		if (gameObject == null)
		{
			return;
		}
		gameObject.AddOrGet<BuildingFacade>();
		BuildingDef def = gameObject.GetComponent<Building>().Def;
		if (!(def != null))
		{
			return;
		}
		def.AddFacade(Id);
		KAnimFileData data = def.AnimFiles[0].GetData();
		KAnimFileData data2 = Assets.GetAnim(AnimFile).GetData();
		for (int i = 0; i < data.animCount; i++)
		{
			KAnim.Anim anim = data.GetAnim(i);
			KAnim.Anim anim2 = data2.GetAnim(anim.name);
			if (anim2 != null)
			{
				bool flag = GameAudioSheets.Get().events.ContainsKey(anim.id);
				if (!GameAudioSheets.Get().events.ContainsKey(anim2.id) && flag)
				{
					GameAudioSheets.Get().skinToBaseAnim[anim2.id] = anim.id;
				}
			}
		}
	}

	public override PermitPresentationInfo GetPermitPresentationInfo()
	{
		PermitPresentationInfo result = default(PermitPresentationInfo);
		result.sprite = Def.GetUISpriteFromMultiObjectAnim(Assets.GetAnim(AnimFile));
		result.SetFacadeForPrefabID(PrefabID);
		return result;
	}
}

===== AnimTileable =====
using UnityEngine;

[SkipSaveFileSerialization]
[AddComponentMenu("KMonoBehaviour/scripts/AnimTileable")]
public class AnimTileable : KMonoBehaviour
{
	private HandleVector<int>.Handle partitionerEntry;

	public ObjectLayer objectLayer = ObjectLayer.Building;

	public Tag[] tags;

	private Extents extents;

	private static readonly KAnimHashedString[] leftSymbols = new KAnimHashedString[3]
	{
		new KAnimHashedString("cap_left"),
		new KAnimHashedString("cap_left_fg"),
		new KAnimHashedString("cap_left_place")
	};

	private static readonly KAnimHashedString[] rightSymbols = new KAnimHashedString[3]
	{
		new KAnimHashedString("cap_right"),
		new KAnimHashedString("cap_right_fg"),
		new KAnimHashedString("cap_right_place")
	};

	private static readonly KAnimHashedString[] topSymbols = new KAnimHashedString[3]
	{
		new KAnimHashedString("cap_top"),
		new KAnimHashedString("cap_top_fg"),
		new KAnimHashedString("cap_top_place")
	};

	private static readonly KAnimHashedString[] bottomSymbols = new KAnimHashedString[3]
	{
		new KAnimHashedString("cap_bottom"),
		new KAnimHashedString("cap_bottom_fg"),
		new KAnimHashedString("cap_bottom_place")
	};

	protected override void OnPrefabInit()
	{
		base.OnPrefabInit();
		if (tags == null || tags.Length == 0)
		{
			tags = new Tag[1] { GetComponent<KPrefabID>().PrefabTag };
		}
	}

	protected override void OnSpawn()
	{
		OccupyArea component = GetComponent<OccupyArea>();
		if (component != null)
		{
			this.extents = component.GetExtents();
		}
		else
		{
			Building component2 = GetComponent<Building>();
			this.extents = component2.GetExtents();
		}
		Extents extents = new Extents(this.extents.x - 1, this.extents.y - 1, this.extents.width + 2, this.extents.height + 2);
		partitionerEntry = GameScenePartitioner.Instance.Add("AnimTileable.OnSpawn", base.gameObject, extents, GameScenePartitioner.Instance.objectLayers[(int)objectLayer], OnNeighbourCellsUpdated);
		UpdateEndCaps();
	}

	protected override void OnCleanUp()
	{
		GameScenePartitioner.Instance.Free(ref partitionerEntry);
		base.OnCleanUp();
	}

	private void UpdateEndCaps()
	{
		int cell = Grid.PosToCell(this);
		bool is_visible = true;
		bool is_visible2 = true;
		bool is_visible3 = true;
		bool is_visible4 = true;
		Grid.CellToXY(cell, out var x, out var y);
		CellOffset offset = new CellOffset(extents.x - x - 1, 0);
		CellOffset offset2 = new CellOffset(extents.x - x + extents.width, 0);
		CellOffset offset3 = new CellOffset(0, extents.y - y + extents.height);
		CellOffset offset4 = new CellOffset(0, extents.y - y - 1);
		Rotatable component = GetComponent<Rotatable>();
		if ((bool)component)
		{
			offset = component.GetRotatedCellOffset(offset);
			offset2 = component.GetRotatedCellOffset(offset2);
			offset3 = component.GetRotatedCellOffset(offset3);
			offset4 = component.GetRotatedCellOffset(offset4);
		}
		int num = Grid.OffsetCell(cell, offset);
		int num2 = Grid.OffsetCell(cell, offset2);
		int num3 = Grid.OffsetCell(cell, offset3);
		int num4 = Grid.OffsetCell(cell, offset4);
		if (Grid.IsValidCell(num))
		{
			is_visible = !HasTileableNeighbour(num);
		}
		if (Grid.IsValidCell(num2))
		{
			is_visible2 = !HasTileableNeighbour(num2);
		}
		if (Grid.IsValidCell(num3))
		{
			is_visible3 = !HasTileableNeighbour(num3);
		}
		if (Grid.IsValidCell(num4))
		{
			is_visible4 = !HasTileableNeighbour(num4);
		}
		KBatchedAnimController[] componentsInChildren = GetComponentsInChildren<KBatchedAnimController>();
		foreach (KBatchedAnimController kBatchedAnimController in componentsInChildren)
		{
			KAnimHashedString[] array = leftSymbols;
			foreach (KAnimHashedString symbol in array)
			{
				kBatchedAnimController.SetSymbolVisiblity(symbol, is_visible);
			}
			array = rightSymbols;
			foreach (KAnimHashedString symbol2 in array)
			{
				kBatchedAnimController.SetSymbolVisiblity(symbol2, is_visible2);
			}
			array = topSymbols;
			foreach (KAnimHashedString symbol3 in array)
			{
				kBatchedAnimController.SetSymbolVisiblity(symbol3, is_visible3);
			}
			array = bottomSymbols;
			foreach (KAnimHashedString symbol4 in array)
			{
				kBatchedAnimController.SetSymbolVisiblity(symbol4, is_visible4);
			}
		}
	}

	private bool HasTileableNeighbour(int neighbour_cell)
	{
		bool result = false;
		GameObject gameObject = Grid.Objects[neighbour_cell, (int)objectLayer];
		if (gameObject != null)
		{
			KPrefabID component = gameObject.GetComponent<KPrefabID>();
			if (component != null && component.HasAnyTags(tags))
			{
				result = true;
			}
		}
		return result;
	}

	private void OnNeighbourCellsUpdated(object data)
	{
		if (!(this == null) && !(base.gameObject == null) && partitionerEntry.IsValid())
		{
			UpdateEndCaps();
		}
	}
}

