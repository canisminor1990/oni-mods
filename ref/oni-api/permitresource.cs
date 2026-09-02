===== Database.PermitResource =====
namespace Database;

public abstract class PermitResource : Resource, IHasDlcRestrictions
{
	public string Description;

	public PermitCategory Category;

	public PermitRarity Rarity;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public PermitResource(string id, string Name, string Desc, PermitCategory permitCategory, PermitRarity rarity, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: base(id, Name)
	{
		DebugUtil.DevAssert(Name != null, "Name must be provided for permit with id \"" + id + "\" of type " + GetType().Name);
		DebugUtil.DevAssert(Desc != null, "Description must be provided for permit with id \"" + id + "\" of type " + GetType().Name);
		Description = Desc;
		Category = permitCategory;
		Rarity = rarity;
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}

	public abstract PermitPresentationInfo GetPermitPresentationInfo();

	public bool IsOwnableOnServer()
	{
		if (Rarity != PermitRarity.Universal)
		{
			return Rarity != PermitRarity.UniversalLocked;
		}
		return false;
	}

	public bool IsUnlocked()
	{
		if (Rarity != PermitRarity.Universal)
		{
			return PermitItems.IsPermitUnlocked(this);
		}
		return true;
	}

	public string GetDlcIdFrom()
	{
		return DlcManager.GetMostSignificantDlc(this);
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}
}

