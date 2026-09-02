using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Klei;
using Newtonsoft.Json;
using UnityEngine;

namespace KMod;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("{title}")]
public class Mod : IHasDlcRestrictions
{
	public enum Status
	{
		NotInstalled,
		Installed,
		UninstallPending,
		ReinstallPending,
		CannotInstall
	}

	public class ArchivedVersion
	{
		public string relativePath;

		public PackagedModInfo info;
	}

	public class PackagedModInfo : IHasDlcRestrictions
	{
		[Obsolete("Use IHasDlcRestrictions interface instead")]
		public string supportedContent { get; set; }

		public string[] requiredDlcIds { get; set; }

		public string[] forbiddenDlcIds { get; set; }

		[Obsolete("Use minimumSupportedBuild instead!")]
		public int lastWorkingBuild { get; set; }

		public int minimumSupportedBuild { get; set; }

		public int APIVersion { get; set; }

		public string version { get; set; }

		public string[] GetRequiredDlcIds()
		{
			return requiredDlcIds;
		}

		public string[] GetForbiddenDlcIds()
		{
			return forbiddenDlcIds;
		}
	}

	public const int MOD_API_VERSION_NONE = 0;

	public const int MOD_API_VERSION_HARMONY1 = 1;

	public const int MOD_API_VERSION_HARMONY2 = 2;

	public const int MOD_API_VERSION = 2;

	[JsonProperty]
	public Label label;

	[JsonProperty]
	public Status status;

	[JsonProperty]
	public bool enabled;

	[JsonProperty]
	public List<string> enabledForDlc;

	[JsonProperty]
	public int crash_count;

	[JsonProperty]
	public string reinstall_path;

	public bool foundInStackTrace;

	public string relative_root = "";

	public PackagedModInfo packagedModInfo;

	public LoadedModData loaded_mod_data;

	private IFileSource _fileSource;

	public IFileSource content_source;

	public bool is_subscribed;

	private const string VANILLA_ID = "VANILLA_ID";

	private const string ALL_ID = "ALL";

	private const string ARCHIVED_VERSIONS_FOLDER = "archived_versions";

	private const string MOD_INFO_FILENAME = "mod_info.yaml";

	public ModContentCompatability contentCompatability;

	public string[] requiredDlcIds;

	public string[] forbiddenDlcIds;

	public const int MAX_CRASH_COUNT = 3;

	private static readonly List<string> PREVIEW_FILENAMES = new List<string> { "preview.png", "Preview.png", "PREVIEW.PNG" };

	public Content available_content { get; private set; }

	[JsonProperty]
	public string staticID { get; private set; }

	public LocString manage_tooltip { get; private set; }

	public System.Action on_managed { get; private set; }

	public bool is_managed => manage_tooltip != null;

	public string title
	{
		get
		{
			return label.title;
		}
		set
		{
			label.title = value;
		}
	}

	public string description { get; set; }

	public Content loaded_content { get; private set; }

	public IFileSource file_source
	{
		get
		{
			return _fileSource;
		}
		set
		{
			if (_fileSource != null)
			{
				_fileSource.Dispose();
			}
			_fileSource = value;
		}
	}

	public bool DevModCrashTriggered { get; private set; }

	public string ContentPath => Path.Combine(label.install_path, relative_root);

	public bool IsDev => label.distribution_platform == Label.DistributionPlatform.Dev;

	public bool IsLocal
	{
		get
		{
			if (label.distribution_platform != Label.DistributionPlatform.Dev)
			{
				return label.distribution_platform == Label.DistributionPlatform.Local;
			}
			return true;
		}
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	[JsonConstructor]
	public Mod()
	{
	}

	public void CopyPersistentDataTo(Mod other_mod)
	{
		other_mod.status = status;
		other_mod.enabledForDlc = ((enabledForDlc != null) ? new List<string>(enabledForDlc) : new List<string>());
		other_mod.crash_count = crash_count;
		other_mod.loaded_content = loaded_content;
		other_mod.loaded_mod_data = loaded_mod_data;
		other_mod.reinstall_path = reinstall_path;
	}

	public Mod(Label label, string staticID, string description, IFileSource file_source, LocString manage_tooltip, System.Action on_managed)
	{
		this.label = label;
		status = Status.NotInstalled;
		this.staticID = staticID;
		this.description = description;
		this.file_source = file_source;
		this.manage_tooltip = manage_tooltip;
		this.on_managed = on_managed;
		loaded_content = (Content)0;
		available_content = (Content)0;
		ScanContent();
	}

	public bool IsEnabledForActiveDlc()
	{
		return IsEnabledForDlc(DlcManager.GetHighestActiveDlcId());
	}

	public bool IsEnabledForDlc(string dlcId)
	{
		if (enabledForDlc != null)
		{
			return enabledForDlc.Contains(dlcId);
		}
		return false;
	}

	public void SetEnabledForActiveDlc(bool enabled)
	{
		SetEnabledForDlc(DlcManager.GetHighestActiveDlcId(), enabled);
	}

	public void SetEnabledForDlc(string dlcId, bool set_enabled)
	{
		if (enabledForDlc == null)
		{
			enabledForDlc = new List<string>();
		}
		bool flag = enabledForDlc.Contains(dlcId);
		if (set_enabled && !flag)
		{
			enabledForDlc.Add(dlcId);
		}
		else if (!set_enabled && flag)
		{
			enabledForDlc.Remove(dlcId);
		}
	}

	public void ScanContent()
	{
		ModDevLog($"{label} ({label.id}): Setting up mod.");
		available_content = (Content)0;
		if (file_source == null)
		{
			if (label.id.EndsWith(".zip"))
			{
				DebugUtil.DevAssert(test: false, "Does this actually get used ever?");
				file_source = new ZipFile(label.install_path);
			}
			else
			{
				file_source = new Directory(label.install_path);
			}
		}
		if (!file_source.Exists())
		{
			Debug.LogWarning($"{label}: File source does not appear to be valid, skipping. ({label.install_path})");
			return;
		}
		KModHeader header = KModUtil.GetHeader(file_source, label.defaultStaticID, label.title, description, IsDev);
		if (label.title != header.title)
		{
			Debug.Log("\t" + label.title + " has a mod.yaml with the title `" + header.title + "`, using that from now on.");
		}
		if (label.defaultStaticID != header.staticID)
		{
			Debug.Log("\t" + label.title + " has a mod.yaml with a staticID `" + header.staticID + "`, using that from now on.");
		}
		label.title = header.title;
		staticID = header.staticID;
		description = header.description;
		ArchivedVersion mostSuitableArchive = GetMostSuitableArchive();
		if (mostSuitableArchive == null)
		{
			Debug.LogWarning($"{label}: No archive supports this game version, skipping content.");
			contentCompatability = ModContentCompatability.DoesntSupportDLCConfig;
			available_content = (Content)0;
			SetEnabledForActiveDlc(enabled: false);
			return;
		}
		packagedModInfo = mostSuitableArchive.info;
		ScanContentFromSource(mostSuitableArchive.relativePath, out var available);
		if (available == (Content)0)
		{
			Debug.LogWarning($"{label}: No supported content for mod, skipping content.");
			contentCompatability = ModContentCompatability.NoContent;
			available_content = (Content)0;
			SetEnabledForActiveDlc(enabled: false);
			return;
		}
		bool flag = mostSuitableArchive.info.APIVersion == 2;
		if ((available & Content.DLL) != 0 && !flag)
		{
			Debug.LogWarning($"{label}: DLLs found but not using the correct API version.");
			contentCompatability = ModContentCompatability.OldAPI;
			available_content = (Content)0;
			SetEnabledForActiveDlc(enabled: false);
		}
		else
		{
			contentCompatability = ModContentCompatability.OK;
			available_content = available;
			relative_root = mostSuitableArchive.relativePath;
			Debug.Assert(content_source == null);
			content_source = new Directory(ContentPath);
			string arg = (string.IsNullOrEmpty(relative_root) ? "root" : relative_root);
			Debug.Log($"{label}: Successfully loaded from path '{arg}' with content '{available_content.ToString()}'.");
		}
	}

	private ArchivedVersion GetMostSuitableArchive()
	{
		PackagedModInfo packagedModInfo = GetModInfoForFolder("");
		if (packagedModInfo == null)
		{
			if (!ScanContentFromSourceForTranslationsOnly(""))
			{
				Debug.Log($"{label}: Is missing a mod_info.yaml file and will not be loaded, which is required. See the stickied post in the Mods and Tools section on the Klei forums.");
				return null;
			}
			ModDevLogWarning($"{label}: No mod_info.yaml found, but since it's a translation we will load it.");
			packagedModInfo = new PackagedModInfo
			{
				minimumSupportedBuild = 0
			};
		}
		requiredDlcIds = packagedModInfo.requiredDlcIds;
		forbiddenDlcIds = packagedModInfo.forbiddenDlcIds;
		ArchivedVersion archivedVersion = new ArchivedVersion
		{
			relativePath = "",
			info = packagedModInfo
		};
		if (!file_source.Exists("archived_versions"))
		{
			ModDevLog($"\t{label}: No archived_versions for this mod, using root version directly.");
			if (!DlcManager.IsCorrectDlcSubscribed(packagedModInfo))
			{
				return null;
			}
			return archivedVersion;
		}
		List<FileSystemItem> list = new List<FileSystemItem>();
		file_source.GetTopLevelItems(list, "archived_versions");
		if (list.Count == 0)
		{
			ModDevLog($"\t{label}: No archived_versions for this mod, using root version directly.");
			if (!DlcManager.IsCorrectDlcSubscribed(packagedModInfo))
			{
				return null;
			}
			return archivedVersion;
		}
		List<ArchivedVersion> list2 = new List<ArchivedVersion>();
		list2.Add(archivedVersion);
		foreach (FileSystemItem item in list)
		{
			if (item.type != FileSystemItem.ItemType.File)
			{
				string relativePath = Path.Combine("archived_versions", item.name);
				PackagedModInfo modInfoForFolder = GetModInfoForFolder(relativePath);
				if (modInfoForFolder != null)
				{
					list2.Add(new ArchivedVersion
					{
						relativePath = relativePath,
						info = modInfoForFolder
					});
				}
			}
		}
		list2 = list2.Where((ArchivedVersion v) => DlcManager.IsCorrectDlcSubscribed(v.info)).ToList();
		list2 = list2.Where((ArchivedVersion v) => v.info.APIVersion == 2 || v.info.APIVersion == 0).ToList();
		ArchivedVersion archivedVersion2 = (from v in list2
			where (long)v.info.minimumSupportedBuild <= 744825L
			orderby v.info.minimumSupportedBuild descending
			select v).FirstOrDefault();
		if (archivedVersion2 != null)
		{
			requiredDlcIds = archivedVersion2.info.requiredDlcIds;
			forbiddenDlcIds = archivedVersion2.info.forbiddenDlcIds;
		}
		if (archivedVersion2 == null)
		{
			return null;
		}
		return archivedVersion2;
	}

	private PackagedModInfo GetModInfoForFolder(string relative_root)
	{
		List<FileSystemItem> list = new List<FileSystemItem>();
		file_source.GetTopLevelItems(list, relative_root);
		bool flag = false;
		foreach (FileSystemItem item in list)
		{
			if (item.type == FileSystemItem.ItemType.File && item.name.ToLower() == "mod_info.yaml")
			{
				flag = true;
				break;
			}
		}
		string text = (string.IsNullOrEmpty(relative_root) ? "root" : relative_root);
		if (!flag)
		{
			ModDevLogWarning("\t" + title + ": has no mod_info.yaml in folder '" + text + "'");
			return null;
		}
		string text2 = file_source.Read(Path.Combine(relative_root, "mod_info.yaml"));
		if (string.IsNullOrEmpty(text2))
		{
			ModDevLogError(string.Format("\t{0}: Failed to read {1} in folder '{2}', skipping", label, "mod_info.yaml", text));
			return null;
		}
		YamlIO.ErrorHandler handle_error = delegate(YamlIO.Error e, bool force_warning)
		{
			YamlIO.LogError(e, !IsDev);
		};
		PackagedModInfo packagedModInfo = YamlIO.Parse<PackagedModInfo>(text2, default(FileHandle), handle_error);
		if (packagedModInfo == null)
		{
			ModDevLogError(string.Format("\t{0}: Failed to parse {1} in folder '{2}', text is {3}", label, "mod_info.yaml", text, text2));
			return null;
		}
		if (packagedModInfo.supportedContent != null && packagedModInfo.requiredDlcIds == null && packagedModInfo.forbiddenDlcIds == null)
		{
			packagedModInfo.supportedContent = packagedModInfo.supportedContent.ToUpperInvariant();
			ModDevLogWarning(string.Format("\t{0}: {1} in folder '{2}' is using supportedContent which has been deprecated. See stickied post on the Klei forums.", label, "mod_info.yaml", text));
			bool num = packagedModInfo.supportedContent.Contains("ALL");
			bool flag2 = packagedModInfo.supportedContent.Contains("VANILLA_ID");
			bool flag3 = packagedModInfo.supportedContent.Contains("EXPANSION1_ID");
			if (num)
			{
				packagedModInfo.requiredDlcIds = null;
				packagedModInfo.forbiddenDlcIds = null;
			}
			else
			{
				string pattern = "\\b\\w+_ID\\b";
				List<string> list2 = new List<string>();
				foreach (Match item2 in Regex.Matches(packagedModInfo.supportedContent, pattern))
				{
					if (!(item2.Value == "VANILLA_ID") && !(item2.Value == "EXPANSION1_ID" && flag2))
					{
						if (item2.Value != "EXPANSION1_ID")
						{
							ModDevLogWarning(string.Format("\t{0}: {1} in folder '{2}' found a DLC '{3}' it didn't recognize, ignoring.", label, "mod_info.yaml", text, item2.Value));
						}
						else
						{
							list2.Add(item2.Value);
						}
					}
				}
				if (list2.Count > 0)
				{
					packagedModInfo.requiredDlcIds = list2.ToArray();
				}
				if (!flag3)
				{
					packagedModInfo.forbiddenDlcIds = DlcManager.EXPANSION1;
				}
			}
		}
		if (packagedModInfo.requiredDlcIds != null)
		{
			for (int i = 0; i < packagedModInfo.requiredDlcIds.Length; i++)
			{
				packagedModInfo.requiredDlcIds[i] = packagedModInfo.requiredDlcIds[i].ToUpperInvariant();
				if (!DlcManager.IsDlcId(packagedModInfo.requiredDlcIds[i]))
				{
					ModDevLogWarning(string.Format("\t{0}: {1} in folder '{2}' is using an unrecognized DLC in requiredDlcIds '{3}'", label, "mod_info.yaml", text, packagedModInfo.requiredDlcIds[i]));
				}
			}
		}
		if (packagedModInfo.forbiddenDlcIds != null)
		{
			for (int j = 0; j < packagedModInfo.forbiddenDlcIds.Length; j++)
			{
				packagedModInfo.forbiddenDlcIds[j] = packagedModInfo.forbiddenDlcIds[j].ToUpperInvariant();
				if (!DlcManager.IsDlcId(packagedModInfo.forbiddenDlcIds[j]))
				{
					ModDevLogWarning(string.Format("\t{0}: {1} in folder '{2}' is using an unrecognized DLC in forbiddenDlcIds '{3}'", label, "mod_info.yaml", text, packagedModInfo.forbiddenDlcIds[j]));
				}
			}
		}
		if (packagedModInfo.lastWorkingBuild != 0)
		{
			ModDevLogError(string.Format("\t{0}: {1} in folder '{2}' is using `{3}`, please upgrade this to `{4}`", label, "mod_info.yaml", text, "lastWorkingBuild", "minimumSupportedBuild"));
			if (packagedModInfo.minimumSupportedBuild == 0)
			{
				packagedModInfo.minimumSupportedBuild = packagedModInfo.lastWorkingBuild;
			}
		}
		ModDevLog($"\t{label}: Found valid mod_info.yaml in folder '{text}': requiredDlcIds='{packagedModInfo.requiredDlcIds.DebugToCommaSeparatedList()}', forbiddenDlcIds='{packagedModInfo.forbiddenDlcIds.DebugToCommaSeparatedList()}' at {packagedModInfo.minimumSupportedBuild}");
		return packagedModInfo;
	}

	private bool ScanContentFromSource(string relativeRoot, out Content available)
	{
		available = (Content)0;
		List<FileSystemItem> list = new List<FileSystemItem>();
		file_source.GetTopLevelItems(list, relativeRoot);
		foreach (FileSystemItem item in list)
		{
			if (item.type == FileSystemItem.ItemType.Directory)
			{
				string directory = item.name.ToLower();
				available |= AddDirectory(directory);
			}
			else
			{
				string file = item.name.ToLower();
				available |= AddFile(file);
			}
		}
		return available != (Content)0;
	}

	private bool ScanContentFromSourceForTranslationsOnly(string relativeRoot)
	{
		available_content = (Content)0;
		List<FileSystemItem> list = new List<FileSystemItem>();
		file_source.GetTopLevelItems(list, relativeRoot);
		foreach (FileSystemItem item in list)
		{
			if (item.type == FileSystemItem.ItemType.File && item.name.ToLower().EndsWith(".po"))
			{
				available_content |= Content.Translation;
			}
		}
		return available_content != (Content)0;
	}

	public bool IsEmpty()
	{
		return available_content == (Content)0;
	}

	private Content AddDirectory(string directory)
	{
		Content content = (Content)0;
		switch (directory.TrimEnd('/'))
		{
		case "strings":
			content |= Content.Strings;
			break;
		case "codex":
			content |= Content.LayerableFiles;
			break;
		case "elements":
			content |= Content.LayerableFiles;
			break;
		case "templates":
			content |= Content.LayerableFiles;
			break;
		case "worldgen":
			content |= Content.LayerableFiles;
			break;
		case "buildingfacades":
			content |= Content.Animation;
			break;
		case "anim":
			content |= Content.Animation;
			break;
		}
		return content;
	}

	private Content AddFile(string file)
	{
		Content content = (Content)0;
		if (file.EndsWith(".dll"))
		{
			content |= Content.DLL;
		}
		if (file.EndsWith(".po"))
		{
			content |= Content.Translation;
		}
		return content;
	}

	private static void AccumulateExtensions(Content content, List<string> extensions)
	{
		if ((content & Content.DLL) != 0)
		{
			extensions.Add(".dll");
		}
		if ((content & (Content.Strings | Content.Translation)) != 0)
		{
			extensions.Add(".po");
		}
	}

	[Conditional("DEBUG")]
	private void Assert(bool condition, string failure_message)
	{
		if (string.IsNullOrEmpty(title))
		{
			DebugUtil.Assert(condition, string.Format("{2}\n\t{0}\n\t{1}", title, label.ToString(), failure_message));
		}
		else
		{
			DebugUtil.Assert(condition, string.Format("{1}\n\t{0}", label.ToString(), failure_message));
		}
	}

	public void Install()
	{
		if (IsLocal)
		{
			status = Status.Installed;
			return;
		}
		if (status != Status.CannotInstall)
		{
			status = Status.ReinstallPending;
		}
		if (file_source != null && DeleteInstallPathDirectory() && CreateInstallPathDirectory() && file_source.TryCopyTo(label.install_path))
		{
			file_source = new Directory(label.install_path);
			status = Status.Installed;
		}
	}

	private bool DeleteInstallPathDirectory()
	{
		return FileUtil.DeleteDirectory(label.install_path);
	}

	private bool CreateInstallPathDirectory()
	{
		return FileUtil.CreateDirectory(label.install_path);
	}

	public bool Uninstall()
	{
		SetEnabledForActiveDlc(enabled: false);
		bool flag = true;
		if (loaded_content != 0)
		{
			Debug.Log($"Can't uninstall {label.ToString()}: still has loaded content: {loaded_content.ToString()}");
			flag = false;
		}
		if (!IsLocal && !DeleteInstallPathDirectory())
		{
			Debug.Log($"Can't uninstall {label.ToString()}: directory deletion failed");
			flag = false;
		}
		if (flag)
		{
			status = Status.NotInstalled;
		}
		else
		{
			switch (status)
			{
			case Status.Installed:
				status = Status.UninstallPending;
				break;
			}
		}
		return flag;
	}

	private bool LoadStrings()
	{
		string path = FileSystem.Normalize(Path.Combine(ContentPath, "strings"));
		if (!System.IO.Directory.Exists(path))
		{
			return false;
		}
		int num = 0;
		FileInfo[] files = new DirectoryInfo(path).GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (!(fileInfo.Extension.ToLower() != ".po"))
			{
				num++;
				Localization.OverloadStrings(Localization.LoadStringsFile(fileInfo.FullName, isTemplate: false));
			}
		}
		return true;
	}

	private bool LoadTranslations()
	{
		return false;
	}

	private bool LoadAnimation()
	{
		string path = FileSystem.Normalize(Path.Combine(ContentPath, "anim"));
		if (!System.IO.Directory.Exists(path))
		{
			return false;
		}
		int num = 0;
		DirectoryInfo[] directories = new DirectoryInfo(path).GetDirectories();
		for (int i = 0; i < directories.Length; i++)
		{
			DirectoryInfo[] directories2 = directories[i].GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories2)
			{
				KAnimFile.Mod mod = new KAnimFile.Mod();
				FileInfo[] files = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in files)
				{
					if (fileInfo.Name.StartsWith("._"))
					{
						continue;
					}
					if (fileInfo.Extension == ".png")
					{
						byte[] data = File.ReadAllBytes(fileInfo.FullName);
						Texture2D texture2D = new Texture2D(2, 2);
						texture2D.LoadImage(data);
						mod.textures.Add(texture2D);
					}
					else if (fileInfo.Extension == ".bytes")
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
						byte[] array = File.ReadAllBytes(fileInfo.FullName);
						if (fileNameWithoutExtension.EndsWith("_anim"))
						{
							mod.anim = array;
							continue;
						}
						if (fileNameWithoutExtension.EndsWith("_build"))
						{
							mod.build = array;
							continue;
						}
						DebugUtil.LogWarningArgs($"Unhandled TextAsset ({fileInfo.FullName})...ignoring");
					}
					else
					{
						DebugUtil.LogWarningArgs($"Unhandled asset ({fileInfo.FullName})...ignoring");
					}
				}
				string name = directoryInfo.Name + "_kanim";
				if (mod.IsValid() && (bool)ModUtil.AddKAnimMod(name, mod))
				{
					num++;
				}
			}
		}
		return true;
	}

	public void Load(Content content)
	{
		content &= available_content & (Content)(~(uint)loaded_content);
		if ((int)content > 0)
		{
			Debug.Log(string.Format("Loading mod content {2} [{0}:{1}] (provides {3})", title, label.id, content.ToString(), available_content.ToString()));
		}
		if ((content & Content.Strings) != 0 && LoadStrings())
		{
			loaded_content |= Content.Strings;
		}
		if ((content & Content.Translation) != 0 && LoadTranslations())
		{
			loaded_content |= Content.Translation;
		}
		if ((content & Content.DLL) != 0)
		{
			loaded_mod_data = DLLLoader.LoadDLLs(this, staticID, ContentPath, IsDev);
			if (loaded_mod_data != null)
			{
				loaded_content |= Content.DLL;
			}
		}
		if ((content & Content.LayerableFiles) != 0)
		{
			Debug.Assert(content_source != null, "Attempting to Load layerable files with content_source not initialized");
			FileSystem.file_sources.Insert(0, content_source.GetFileSystem());
			loaded_content |= Content.LayerableFiles;
		}
		if ((content & Content.Animation) != 0 && LoadAnimation())
		{
			loaded_content |= Content.Animation;
		}
	}

	public void PostLoad(IReadOnlyList<Mod> mods)
	{
		if ((loaded_content & Content.DLL) != 0 && loaded_mod_data != null)
		{
			DLLLoader.PostLoadDLLs(staticID, loaded_mod_data, mods);
		}
	}

	public void Unload(Content content)
	{
		content &= loaded_content;
		if ((content & Content.LayerableFiles) != 0)
		{
			FileSystem.file_sources.Remove(content_source.GetFileSystem());
			loaded_content &= ~Content.LayerableFiles;
		}
	}

	private void SetCrashCount(int new_crash_count)
	{
		crash_count = MathUtil.Clamp(0, 3, new_crash_count);
	}

	public void SetCrashed()
	{
		SetCrashCount(crash_count + 1);
		if (!IsDev)
		{
			SetEnabledForActiveDlc(enabled: false);
		}
	}

	public void Uncrash()
	{
		SetCrashCount(IsDev ? (crash_count - 1) : 0);
	}

	public bool IsActive()
	{
		return loaded_content != (Content)0;
	}

	public bool AllActive(Content content)
	{
		return (loaded_content & content) == content;
	}

	public bool AllActive()
	{
		return (loaded_content & available_content) == available_content;
	}

	public bool AnyActive(Content content)
	{
		return (loaded_content & content) != 0;
	}

	public bool HasContent()
	{
		return available_content != (Content)0;
	}

	public bool HasAnyContent(Content content)
	{
		return (available_content & content) != 0;
	}

	public bool HasOnlyTranslationContent()
	{
		return available_content == Content.Translation;
	}

	public Texture2D GetPreviewImage()
	{
		string text = null;
		foreach (string pREVIEW_FILENAME in PREVIEW_FILENAMES)
		{
			if (System.IO.Directory.Exists(ContentPath) && File.Exists(Path.Combine(ContentPath, pREVIEW_FILENAME)))
			{
				text = pREVIEW_FILENAME;
				break;
			}
		}
		if (text == null)
		{
			return null;
		}
		try
		{
			byte[] data = File.ReadAllBytes(Path.Combine(ContentPath, text));
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
			return texture2D;
		}
		catch
		{
			Debug.LogWarning($"Mod {label} seems to have a preview.png but it didn't load correctly.");
			return null;
		}
	}

	public void ModDevLog(string msg)
	{
		if (IsDev)
		{
			Debug.Log(msg);
		}
	}

	public void ModDevLogWarning(string msg)
	{
		if (IsDev)
		{
			Debug.LogWarning(msg);
		}
	}

	public void ModDevLogError(string msg)
	{
		if (IsDev)
		{
			DevModCrashTriggered = true;
			Debug.LogError(msg);
		}
	}
}
