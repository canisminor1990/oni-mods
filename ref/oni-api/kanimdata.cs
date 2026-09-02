===== MISSING KAnim+Build+SymbolFrame =====

===== KAnim.Build.Symbol =====
using System;
using System.Diagnostics;
using UnityEngine;

public class KAnim
{
	public enum PlayMode
	{
		Loop,
		Once,
		Paused
	}

	public enum SymbolFlags
	{
		Bloom = 1,
		OnLight = 2,
		SnapTo = 4,
		FG = 8,
		SH = 0x10
	}

	[DebuggerDisplay("{id} {animFile}")]
	public class Anim
	{
		public struct Frame
		{
			public int firstElementIdx;

			public int numElements;

			public bool hasHead;
		}

		public struct FrameElement
		{
			public Matrix2x3 transform;

			public KAnimHashedString symbol;

			public int frame;

			public float multAlpha;
		}

		public string name;

		public HashedString id;

		public float frameRate;

		public int firstFrameIdx;

		public int numFrames;

		public HashedString rootSymbol;

		public HashedString hash;

		public float totalTime;

		public float scaledBoundingRadius;

		public Vector2 unScaledSize = Vector2.zero;

		public int index { get; private set; }

		public KAnimFileData animFile { get; private set; }

		public Anim(KAnimFileData anim_file, int idx)
		{
			animFile = anim_file;
			index = idx;
		}

		public int GetFrameIdx(PlayMode mode, float elapsedSeconds)
		{
			if (numFrames <= 0)
			{
				return -1;
			}
			int result = 0;
			switch (mode)
			{
			case PlayMode.Loop:
				elapsedSeconds %= totalTime;
				break;
			}
			if (elapsedSeconds > 0f)
			{
				float num = 0.01f / frameRate;
				float num2 = elapsedSeconds * frameRate + num;
				result = Math.Min(numFrames - 1, (int)num2);
			}
			return result;
		}

		public bool TryGetFrame(HashedString batchTag, int idx, out Frame frame)
		{
			return KAnimBatchManager.Instance().GetBatchGroupData(batchTag).TryGetFrame(idx + firstFrameIdx, out frame);
		}
	}

	public class Build
	{
		public struct SymbolFrameInstance
		{
			public int sourceFrameNum;

			public int duration;

			public Vector2 uvMin;

			public Vector2 uvMax;

			public Vector2 bboxMin;

			public Vector2 bboxMax;

			public int buildImageIdx;

			public int symbolIdx;
		}

		[DebuggerDisplay("{hash} {path} {folder} {colourChannel}")]
		public class Symbol
		{
			[NonSerialized]
			public Build build;

			public KAnimHashedString hash;

			public KAnimHashedString path;

			public KAnimHashedString folder;

			public KAnimHashedString colourChannel;

			public int flags;

			public int firstFrameIdx;

			public int numFrames;

			public int numLookupFrames;

			public int[] frameLookup;

			public int symbolIndexInSourceBuild;

			public int GetFrameIdx(int frame)
			{
				if (frameLookup == null)
				{
					Debug.LogErrorFormat("Cant get frame [{2}] because Symbol [{0}] for build [{1}] batch [{3}] has no frameLookup", hash.ToString(), build.name, frame, build.batchTag.ToString());
				}
				if (frameLookup.Length == 0 || frame >= frameLookup.Length)
				{
					return -1;
				}
				frame = Math.Min(frame, frameLookup.Length - 1);
				return frameLookup[frame];
			}

			public SymbolFrameInstance GetFrame(int frame)
			{
				return GetFrame(frame, KAnimBatchManager.Instance().GetBatchGroupData(build.batchTag));
			}

			public SymbolFrameInstance GetFrame(int frame, KBatchGroupData batch_group_data)
			{
				int frameIdx = GetFrameIdx(frame);
				return batch_group_data.GetSymbolFrameInstance(frameIdx);
			}
		}

		public KAnimHashedString fileHash;

		public int index;

		public string name;

		public HashedString batchTag;

		public int textureStartIdx;

		public int textureCount;

		public Symbol[] symbols;

		public Symbol GetSymbolByIndex(uint index)
		{
			if (index >= symbols.Length)
			{
				return null;
			}
			return symbols[index];
		}

		public Texture2D GetTexture(int index)
		{
			if (index < 0 || index >= textureCount)
			{
				Debug.LogError("Invalid texture index:" + index);
			}
			return GetTexture(index, KAnimBatchManager.Instance().GetBatchGroupData(batchTag));
		}

		public Texture2D GetTexture(int index, KBatchGroupData batch_group_data)
		{
			if (index < 0 || index >= textureCount)
			{
				Debug.LogError("Invalid texture index:" + index);
			}
			return batch_group_data.GetTexure(textureStartIdx + index);
		}

		public Symbol GetSymbol(KAnimHashedString symbol_name)
		{
			for (int i = 0; i < symbols.Length; i++)
			{
				if (symbols[i].hash == symbol_name)
				{
					return symbols[i];
				}
			}
			return null;
		}

		public override string ToString()
		{
			return name;
		}
	}
}

===== KAnimFileData =====
using System.Diagnostics;

[DebuggerDisplay("{name}")]
public class KAnimFileData
{
	public const int NO_RECORD = -1;

	public int index;

	public HashedString batchTag;

	public int buildIndex;

	public HashedString animBatchTag;

	public int firstAnimIndex;

	public int animCount;

	public int frameCount;

	public int firstElementIndex;

	public int elementCount;

	public int maxVisSymbolFrames;

	public string name { get; private set; }

	public KAnimHashedString hashName { get; private set; }

	public KAnim.Build build
	{
		get
		{
			if (buildIndex == -1)
			{
				return null;
			}
			KBatchGroupData batchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(batchTag);
			if (batchGroupData == null)
			{
				Debug.LogErrorFormat("[{0}] No such batch group [{1}]", name, batchTag.ToString());
			}
			return batchGroupData.GetBuild(buildIndex);
		}
	}

	public KAnimFileData(string name)
	{
		this.name = name;
		firstAnimIndex = -1;
		buildIndex = -1;
		firstElementIndex = -1;
		animCount = 0;
		frameCount = 0;
		elementCount = 0;
		maxVisSymbolFrames = 0;
		hashName = new KAnimHashedString(name);
	}

	public KAnim.Anim GetAnim(int index)
	{
		Debug.Assert(index >= 0 && index < animCount);
		KBatchGroupData batchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(animBatchTag);
		if (batchGroupData == null)
		{
			Debug.LogError($"[{name}] No such batch group [{animBatchTag.ToString()}]");
		}
		return batchGroupData.GetAnim(index + firstAnimIndex);
	}

	public KAnim.Anim GetAnim(string anim_name)
	{
		KBatchGroupData batchGroupData = KAnimBatchManager.Instance().GetBatchGroupData(animBatchTag);
		if (batchGroupData == null)
		{
			Debug.LogError($"[{name}] No such batch group [{animBatchTag.ToString()}]");
			return null;
		}
		foreach (KAnim.Anim anim in batchGroupData.anims)
		{
			if (anim.name == anim_name)
			{
				return anim;
			}
		}
		return null;
	}
}

