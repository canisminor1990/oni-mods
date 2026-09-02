===== KAnimGroupFile =====
using System;
using System.Collections.Generic;
using UnityEngine;

public class KAnimGroupFile : ScriptableObject
{
	[Serializable]
	public class Group
	{
		[SerializeField]
		public HashedString id;

		[SerializeField]
		public string commandDirectory = "";

		[SerializeField]
		public List<HashedString> animNames = new List<HashedString>();

		[SerializeField]
		public KAnimBatchGroup.RendererType renderType;

		[SerializeField]
		public int maxVisibleSymbols;

		[SerializeField]
		public int maxGroupSize;

		[SerializeField]
		public HashedString target;

		[SerializeField]
		public HashedString swapTarget;

		[SerializeField]
		public HashedString animTarget;

		[NonSerialized]
		public List<KAnimFile> animFiles = new List<KAnimFile>();

		public Group(HashedString tag)
		{
			id = tag;
		}
	}

	public class GroupFile
	{
		public string groupID { get; set; }

		public string commandDirectory { get; set; }
	}

	public enum AddModResult
	{
		Added,
		Replaced
	}

	private const string MASTER_GROUP_FILE = "animgrouptags";

	public const int MAX_ANIMS_PER_GROUP = 10;

	private static KAnimGroupFile groupfile;

	private Dictionary<int, KAnimFileData> fileData = new Dictionary<int, KAnimFileData>();

	[SerializeField]
	private List<Group> groups = new List<Group>();

	[SerializeField]
	private List<Pair<HashedString, HashedString>> currentGroup = new List<Pair<HashedString, HashedString>>();

	public static void DestroyInstance()
	{
		groupfile = null;
	}

	public static string GetFilePath(string contentDir = "")
	{
		if (string.IsNullOrEmpty(contentDir))
		{
			return "Assets/anim/base/resources/animgrouptags.asset";
		}
		return "Assets/anim" + $"/{contentDir}/" + "animgrouptags.asset";
	}

	public static KAnimGroupFile GetGroupFile()
	{
		Debug.Assert(groupfile != null, "Cannot GetGroupFile before it is loaded.");
		return groupfile;
	}

	public static Group GetGroup(HashedString tag)
	{
		Debug.Assert(groupfile != null, "GetGroup called before LoadAll called");
		List<Group> list = groupfile.groups;
		Debug.Assert(list != null);
		for (int i = 0; i < list.Count; i++)
		{
			Group group = list[i];
			if (group.id == tag || group.target == tag)
			{
				return group;
			}
		}
		return null;
	}

	public static HashedString GetGroupForHomeDirectory(HashedString homedirectory)
	{
		List<Pair<HashedString, HashedString>> list = groupfile.currentGroup;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].first == homedirectory)
			{
				return list[i].second;
			}
		}
		return default(HashedString);
	}

	public List<Group> GetData()
	{
		return groups;
	}

	public void Reset()
	{
		groups = new List<Group>();
		currentGroup = new List<Pair<HashedString, HashedString>>();
	}

	private int AddGroup(AnimCommandFile akf, GroupFile gf, KAnimFile file)
	{
		bool flag = akf.IsSwap(file);
		HashedString groupId = new HashedString(gf.groupID);
		int num = groups.FindIndex((Group t) => t.id == groupId);
		if (num == -1)
		{
			num = groups.Count;
			Group group = new Group(groupId);
			group.commandDirectory = akf.directory;
			group.maxGroupSize = akf.MaxGroupSize;
			group.renderType = akf.RendererType;
			if (groups.FindIndex((Group t) => t.commandDirectory == group.commandDirectory) == -1)
			{
				if (flag)
				{
					if (!string.IsNullOrEmpty(akf.TargetBuild))
					{
						group.target = new HashedString(akf.TargetBuild);
					}
					if (group.renderType != KAnimBatchGroup.RendererType.DontRender)
					{
						group.renderType = KAnimBatchGroup.RendererType.DontRender;
						group.swapTarget = new HashedString(akf.SwapTargetBuild);
					}
				}
				if (akf.Type == AnimCommandFile.ConfigType.AnimOnly)
				{
					group.target = new HashedString(akf.TargetBuild);
					group.renderType = KAnimBatchGroup.RendererType.AnimOnly;
					group.animTarget = new HashedString(akf.AnimTargetBuild);
					group.swapTarget = new HashedString(akf.SwapTargetBuild);
				}
				if (akf.Type == AnimCommandFile.ConfigType.BuildAndAnim)
				{
					group.renderType = KAnimBatchGroup.RendererType.BuildAndAnims;
				}
			}
			groups.Add(group);
		}
		return num;
	}

	public bool AddAnimFile(GroupFile gf, AnimCommandFile akf, KAnimFile file)
	{
		Debug.Assert(gf != null);
		Debug.Assert(file != null, gf.groupID);
		Debug.Assert(akf != null, gf.groupID);
		int groupIndex = AddGroup(akf, gf, file);
		return AddFile(groupIndex, file);
	}

	private bool AddFile(int groupIndex, KAnimFile file)
	{
		if (!groups[groupIndex].animNames.Contains(file.name))
		{
			Pair<HashedString, HashedString> pair = new Pair<HashedString, HashedString>(file.homedirectory, groups[groupIndex].id);
			bool flag = false;
			for (int i = 0; i < currentGroup.Count; i++)
			{
				if (currentGroup[i].first == file.homedirectory)
				{
					currentGroup[i] = pair;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				currentGroup.Add(pair);
			}
			groups[groupIndex].animFiles.Add(file);
			groups[groupIndex].animNames.Add(file.name);
			return true;
		}
		return false;
	}

	public AddModResult AddAnimMod(GroupFile gf, AnimCommandFile akf, KAnimFile file)
	{
		Debug.Assert(gf != null);
		Debug.Assert(file != null, gf.groupID);
		Debug.Assert(akf != null, gf.groupID);
		int index = AddGroup(akf, gf, file);
		string name = file.GetData().name;
		int num = groups[index].animFiles.FindIndex((KAnimFile candidate) => candidate != null && candidate.GetData().name == name);
		if (num == -1)
		{
			groups[index].animFiles.Add(file);
			groups[index].animNames.Add(file.GetData().name);
			return AddModResult.Added;
		}
		groups[index].animFiles[num].mod = file.mod;
		return AddModResult.Replaced;
	}

	public static void LoadGroupResourceFile()
	{
		groupfile = (KAnimGroupFile)Resources.Load("animgrouptags", typeof(KAnimGroupFile));
	}

	public static void LoadAll()
	{
		groupfile.Load();
	}

	public static void MapNamesToAnimFiles(Dictionary<HashedString, KAnimFile> animTable)
	{
		groupfile.DoMapNamesToAnimFiles(animTable);
	}

	private void DoMapNamesToAnimFiles(Dictionary<HashedString, KAnimFile> animTable)
	{
		for (int i = 0; i < groups.Count; i++)
		{
			groups[i].animFiles = new List<KAnimFile>();
			for (int j = 0; j < groups[i].animNames.Count; j++)
			{
				HashedString key = groups[i].animNames[j];
				KAnimFile value = null;
				animTable.TryGetValue(key, out value);
				if (value != null)
				{
					groups[i].animFiles.Add(value);
				}
			}
		}
	}

	private void Load()
	{
		fileData.Clear();
		for (int i = 0; i < groups.Count; i++)
		{
			if (!groups[i].id.IsValid)
			{
				Debug.LogErrorFormat("Group invalid groupIndex [{0}]", i);
			}
			KBatchGroupData kBatchGroupData = null;
			kBatchGroupData = ((!groups[i].target.IsValid) ? KAnimBatchManager.Instance().GetBatchGroupData(groups[i].id) : KAnimBatchManager.Instance().GetBatchGroupData(groups[i].target));
			HashedString batchTag = groups[i].id;
			if (groups[i].renderType == KAnimBatchGroup.RendererType.AnimOnly)
			{
				if (!groups[i].swapTarget.IsValid)
				{
					continue;
				}
				kBatchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(groups[i].swapTarget);
				batchTag = groups[i].swapTarget;
			}
			for (int j = 0; j < groups[i].animFiles.Count; j++)
			{
				KAnimFile kAnimFile = groups[i].animFiles[j];
				if (!(kAnimFile != null))
				{
					continue;
				}
				byte[] buildBytes = kAnimFile.buildBytes;
				if (buildBytes != null && !fileData.ContainsKey(kAnimFile.GetInstanceID()))
				{
					if (buildBytes.Length == 0)
					{
						Debug.LogWarning("Build File [" + kAnimFile.GetData().name + "] has 0 bytes");
						continue;
					}
					HashedString hashedString = new HashedString(kAnimFile.name);
					HashCache.Get().Add(hashedString.HashValue, kAnimFile.name);
					KAnimFileData file = KGlobalAnimParser.Get().GetFile(kAnimFile);
					file.maxVisSymbolFrames = 0;
					file.batchTag = batchTag;
					file.buildIndex = KGlobalAnimParser.ParseBuildData(kBatchGroupData, hashedString, new FastReader(buildBytes), kAnimFile.textureList);
					fileData.Add(kAnimFile.GetInstanceID(), file);
				}
			}
		}
		for (int k = 0; k < groups.Count; k++)
		{
			if (groups[k].renderType != KAnimBatchGroup.RendererType.AnimOnly)
			{
				continue;
			}
			KBatchGroupData batchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(groups[k].swapTarget);
			KBatchGroupData batchGroupData2 = KAnimBatchManager.Instance().GetBatchGroupData(groups[k].animTarget);
			for (int l = 0; l < batchGroupData.builds.Count; l++)
			{
				KAnim.Build build = batchGroupData.builds[l];
				if (build == null || build.symbols == null)
				{
					continue;
				}
				for (int m = 0; m < build.symbols.Length; m++)
				{
					KAnim.Build.Symbol symbol = build.symbols[m];
					if (symbol != null && symbol.hash.IsValid() && batchGroupData2.GetFirstIndex(symbol.hash) == -1)
					{
						KAnim.Build.Symbol symbol2 = new KAnim.Build.Symbol();
						symbol2.build = build;
						symbol2.hash = symbol.hash;
						symbol2.path = symbol.path;
						symbol2.colourChannel = symbol.colourChannel;
						symbol2.flags = symbol.flags;
						symbol2.firstFrameIdx = batchGroupData2.symbolFrameInstances.Count;
						symbol2.numFrames = symbol.numFrames;
						symbol2.symbolIndexInSourceBuild = batchGroupData2.frameElementSymbols.Count;
						for (int n = 0; n < symbol2.numFrames; n++)
						{
							KAnim.Build.SymbolFrameInstance symbolFrameInstance = batchGroupData.GetSymbolFrameInstance(n + symbol.firstFrameIdx);
							KAnim.Build.SymbolFrameInstance symbolFrameInstance2 = default(KAnim.Build.SymbolFrameInstance);
							symbolFrameInstance2 = symbolFrameInstance;
							symbolFrameInstance2.buildImageIdx = -1;
							symbolFrameInstance2.symbolIdx = batchGroupData2.GetSymbolCount();
							batchGroupData2.symbolFrameInstances.Add(symbolFrameInstance2);
						}
						batchGroupData2.AddBuildSymbol(symbol2);
					}
				}
			}
		}
		for (int num = 0; num < groups.Count; num++)
		{
			if (groups[num].renderType != KAnimBatchGroup.RendererType.BuildAndAnims)
			{
				continue;
			}
			KBatchGroupData batchGroupData3 = KAnimBatchManager.Instance().GetBatchGroupData(groups[num].id);
			int num2 = 0;
			Dictionary<HashedString, int> dictionary = new Dictionary<HashedString, int>();
			for (int num3 = 0; num3 < batchGroupData3.builds.Count; num3++)
			{
				KAnim.Build build2 = batchGroupData3.builds[num3];
				if (build2 == null || build2.symbols == null)
				{
					continue;
				}
				for (int num4 = 0; num4 < build2.symbols.Length; num4++)
				{
					KAnim.Build.Symbol symbol3 = build2.symbols[num4];
					if (symbol3 != null && symbol3.hash.IsValid())
					{
						Debug.Assert(num2 < batchGroupData3.maxSymbolsPerBuild, "Symbol count is larger than symbols in source builds");
						if (!dictionary.ContainsKey(symbol3.hash))
						{
							dictionary[symbol3.hash] = num2;
							num2++;
						}
						symbol3.symbolIndexInSourceBuild = dictionary[symbol3.hash];
					}
				}
			}
		}
		for (int num5 = 0; num5 < groups.Count; num5++)
		{
			if (!groups[num5].id.IsValid)
			{
				Debug.LogErrorFormat("Group invalid groupIndex [{0}]", num5);
			}
			if (groups[num5].renderType == KAnimBatchGroup.RendererType.DontRender)
			{
				continue;
			}
			KBatchGroupData kBatchGroupData2 = null;
			if (groups[num5].animTarget.IsValid)
			{
				kBatchGroupData2 = KAnimBatchManager.Instance().GetBatchGroupData(groups[num5].animTarget);
				if (kBatchGroupData2 == null)
				{
					Debug.LogErrorFormat("Anim group is null for [{0}] -> [{1}]", groups[num5].id, groups[num5].animTarget);
				}
			}
			else
			{
				kBatchGroupData2 = KAnimBatchManager.Instance().GetBatchGroupData(groups[num5].id);
				if (kBatchGroupData2 == null)
				{
					Debug.LogErrorFormat("Anim group is null for [{0}]", groups[num5].id);
				}
			}
			for (int num6 = 0; num6 < groups[num5].animFiles.Count; num6++)
			{
				KAnimFile kAnimFile2 = groups[num5].animFiles[num6];
				if (!(kAnimFile2 != null))
				{
					continue;
				}
				byte[] animBytes = kAnimFile2.animBytes;
				if (animBytes == null)
				{
					continue;
				}
				if (animBytes.Length == 0)
				{
					Debug.LogWarning("Anim File [" + kAnimFile2.GetData().name + "] has 0 bytes");
					continue;
				}
				if (!fileData.ContainsKey(kAnimFile2.GetInstanceID()))
				{
					KAnimFileData file2 = KGlobalAnimParser.Get().GetFile(kAnimFile2);
					file2.maxVisSymbolFrames = 0;
					file2.batchTag = groups[num5].id;
					fileData.Add(kAnimFile2.GetInstanceID(), file2);
				}
				HashedString fileNameHash = new HashedString(kAnimFile2.name);
				FastReader reader = new FastReader(animBytes);
				KAnimFileData animFile = fileData[kAnimFile2.GetInstanceID()];
				KGlobalAnimParser.ParseAnimData(kBatchGroupData2, fileNameHash, reader, animFile);
			}
		}
		for (int num7 = 0; num7 < groups.Count; num7++)
		{
			if (!groups[num7].id.IsValid)
			{
				Debug.LogErrorFormat("Group invalid groupIndex [{0}]", num7);
			}
			KBatchGroupData kBatchGroupData3 = null;
			if (groups[num7].target.IsValid)
			{
				kBatchGroupData3 = KAnimBatchManager.Instance().GetBatchGroupData(groups[num7].target);
				if (kBatchGroupData3 == null)
				{
					Debug.LogErrorFormat("Group is null for  [{0}] target [{1}]", groups[num7].id, groups[num7].target);
				}
			}
			else
			{
				kBatchGroupData3 = KAnimBatchManager.Instance().GetBatchGroupData(groups[num7].id);
				if (kBatchGroupData3 == null)
				{
					Debug.LogErrorFormat("Group is null for [{0}]", groups[num7].id);
				}
			}
			KGlobalAnimParser.PostParse(kBatchGroupData3);
		}
	}
}

===== KGlobalAnimParser =====
using System;
using System.Collections.Generic;
using System.IO;
using Klei;
using UnityEngine;

public class KGlobalAnimParser
{
	public static KAnimHashedString MISSING_SYMBOL = new KAnimHashedString("MISSING_SYMBOL");

	public static string ANIM_COMMAND_FILE = "batchgroup.yaml";

	private static KAnimHashedString ANIM_HASH_HEAD_ANIM = "head_anim";

	public const float ANIM_SCALE = 0.005f;

	private Dictionary<HashedString, AnimCommandFile> commandFiles = new Dictionary<HashedString, AnimCommandFile>();

	private Dictionary<int, KAnimFileData> files = new Dictionary<int, KAnimFileData>();

	private static KGlobalAnimParser instance => Singleton<KGlobalAnimParser>.Instance;

	public static void CreateInstance()
	{
		Singleton<KGlobalAnimParser>.CreateInstance();
	}

	public static KGlobalAnimParser Get()
	{
		return instance;
	}

	public static void DestroyInstance()
	{
		if (instance != null)
		{
			instance.commandFiles.Clear();
			instance.commandFiles = null;
			instance.files.Clear();
			instance.files = null;
		}
		Singleton<KGlobalAnimParser>.DestroyInstance();
	}

	public KAnimFileData GetFile(KAnimFile anim_file)
	{
		KAnimFileData value = null;
		int instanceID = anim_file.GetInstanceID();
		if (!files.TryGetValue(instanceID, out value))
		{
			value = new KAnimFileData(anim_file.name);
			files[instanceID] = value;
		}
		return value;
	}

	public KAnimFileData Load(KAnimFile anim_file)
	{
		KAnimFileData value = null;
		int instanceID = anim_file.GetInstanceID();
		if (!files.TryGetValue(instanceID, out value))
		{
			value = GetFile(anim_file);
		}
		return value;
	}

	public static AnimCommandFile GetParseCommands(string path)
	{
		string fullName = Directory.GetParent(path).FullName;
		HashedString key = new HashedString(fullName);
		if (Get().commandFiles.ContainsKey(key))
		{
			return instance.commandFiles[key];
		}
		string text = Path.Combine(fullName, ANIM_COMMAND_FILE);
		if (File.Exists(text))
		{
			AnimCommandFile animCommandFile = YamlIO.LoadFile<AnimCommandFile>(text);
			animCommandFile.directory = "Assets/anim/" + Directory.GetParent(path).Name;
			instance.commandFiles[key] = animCommandFile;
			return animCommandFile;
		}
		return null;
	}

	public static void ParseAnimData(KBatchGroupData data, HashedString fileNameHash, FastReader reader, KAnimFileData animFile)
	{
		CheckHeader("ANIM", reader);
		Assert(reader.ReadUInt32() == 5, "Invalid anim.bytes version");
		reader.ReadInt32();
		reader.ReadInt32();
		int num = reader.ReadInt32();
		animFile.maxVisSymbolFrames = 0;
		animFile.animCount = 0;
		animFile.frameCount = 0;
		animFile.elementCount = 0;
		animFile.firstAnimIndex = data.anims.Count;
		animFile.animBatchTag = data.groupID;
		data.animIndex.Add(fileNameHash, data.anims.Count);
		animFile.firstElementIndex = data.frameElements.Count;
		for (int i = 0; i < num; i++)
		{
			KAnim.Anim anim = new KAnim.Anim(animFile, data.anims.Count);
			anim.name = reader.ReadKleiString();
			string text = animFile.name + "." + anim.name;
			anim.id = text;
			HashCache.Get().Add(anim.name);
			HashCache.Get().Add(text);
			anim.hash = anim.name;
			anim.rootSymbol.HashValue = reader.ReadInt32();
			anim.frameRate = reader.ReadSingle();
			anim.firstFrameIdx = data.animFrames.Count;
			anim.numFrames = reader.ReadInt32();
			anim.totalTime = (float)anim.numFrames / anim.frameRate;
			anim.scaledBoundingRadius = 0f;
			for (int j = 0; j < anim.numFrames; j++)
			{
				KAnim.Anim.Frame item = default(KAnim.Anim.Frame);
				float num2 = reader.ReadSingle();
				float num3 = reader.ReadSingle();
				float num4 = reader.ReadSingle();
				float num5 = reader.ReadSingle();
				Vector2 vector = new Vector2(num2 - num4 * 0.5f, 0f - (num3 + num5 * 0.5f)) * 0.005f;
				Vector2 vector2 = new Vector2(num2 + num4 * 0.5f, 0f - (num3 - num5 * 0.5f)) * 0.005f;
				float num6 = Math.Max(Math.Abs(vector2.x), Math.Abs(vector.x));
				float num7 = Math.Max(Math.Abs(vector2.y), Math.Abs(vector.y));
				float num8 = Math.Max(num6, num7);
				anim.unScaledSize.x = Math.Max(anim.unScaledSize.x, num6 / 0.005f);
				anim.unScaledSize.y = Math.Max(anim.unScaledSize.y, num7 / 0.005f);
				anim.scaledBoundingRadius = Math.Max(anim.scaledBoundingRadius, Mathf.Sqrt(num8 * num8 + num8 * num8));
				item.firstElementIdx = data.frameElements.Count;
				item.numElements = reader.ReadInt32();
				item.hasHead = false;
				int num9 = 0;
				for (int k = 0; k < item.numElements; k++)
				{
					KAnim.Anim.FrameElement item2 = default(KAnim.Anim.FrameElement);
					item2.symbol = new KAnimHashedString(reader.ReadInt32());
					item2.frame = reader.ReadInt32();
					if (new KAnimHashedString(reader.ReadInt32()) == ANIM_HASH_HEAD_ANIM)
					{
						item.hasHead = true;
					}
					reader.ReadInt32();
					float num10 = reader.ReadSingle();
					float num11 = reader.ReadSingle();
					float num12 = reader.ReadSingle();
					float num13 = reader.ReadSingle();
					DebugUtil.DevAssert(num13 == num11 && num11 == num12, "Unhandled color values!");
					item2.multAlpha = num13 * num10;
					float m = reader.ReadSingle();
					float m2 = reader.ReadSingle();
					float m3 = reader.ReadSingle();
					float m4 = reader.ReadSingle();
					float m5 = reader.ReadSingle();
					float m6 = reader.ReadSingle();
					reader.ReadSingle();
					item2.transform.m00 = m;
					item2.transform.m01 = m3;
					item2.transform.m02 = m5;
					item2.transform.m10 = m2;
					item2.transform.m11 = m4;
					item2.transform.m12 = m6;
					if (data.GetSymbolIndex(item2.symbol) == -1)
					{
						num9++;
						item2.symbol = MISSING_SYMBOL;
					}
					else
					{
						data.frameElements.Add(item2);
						animFile.elementCount++;
					}
				}
				item.numElements -= num9;
				data.animFrames.Add(item);
				animFile.frameCount++;
			}
			data.AddAnim(anim);
			animFile.animCount++;
		}
		Debug.Assert(num == animFile.animCount);
		animFile.maxVisSymbolFrames = Math.Max(animFile.maxVisSymbolFrames, reader.ReadInt32());
		data.UpdateMaxVisibleSymbols(animFile.maxVisSymbolFrames);
		ParseHashTable(reader);
	}

	private static void ParseHashTable(FastReader reader)
	{
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int hash = reader.ReadInt32();
			string text = reader.ReadKleiString();
			HashCache.Get().Add(hash, text);
		}
	}

	public static int ParseBuildData(KBatchGroupData data, KAnimHashedString fileNameHash, FastReader reader, List<Texture2D> textures)
	{
		CheckHeader("BILD", reader);
		int num = reader.ReadInt32();
		if (num != 10 && num != 9)
		{
			KAnimHashedString kAnimHashedString = fileNameHash;
			Debug.LogError(kAnimHashedString.ToString() + " has invalid build.bytes version [" + num + "]");
			return -1;
		}
		KAnimGroupFile.Group group = KAnimGroupFile.GetGroup(data.groupID);
		if (group == null)
		{
			Debug.LogErrorFormat("[{1}] Failed to get group [{0}]", data.groupID, fileNameHash.DebuggerDisplay);
		}
		KAnim.Build build = null;
		int num2 = reader.ReadInt32();
		reader.ReadInt32();
		build = data.AddNewBuildFile(fileNameHash);
		build.textureCount = textures.Count;
		if (textures.Count > 0)
		{
			data.AddTextures(textures);
		}
		build.symbols = new KAnim.Build.Symbol[num2];
		build.name = reader.ReadKleiString();
		build.batchTag = (group.swapTarget.IsValid ? group.target : data.groupID);
		build.fileHash = fileNameHash;
		for (int i = 0; i < build.symbols.Length; i++)
		{
			KAnimHashedString hash = new KAnimHashedString(reader.ReadInt32());
			KAnim.Build.Symbol symbol = new KAnim.Build.Symbol();
			symbol.build = build;
			symbol.hash = hash;
			if (num > 9)
			{
				symbol.path = new KAnimHashedString(reader.ReadInt32());
			}
			symbol.colourChannel = new KAnimHashedString(reader.ReadInt32());
			symbol.flags = reader.ReadInt32();
			symbol.firstFrameIdx = data.symbolFrameInstances.Count;
			symbol.numFrames = reader.ReadInt32();
			symbol.symbolIndexInSourceBuild = i;
			int num3 = 0;
			for (int j = 0; j < symbol.numFrames; j++)
			{
				KAnim.Build.SymbolFrameInstance item = default(KAnim.Build.SymbolFrameInstance);
				item.sourceFrameNum = reader.ReadInt32();
				item.duration = reader.ReadInt32();
				item.buildImageIdx = data.textureStartIndex[fileNameHash] + reader.ReadInt32();
				if (item.buildImageIdx >= textures.Count + data.textureStartIndex[fileNameHash])
				{
					Debug.LogErrorFormat("{0} Symbol: [{1}] tex count: [{2}] buildImageIdx: [{3}] group total [{4}]", fileNameHash.ToString(), symbol.hash, textures.Count, item.buildImageIdx, data.textureStartIndex[fileNameHash]);
				}
				item.symbolIdx = data.GetSymbolCount();
				num3 = Math.Max(item.sourceFrameNum + item.duration, num3);
				float num4 = reader.ReadSingle();
				float num5 = reader.ReadSingle();
				float num6 = reader.ReadSingle();
				float num7 = reader.ReadSingle();
				item.bboxMin = new Vector2(num4 - num6 * 0.5f, num5 - num7 * 0.5f);
				item.bboxMax = new Vector2(num4 + num6 * 0.5f, num5 + num7 * 0.5f);
				float x = reader.ReadSingle();
				float num8 = reader.ReadSingle();
				float x2 = reader.ReadSingle();
				float num9 = reader.ReadSingle();
				item.uvMin = new Vector2(x, 1f - num8);
				item.uvMax = new Vector2(x2, 1f - num9);
				data.symbolFrameInstances.Add(item);
			}
			symbol.numLookupFrames = num3;
			data.AddBuildSymbol(symbol);
			build.symbols[i] = symbol;
		}
		ParseHashTable(reader);
		return build.index;
	}

	public static void PostParse(KBatchGroupData data)
	{
		for (int i = 0; i < data.GetSymbolCount(); i++)
		{
			KAnim.Build.Symbol symbol = data.GetSymbol(i);
			if (symbol == null)
			{
				Debug.LogWarning("Symbol null for [" + data.groupID.ToString() + "] idx: [" + i + "]");
				continue;
			}
			if (symbol.numLookupFrames <= 0)
			{
				int num = symbol.numFrames;
				for (int j = symbol.firstFrameIdx; j < symbol.firstFrameIdx + symbol.numFrames; j++)
				{
					KAnim.Build.SymbolFrameInstance symbolFrameInstance = data.GetSymbolFrameInstance(j);
					num = Mathf.Max(num, symbolFrameInstance.sourceFrameNum + symbolFrameInstance.duration);
				}
				symbol.numLookupFrames = num;
			}
			symbol.frameLookup = new int[symbol.numLookupFrames];
			if (symbol.numLookupFrames <= 0)
			{
				string[] obj = new string[9]
				{
					"No lookup frames for  [",
					data.groupID.ToString(),
					"] build: [",
					symbol.build.name,
					"] idx: [",
					i.ToString(),
					"] id: [",
					null,
					null
				};
				KAnimHashedString hash = symbol.hash;
				obj[7] = hash.ToString();
				obj[8] = "]";
				Debug.LogWarning(string.Concat(obj));
				continue;
			}
			for (int k = 0; k < symbol.numLookupFrames; k++)
			{
				symbol.frameLookup[k] = -1;
			}
			for (int l = symbol.firstFrameIdx; l < symbol.firstFrameIdx + symbol.numFrames; l++)
			{
				KAnim.Build.SymbolFrameInstance symbolFrameInstance2 = data.GetSymbolFrameInstance(l);
				for (int m = symbolFrameInstance2.sourceFrameNum; m < symbolFrameInstance2.sourceFrameNum + symbolFrameInstance2.duration; m++)
				{
					if (m >= symbol.frameLookup.Length)
					{
						string[] obj2 = new string[11]
						{
							"Too many lookup frames [",
							m.ToString(),
							">=",
							symbol.frameLookup.Length.ToString(),
							"] for  [",
							data.groupID.ToString(),
							"] idx: [",
							i.ToString(),
							"] id: [",
							null,
							null
						};
						KAnimHashedString hash = symbol.hash;
						obj2[9] = hash.ToString();
						obj2[10] = "]";
						Debug.LogWarning(string.Concat(obj2));
					}
					else
					{
						symbol.frameLookup[m] = l;
					}
				}
			}
			string text = HashCache.Get().Get(symbol.path);
			if (!string.IsNullOrEmpty(text))
			{
				int num2 = text.IndexOf("/");
				if (num2 != -1)
				{
					string text2 = text.Substring(0, num2);
					symbol.folder = new KAnimHashedString(text2);
					HashCache.Get().Add(symbol.folder.HashValue, text2);
				}
			}
		}
	}

	private static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			throw new Exception(message);
		}
	}

	private static void CheckHeader(string header, FastReader reader)
	{
		char[] array = reader.ReadChars(header.Length);
		for (int i = 0; i < header.Length; i++)
		{
			if (array[i] != header[i])
			{
				throw new Exception("Expected " + header);
			}
		}
	}
}

