using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class KAnimFactory
	{
		private static readonly KAnimHashedString[] CapSymbols =
		{
			new KAnimHashedString("cap_left"),
			new KAnimHashedString("cap_left_fg"),
			new KAnimHashedString("cap_left_place"),
			new KAnimHashedString("cap_right"),
			new KAnimHashedString("cap_right_fg"),
			new KAnimHashedString("cap_right_place"),
			new KAnimHashedString("cap_top"),
			new KAnimHashedString("cap_top_fg"),
			new KAnimHashedString("cap_top_place"),
			new KAnimHashedString("cap_bottom"),
			new KAnimHashedString("cap_bottom_fg"),
			new KAnimHashedString("cap_bottom_place")
		};

		private const int AtlasScale = 2;
		private static byte[] wallsAlpha;
		private static Color32[] atlasBuffer;
		private static int wallsWidth;
		private static int wallsHeight;
		private static bool symbolRectsReady;
		private static readonly KAnimHashedString UiSymbol = new KAnimHashedString("ui");
		private static readonly List<RectInt> CapRects = new List<RectInt>();
		private static readonly List<RectInt> BodyRects = new List<RectInt>();
		private static readonly List<RectInt> BodyOpaqueRects = new List<RectInt>();
		private static readonly List<RectInt> UiRects = new List<RectInt>();

		public static KAnimFile Create(string kanimName, Texture2D interior, Texture2D uiOverlay = null)
		{
			if (Mod.WallsAnimBytes == null || Mod.WallsBuildBytes == null || Mod.WallsTexture == null || interior == null)
				return null;

			Texture2D atlas = PaintDrywallAtlas(interior, uiOverlay);
			if (atlas == null)
				return null;
			atlas.name = kanimName;

			KAnimFile file = ScriptableObject.CreateInstance<KAnimFile>();
			file.name = kanimName;
			file.homedirectory = "assets/" + kanimName;
			file.mod = new KAnimFile.Mod
			{
				anim = Mod.WallsAnimBytes,
				build = Mod.WallsBuildBytes,
				textures = new List<Texture2D> { atlas }
			};
			file.FinalizeLoading();

			if (!Register(file))
				return null;
			return file;
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
					Debug.LogWarning("[DrywallTileSkins] missing anim group for " + file.name);
					return false;
				}

				if (Mod.WallsKanim != null)
				{
					KAnimGroupFile.Group templateGroup = KAnimGroupFile.GetGroup(Mod.WallsKanim.batchTag);
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
				Debug.LogError("[DrywallTileSkins] failed to register kanim " + file.name + ": " + ex);
				return false;
			}
		}

		private static Texture2D PaintDrywallAtlas(Texture2D interior, Texture2D uiOverlay)
		{
			if (wallsAlpha == null)
			{
				Texture2D walls = TextureUtil.GetReadable(Mod.WallsTexture);
				if (walls == null)
					return null;
				CaptureMask(walls);
				CacheSymbolRects();
			}

			if (atlasBuffer == null || atlasBuffer.Length != wallsAlpha.Length)
				atlasBuffer = new Color32[wallsAlpha.Length];
			else
				System.Array.Clear(atlasBuffer, 0, atlasBuffer.Length);

			Color32[] dst = atlasBuffer;
			Color32[] fill = interior.GetPixels32();
			int iw = Mathf.Max(1, interior.width);
			int ih = Mathf.Max(1, interior.height);

			if (BodyRects.Count == 0)
			{
				Debug.LogWarning("[DrywallTileSkins] walls_kanim body UVs were empty; tile skins may look shifted");
				Stamp(dst, new RectInt(0, 0, wallsWidth, wallsHeight), new RectInt(0, 0, wallsWidth, wallsHeight), fill, iw, ih, true);
			}
			else
			{
				for (int b = 0; b < BodyOpaqueRects.Count; b++)
					Stamp(dst, BodyRects[b], BodyOpaqueRects[b], fill, iw, ih, true);
			}

			if (uiOverlay != null)
			{
				Color32[] uiPixels = uiOverlay.GetPixels32();
				int uw = Mathf.Max(1, uiOverlay.width);
				int uh = Mathf.Max(1, uiOverlay.height);
				for (int u = 0; u < UiRects.Count; u++)
					Stamp(dst, UiRects[u], UiRects[u], uiPixels, uw, uh, false);
			}
			else
			{
				for (int u = 0; u < UiRects.Count; u++)
					Stamp(dst, UiRects[u], UiRects[u], fill, iw, ih, true);
			}

			for (int c = 0; c < CapRects.Count; c++)
			{
				RectInt rect = CapRects[c];
				for (int y = rect.y; y < rect.yMax && y < wallsHeight; y++)
				{
					if (y < 0)
						continue;
					int row = y * wallsWidth;
					for (int x = rect.x; x < rect.xMax && x < wallsWidth; x++)
					{
						if (x >= 0)
							dst[row + x] = default;
					}
				}
			}

			Texture2D atlas = new Texture2D(wallsWidth, wallsHeight, TextureFormat.RGBA32, true)
			{
				wrapMode = TextureWrapMode.Clamp
			};
			CopyVanillaSampler(atlas);
			atlas.SetPixels32(dst);
			atlas.Apply(true, false);
			return atlas;
		}

		private static void CopyVanillaSampler(Texture2D atlas)
		{
			Texture2D src = Mod.WallsTexture;
			if (src != null)
			{
				atlas.wrapMode = src.wrapMode;
				atlas.anisoLevel = src.anisoLevel;
				atlas.mipMapBias = src.mipMapBias;
			}

			// Vanilla kanims generate mipmaps and minify with linear filtering.
			// Point / bilinear-without-mips crawls and aliases when the camera zooms out.
			atlas.filterMode = FilterMode.Trilinear;
		}

		private static void CaptureMask(Texture2D walls)
		{
			int srcW = Mathf.Max(1, walls.width);
			int srcH = Mathf.Max(1, walls.height);
			wallsWidth = srcW * AtlasScale;
			wallsHeight = srcH * AtlasScale;
			Color32[] src = walls.GetPixels32();
			wallsAlpha = new byte[wallsWidth * wallsHeight];
			for (int y = 0; y < wallsHeight; y++)
			{
				float v = (y + 0.5f) / wallsHeight;
				int dstRow = y * wallsWidth;
				for (int x = 0; x < wallsWidth; x++)
					wallsAlpha[dstRow + x] = SampleBilinear(src, srcW, srcH, (x + 0.5f) / wallsWidth, v).a;
			}

			Texture2D srcTex = Mod.WallsTexture;
			if (srcTex != null)
			{
				Debug.Log("[DrywallTileSkins] vanilla walls tex " + srcTex.width + "x" + srcTex.height
					+ " filter=" + srcTex.filterMode + " mips=" + srcTex.mipmapCount
					+ " wrap=" + srcTex.wrapMode + " aniso=" + srcTex.anisoLevel);
			}
		}

		private static void Stamp(Color32[] dst, RectInt clip, RectInt opaque, Color32[] fill, int iw, int ih, bool applyWallAlpha)
		{
			int rw = Mathf.Max(1, opaque.width);
			int rh = Mathf.Max(1, opaque.height);
			int y0 = Mathf.Max(clip.y, 0);
			int y1 = Mathf.Min(clip.yMax, wallsHeight);
			int x0 = Mathf.Max(clip.x, 0);
			int x1 = Mathf.Min(clip.xMax, wallsWidth);
			float invW = (float)iw / rw;
			float invH = (float)ih / rh;
			for (int y = y0; y < y1; y++)
			{
				float fy0 = (y - opaque.y) * invH;
				float fy1 = (y - opaque.y + 1) * invH;
				int row = y * wallsWidth;
				for (int x = x0; x < x1; x++)
				{
					int i = row + x;
					Color32 c = SampleRect(fill, iw, ih, (x - opaque.x) * invW, fy0, (x - opaque.x + 1) * invW, fy1);
					if (applyWallAlpha)
					{
						byte a = wallsAlpha[i];
						if (a < 3)
							continue;
						c.a = (byte)(c.a * a / 255);
					}
					dst[i] = c;
				}
			}
		}

		private static Color32 SampleRect(Color32[] px, int w, int h, float x0, float y0, float x1, float y1)
		{
			if (w <= 1 || h <= 1)
				return px[0];

			if (x1 < x0)
			{
				float t = x0;
				x0 = x1;
				x1 = t;
			}
			if (y1 < y0)
			{
				float t = y0;
				y0 = y1;
				y1 = t;
			}

			x0 = Mathf.Clamp(x0, 0f, w);
			x1 = Mathf.Clamp(x1, 0f, w);
			y0 = Mathf.Clamp(y0, 0f, h);
			y1 = Mathf.Clamp(y1, 0f, h);
			float bw = x1 - x0;
			float bh = y1 - y0;
			if (bw < 1e-4f || bh < 1e-4f)
				return default;

			if (bw <= 1.01f && bh <= 1.01f)
				return SampleBilinear(px, w, h, (x0 + x1) * 0.5f / w, (y0 + y1) * 0.5f / h);

			int ix0 = Mathf.Clamp(Mathf.FloorToInt(x0), 0, w - 1);
			int iy0 = Mathf.Clamp(Mathf.FloorToInt(y0), 0, h - 1);
			int ix1 = Mathf.Clamp(Mathf.CeilToInt(x1) - 1, 0, w - 1);
			int iy1 = Mathf.Clamp(Mathf.CeilToInt(y1) - 1, 0, h - 1);

			float r = 0f;
			float g = 0f;
			float b = 0f;
			float a = 0f;
			float wt = 0f;
			for (int y = iy0; y <= iy1; y++)
			{
				float wy = Mathf.Min(y1, y + 1f) - Mathf.Max(y0, y);
				if (wy <= 0f)
					continue;
				int row = y * w;
				for (int x = ix0; x <= ix1; x++)
				{
					float wxy = wy * (Mathf.Min(x1, x + 1f) - Mathf.Max(x0, x));
					if (wxy <= 0f)
						continue;
					Color32 c = px[row + x];
					r += c.r * wxy;
					g += c.g * wxy;
					b += c.b * wxy;
					a += c.a * wxy;
					wt += wxy;
				}
			}

			if (wt < 1e-4f)
				return default;
			float inv = 1f / wt;
			return new Color32(
				(byte)Mathf.Clamp(Mathf.RoundToInt(r * inv), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(g * inv), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(b * inv), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(a * inv), 0, 255));
		}

		private static Color32 SampleBilinear(Color32[] px, int w, int h, float u, float v)
		{
			if (w <= 1 || h <= 1)
				return px[0];
			float x = Mathf.Clamp01(u) * (w - 1);
			float y = Mathf.Clamp01(v) * (h - 1);
			int x0 = Mathf.FloorToInt(x);
			int y0 = Mathf.FloorToInt(y);
			int x1 = Mathf.Min(x0 + 1, w - 1);
			int y1 = Mathf.Min(y0 + 1, h - 1);
			float tx = x - x0;
			float ty = y - y0;
			Color32 c00 = px[y0 * w + x0];
			Color32 c10 = px[y0 * w + x1];
			Color32 c01 = px[y1 * w + x0];
			Color32 c11 = px[y1 * w + x1];
			return Color32.Lerp(Color32.Lerp(c00, c10, tx), Color32.Lerp(c01, c11, tx), ty);
		}

		private static void CacheSymbolRects()
		{
			if (symbolRectsReady || Mod.WallsKanim == null)
				return;
			symbolRectsReady = true;

			KAnimFileData data = Mod.WallsKanim.GetData();
			if (data == null || data.build == null || data.build.symbols == null)
				return;

			foreach (KAnim.Build.Symbol symbol in data.build.symbols)
			{
				if (symbol == null)
					continue;

				bool cap = IsCap(symbol.hash);
				bool ui = IsUi(symbol.hash);
				for (int i = 0; i < symbol.numFrames; i++)
				{
					KAnim.Build.SymbolFrameInstance frame = symbol.GetFrame(i);
					RectInt rect = UvToPixels(frame.uvMin, frame.uvMax);
					if (rect.width <= 1 || rect.height <= 1)
						continue;
					if (cap)
						CapRects.Add(rect);
					else if (ui)
						UiRects.Add(rect);
					else
					{
						BodyRects.Add(rect);
						BodyOpaqueRects.Add(OpaqueBounds(rect));
					}
				}
			}

			RectInt sample = BodyOpaqueRects.Count > 0 ? BodyOpaqueRects[0] : default;
			Debug.Log("[DrywallTileSkins] walls atlas " + wallsWidth + "x" + wallsHeight
				+ " (x" + AtlasScale + "), body sprites " + BodyRects.Count + ", ui " + UiRects.Count + ", caps " + CapRects.Count
				+ ", opaque0=" + sample.width + "x" + sample.height);
		}

		private static RectInt OpaqueBounds(RectInt rect)
		{
			int minX = rect.xMax;
			int minY = rect.yMax;
			int maxX = rect.x;
			int maxY = rect.y;
			for (int y = rect.y; y < rect.yMax; y++)
			{
				if (y < 0 || y >= wallsHeight)
					continue;
				int row = y * wallsWidth;
				for (int x = rect.x; x < rect.xMax; x++)
				{
					if (x < 0 || x >= wallsWidth)
						continue;
					if (wallsAlpha[row + x] < 3)
						continue;
					if (x < minX) minX = x;
					if (y < minY) minY = y;
					if (x > maxX) maxX = x;
					if (y > maxY) maxY = y;
				}
			}

			if (maxX < minX || maxY < minY)
				return rect;
			return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
		}

		private static RectInt UvToPixels(Vector2 uvMin, Vector2 uvMax)
		{
			int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(uvMin.x, uvMax.x) * wallsWidth), 0, wallsWidth - 1);
			int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(uvMin.x, uvMax.x) * wallsWidth), 0, wallsWidth);
			int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(uvMin.y, uvMax.y) * wallsHeight), 0, wallsHeight - 1);
			int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(uvMin.y, uvMax.y) * wallsHeight), 0, wallsHeight);
			return new RectInt(x0, y0, Mathf.Max(0, x1 - x0), Mathf.Max(0, y1 - y0));
		}

		private static bool IsUi(KAnimHashedString hash)
		{
			if (hash.HashValue == UiSymbol.HashValue)
				return true;
			string name = hash.ToString();
			return name == "ui";
		}

		private static bool IsCap(KAnimHashedString hash)
		{
			for (int i = 0; i < CapSymbols.Length; i++)
			{
				if (CapSymbols[i].HashValue == hash.HashValue)
					return true;
			}

			string name = hash.ToString();
			return !string.IsNullOrEmpty(name) && name.IndexOf("cap_", System.StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
