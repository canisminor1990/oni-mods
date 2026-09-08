// Startup load path (U59). The Klei logo stays up through LaunchInitializer.Update:
// Global.Awake (mods / localization / world settings) then Assets.OnPrefabInit
// (anims / elements / prefabs / Db). MainMenu.OnActivate is the first frontend screen.

public class Global : MonoBehaviour
{
	public GameObject globalCanvas; // GameObject.Find("Canvas") before DLL load

	private void Awake()
	{
		globalCanvas = GameObject.Find("Canvas");
		modManager = new KMod.Manager();
		modManager.LoadModDBAndInitialize();
		modManager.Load(Content.DLL);
		modManager.Load(Content.Strings);
		Localization.Initialize();
		modManager.Load(Content.Translation);
		modManager.Load(Content.LayerableFiles);
		WorldGen.LoadSettings(in_async_thread: true);
		GlobalResources.Instance();
	}
}

namespace KMod
{
	[Flags]
	public enum Content : byte
	{
		LayerableFiles = 1,
		Strings = 2,
		DLL = 4,
		Translation = 8,
		Animation = 0x10
	}

	public class Mod
	{
		public string title { get; }
		public void Load(Content content);
		public void PostLoad(IReadOnlyList<Mod> mods);
	}
}

public class Assets : KMonoBehaviour
{
	protected override void OnPrefabInit()
	{
		LoadAnims();          // KAnimGroupFile.LoadAll + KAnimFile.FinalizeLoading
		SubstanceListHookup(); // ElementLoader.Load
		CreatePrefabs();       // Db.Get, AddPrefab, LegacyModMain.Load, Db.PostProcess
	}
}
