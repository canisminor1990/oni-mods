===== AnimCommandFile =====
using System;
using System.Collections.Generic;
using System.IO;
using KSerialization.Converters;

public class AnimCommandFile
{
	public enum ConfigType
	{
		Default,
		AnimOnly,
		BuildAndAnim
	}

	public enum GroupBy
	{
		__IGNORE__,
		DontGroup,
		Folder,
		NamedGroup,
		NamedGroupNoSplit
	}

	[NonSerialized]
	public string directory = "";

	[NonSerialized]
	private List<KAnimGroupFile.GroupFile> groupFiles = new List<KAnimGroupFile.GroupFile>();

	[StringEnumConverter]
	public ConfigType Type { get; private set; }

	[StringEnumConverter]
	public GroupBy TagGroup { get; private set; }

	[StringEnumConverter]
	public KAnimBatchGroup.RendererType RendererType { get; private set; }

	public string TargetBuild { get; private set; }

	public string AnimTargetBuild { get; private set; }

	public string SwapTargetBuild { get; private set; }

	public Dictionary<string, List<string>> DefaultBuilds { get; private set; }

	public int MaxGroupSize { get; private set; }

	public AnimCommandFile()
	{
		MaxGroupSize = 30;
		DefaultBuilds = new Dictionary<string, List<string>>();
		TagGroup = GroupBy.DontGroup;
	}

	public bool IsSwap(KAnimFile file)
	{
		if (TagGroup != GroupBy.NamedGroup)
		{
			return false;
		}
		string fileName = Path.GetFileName(file.homedirectory);
		foreach (KeyValuePair<string, List<string>> defaultBuild in DefaultBuilds)
		{
			if (defaultBuild.Value.Contains(fileName))
			{
				return false;
			}
		}
		return true;
	}

	public void AddGroupFile(KAnimGroupFile.GroupFile gf)
	{
		if (!groupFiles.Contains(gf))
		{
			groupFiles.Add(gf);
		}
	}

	public string GetGroupName(KAnimFile kaf)
	{
		switch (TagGroup)
		{
		case GroupBy.__IGNORE__:
			return null;
		case GroupBy.NamedGroupNoSplit:
			return TargetBuild;
		case GroupBy.NamedGroup:
		{
			string fileName = Path.GetFileName(kaf.homedirectory);
			foreach (KeyValuePair<string, List<string>> defaultBuild in DefaultBuilds)
			{
				if (defaultBuild.Value.Contains(fileName))
				{
					return defaultBuild.Key;
				}
			}
			return TargetBuild;
		}
		case GroupBy.Folder:
			return Path.GetFileName(directory) + groupFiles.Count / 10;
		case GroupBy.DontGroup:
			return kaf.name;
		default:
			return null;
		}
	}
}

===== TextureAtlas =====
using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureAtlas : ScriptableObject
{
	[Serializable]
	public struct Item
	{
		public string name;

		public Vector4 uvBox;

		public Vector3[] vertices;

		public Vector2[] uvs;

		public int[] indices;
	}

	public class AtlasData
	{
		public class Frame
		{
			public string filename;

			public Dictionary<string, int> frame;

			public List<int[]> vertices;

			public List<int[]> verticesUV;

			public List<int[]> triangles;
		}

		public struct Meta
		{
			public Dictionary<string, int> size;
		}

		public List<Frame> frames;

		public Meta meta;
	}

	public List<string> sourceReference = new List<string>();

	public Texture2D texture;

	public float scaleFactor = 1f;

	public int mipPaddingPixels = 64;

	public int tileDimension = 256;

	public bool onlyRebuildSelfNamedFolder = true;

	public Item[] items;
}

