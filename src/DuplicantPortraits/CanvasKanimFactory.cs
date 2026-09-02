using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DuplicantPortraits
{
	internal static class CanvasKanimFactory
	{
		private const string TemplateAnimName = "painting_art_b";
		private const string TemplateClipName = "art_b";
		private const float PortraitFill = 0.75f;
		private static readonly Color32 Paper = new Color32(236, 217, 184, 255);
		private static readonly KAnimHashedString UiSymbol = new KAnimHashedString("ui");
		private static readonly KAnimHashedString CanvasSymbol = new KAnimHashedString("canvas");

		private static byte[] templateAnimBytes;
		private static byte[] templateBuildBytes;
		private static Texture2D templateTexture;
		private static KAnimFile templateFile;
		private static bool layoutReady;
		private static RectInt canvasRect;
		private static RectInt uiHole;
		private static bool hasUiHole;

		public static string ClipName
		{
			get
			{
				if (templateFile != null && !string.IsNullOrEmpty(templateFile.name)
					&& templateFile.name.ToLowerInvariant().Contains("painting_art_a")
					&& !templateFile.name.ToLowerInvariant().Contains("painting_art_b"))
					return "art_a";
				return TemplateClipName;
			}
		}

		public static bool HasTemplate
		{
			get
			{
				return templateAnimBytes != null && templateBuildBytes != null && templateTexture != null;
			}
		}

		public static void CaptureIfTemplate(KAnimFile file)
		{
			if (file == null || !LooksLikePaintingArt(file.name))
				return;

			bool preferred = IsPreferredTemplate(file.name);
			if (HasTemplate && !preferred)
				return;
			if (HasTemplate && preferred && templateFile != null && IsPreferredTemplate(templateFile.name))
				return;

			byte[] anim = CopyBytes(file.animBytes);
			byte[] build = CopyBytes(file.buildBytes);
			Texture2D tex = file.textureList != null && file.textureList.Count > 0 ? file.textureList[0] : null;
			if (anim == null || build == null || tex == null)
			{
				Debug.Log(Mod.LogPrefix + "saw painting kanim " + file.name
					+ " anim=" + (anim != null) + " build=" + (build != null) + " tex=" + (tex != null));
				return;
			}

			templateFile = file;
			templateAnimBytes = anim;
			templateBuildBytes = build;
			templateTexture = tex;
			layoutReady = false;
			Debug.Log(Mod.LogPrefix + "captured template " + file.name
				+ " anim=" + anim.Length + " build=" + build.Length
				+ " tex=" + tex.width + "x" + tex.height);
		}

		public static bool EnsureTemplate()
		{
			if (HasTemplate)
				return CacheLayout();

			KAnimFile file = FindTemplate();
			if (file == null)
			{
				Debug.LogWarning(Mod.LogPrefix + "missing template kanim " + TemplateAnimName);
				return false;
			}

			CaptureIfTemplate(file);
			if (!HasTemplate)
			{
				Debug.LogWarning(Mod.LogPrefix + "template kanim was incomplete: " + file.name);
				return false;
			}

			return CacheLayout();
		}

		public static KAnimFile Create(string kanimName, Texture2D portrait)
		{
			if (!EnsureTemplate() || portrait == null)
				return null;

			Texture2D atlas = PaintAtlas(portrait);
			if (atlas == null)
				return null;
			atlas.name = kanimName;

			KAnimFile file = ScriptableObject.CreateInstance<KAnimFile>();
			file.name = kanimName;
			file.homedirectory = "assets/" + kanimName;
			file.mod = new KAnimFile.Mod
			{
				anim = templateAnimBytes,
				build = templateBuildBytes,
				textures = new List<Texture2D> { atlas }
			};
			file.FinalizeLoading();

			if (!Register(file))
				return null;
			return file;
		}

		private static bool LooksLikePaintingArt(string name)
		{
			return !string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains("painting_art");
		}

		private static bool IsPreferredTemplate(string name)
		{
			return !string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains("painting_art_b");
		}

		private static byte[] CopyBytes(byte[] source)
		{
			if (source == null || source.Length == 0)
				return null;
			return (byte[])source.Clone();
		}

		private static KAnimFile FindTemplate()
		{
			KAnimFile file = Assets.GetAnim(TemplateAnimName);
			if (file != null)
				return file;
			file = Assets.GetAnim(TemplateAnimName + "_kanim");
			if (file != null)
				return file;
			if (Assets.Anims == null)
				return null;
			for (int i = 0; i < Assets.Anims.Count; i++)
			{
				KAnimFile candidate = Assets.Anims[i];
				if (candidate != null && LooksLikePaintingArt(candidate.name))
					return candidate;
			}
			return null;
		}

		private static bool CacheLayout()
		{
			if (layoutReady)
				return canvasRect.width > 4 && canvasRect.height > 4;

			layoutReady = true;
			Texture2D readable = TextureUtil.GetReadable(templateTexture);
			if (readable == null)
				return false;

			int tw = readable.width;
			int th = readable.height;
			KAnimFileData data = templateFile != null ? templateFile.GetData() : null;
			RectInt ui = default;
			bool foundCanvas = false;
			bool foundUi = false;

			if (data != null && data.build != null && data.build.symbols != null)
			{
				foreach (KAnim.Build.Symbol symbol in data.build.symbols)
				{
					if (symbol == null || symbol.numFrames <= 0)
						continue;
					RectInt rect = UvToPixels(symbol.GetFrame(0).uvMin, symbol.GetFrame(0).uvMax, tw, th);
					if (rect.width <= 2 || rect.height <= 2)
						continue;

					string name = SymbolName(symbol.hash);
					if (symbol.hash.HashValue == UiSymbol.HashValue || name == "ui")
					{
						ui = rect;
						foundUi = true;
					}
					else if (symbol.hash.HashValue == CanvasSymbol.HashValue || name == "canvas")
					{
						canvasRect = rect;
						foundCanvas = true;
					}
				}
			}

			if (!foundCanvas)
				canvasRect = new RectInt(0, 0, tw, th);
			if (foundUi)
			{
				uiHole = InnerHole(ui);
				hasUiHole = uiHole.width > 4 && uiHole.height > 4;
			}

			Debug.Log(Mod.LogPrefix + "template atlas " + tw + "x" + th
				+ " canvas=" + canvasRect.width + "x" + canvasRect.height
				+ (hasUiHole ? " ui=" + uiHole.width + "x" + uiHole.height : " ui=none"));
			return canvasRect.width > 4 && canvasRect.height > 4;
		}

		private static Texture2D PaintAtlas(Texture2D portrait)
		{
			Texture2D readable = TextureUtil.GetReadable(templateTexture);
			if (readable == null)
				return null;

			Color32[] pixels = readable.GetPixels32();
			int w = readable.width;
			int h = readable.height;
			TextureUtil.PlaceInFrame(pixels, w, h, canvasRect, portrait, Paper, PortraitFill);
			if (hasUiHole)
				TextureUtil.PutPaintingInFrame(pixels, w, h, canvasRect, uiHole, Paper);

			Texture2D atlas = new Texture2D(w, h, TextureFormat.RGBA32, false)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Bilinear
			};
			atlas.SetPixels32(pixels);
			atlas.Apply(false, false);
			return atlas;
		}

		private static bool Register(KAnimFile file)
		{
			try
			{
				var groupFile = new KAnimGroupFile.GroupFile
				{
					groupID = file.name,
					commandDirectory = file.homedirectory
				};
				KAnimGroupFile.GetGroupFile().AddAnimFile(groupFile, new AnimCommandFile(), file);

				KAnimGroupFile.Group group = KAnimGroupFile.GetGroup(new HashedString(file.name));
				if (group == null)
				{
					Debug.LogWarning(Mod.LogPrefix + "missing anim group for " + file.name);
					return false;
				}

				if (templateFile != null)
				{
					KAnimGroupFile.Group templateGroup = KAnimGroupFile.GetGroup(templateFile.batchTag);
					if (templateGroup != null)
					{
						group.renderType = templateGroup.renderType;
						group.maxVisibleSymbols = templateGroup.maxVisibleSymbols;
						group.maxGroupSize = 1;
					}
				}

				KBatchGroupData batchData = KAnimBatchManager.Instance().GetBatchGroupData(group.id);
				var hashed = new KAnimHashedString(file.name);
				HashCache.Get().Add(hashed.HashValue, file.name);

				KAnimFileData fileData = KGlobalAnimParser.Get().GetFile(file);
				fileData.maxVisSymbolFrames = 0;
				fileData.batchTag = group.id;
				fileData.buildIndex = KGlobalAnimParser.ParseBuildData(batchData, hashed, new FastReader(file.buildBytes), file.textureList);
				KGlobalAnimParser.ParseAnimData(batchData, hashed, new FastReader(file.animBytes), fileData);
				KGlobalAnimParser.PostParse(batchData);

				Assets.Anims.Add(file);
				Assets.ModLoadedKAnims.Add(file);
				var table = AccessTools.Field(typeof(Assets), "AnimTable").GetValue(null) as Dictionary<HashedString, KAnimFile>;
				if (table != null)
					table[new HashedString(file.name)] = file;

				return true;
			}
			catch (System.Exception ex)
			{
				Debug.LogError(Mod.LogPrefix + "failed to register kanim " + file.name + ": " + ex);
				return false;
			}
		}

		private static string SymbolName(KAnimHashedString hash)
		{
			string cached = HashCache.Get().Get(hash);
			return string.IsNullOrEmpty(cached) ? hash.ToString() : cached;
		}

		// Thumbnail `ui` is a gold frame + drop shadow. InnerHole is the empty window we put the world painting into.
		private static RectInt InnerHole(RectInt ui)
		{
			int x = ui.x + Mathf.RoundToInt(ui.width * 26f / 123f) - 2;
			int y = ui.y + Mathf.RoundToInt(ui.height * 23f / 124f) + 2;
			return new RectInt(x, y, 78, 85);
		}

		private static RectInt UvToPixels(Vector2 uvMin, Vector2 uvMax, int tw, int th)
		{
			int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(uvMin.x, uvMax.x) * tw), 0, tw - 1);
			int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(uvMin.x, uvMax.x) * tw), 0, tw);
			int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(uvMin.y, uvMax.y) * th), 0, th - 1);
			int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(uvMin.y, uvMax.y) * th), 0, th);
			return new RectInt(x0, y0, Mathf.Max(0, x1 - x0), Mathf.Max(0, y1 - y0));
		}
	}
}
