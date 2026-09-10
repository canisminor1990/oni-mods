// Excerpt: custom SideScreenContent registration. DetailsScreen.Refresh
// instantiates screenPrefab into the Config/Errands tab body when
// IsValidForTarget(target) is true.

public class DetailsScreen : KTabMenu
{
	public enum SidescreenTabTypes
	{
		Config,
		Errands,
		Material,
		Blueprints
	}

	public class SideScreenRef
	{
		public string name;
		public SideScreenContent screenPrefab;
		public Vector2 offset;
		public SidescreenTabTypes tab;
		public SideScreenContent screenInstance;
	}

	private List<SideScreenRef> sideScreens;

	public void Refresh(GameObject go)
	{
		foreach (SideScreenRef sideScreen in sideScreens)
		{
			if (!sideScreen.screenPrefab.IsValidForTarget(target))
			{
				if (sideScreen.screenInstance != null && sideScreen.screenInstance.gameObject.activeSelf)
					sideScreen.screenInstance.gameObject.SetActive(false);
				continue;
			}
			if (sideScreen.screenInstance == null)
			{
				SidescreenTab tabOfType = GetTabOfType(sideScreen.tab);
				sideScreen.screenInstance = Util.KInstantiateUI<SideScreenContent>(
					sideScreen.screenPrefab.gameObject,
					tabOfType.bodyInstance);
			}
			sideScreen.screenInstance.SetTarget(target);
			sideScreen.screenInstance.Show();
		}
	}
}
