===== Database.BuildingFacades =====
using System;
using System.Collections.Generic;

namespace Database;

public class BuildingFacades : ResourceSet<BuildingFacadeResource>
{
	public BuildingFacades(ResourceSet parent)
		: base("BuildingFacades", parent)
	{
		Initialize();
		foreach (BuildingFacadeInfo buildingFacade in Blueprints.Get().all.buildingFacades)
		{
			Add(buildingFacade.id, buildingFacade.name, buildingFacade.desc, buildingFacade.rarity, buildingFacade.prefabId, buildingFacade.animFile, buildingFacade.workables, buildingFacade.GetRequiredDlcIds(), buildingFacade.GetForbiddenDlcIds(), buildingFacade.data);
		}
	}

	public void Add(string id, LocString Name, LocString Desc, PermitRarity rarity, string prefabId, string animFile, Dictionary<string, string> workables = null, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null, Dictionary<string, string> data = null)
	{
		BuildingFacadeResource item = new BuildingFacadeResource(id, Name, Desc, rarity, prefabId, animFile, workables, requiredDlcIds, forbiddenDlcIds, data);
		resources.Add(item);
	}

	[Obsolete("Use overload with data parameter")]
	public void Add(string id, LocString Name, LocString Desc, PermitRarity rarity, string prefabId, string animFile, Dictionary<string, string> workables = null, string[] requiredDlcIds = null, string[] forbiddenDlcIds = null)
	{
		Add(id, Name, Desc, rarity, prefabId, animFile, workables, requiredDlcIds, forbiddenDlcIds, null);
	}

	public void PostProcess()
	{
		foreach (BuildingFacadeResource resource in resources)
		{
			resource.Init();
		}
	}
}

