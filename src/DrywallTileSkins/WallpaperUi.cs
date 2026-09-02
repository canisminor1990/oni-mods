using Database;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class WallpaperUi
	{
		private static readonly string[] TemplateAnims =
		{
			"walls_pastel_pink_kanim",
			"walls_basic_white_kanim",
			"walls_pastel_yellow_kanim"
		};

		private static readonly string[] DiffAnims =
		{
			"walls_pastel_blue_kanim",
			"walls_pastel_green_kanim",
			"walls_pastel_yellow_kanim",
			"walls_basic_white_kanim"
		};

		private static bool attempted;
		private static bool ready;
		private static Color[] template;
		private static bool[] fillMask;
		private static int width;
		private static int height;
		private static RectInt fillBounds;

		public static bool Ready
		{
			get
			{
				EnsureTemplate();
				return ready;
			}
		}

		public static Sprite CreateSprite(Texture2D interior)
		{
			Texture2D tex = ComposeTexture(interior);
			if (tex == null)
				return null;
			return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
		}

		public static Texture2D ComposeTexture(Texture2D interior)
		{
			EnsureTemplate();
			if (!ready || interior == null)
				return null;

			Color32[] fill = interior.GetPixels32();
			Color[] pixels = Compose(fill, interior.width, interior.height);
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
			{
				name = interior.name + "_wallpaper_ui",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			tex.SetPixels(pixels);
			tex.Apply(false, false);
			return tex;
		}

		private static Color[] Compose(Color32[] fillPixels, int fw, int fh)
		{
			Color[] pixels = new Color[template.Length];
			for (int y = 0; y < height; y++)
			{
				int row = y * width;
				for (int x = 0; x < width; x++)
					pixels[row + x] = SampleComposed(x, y, fillPixels, fw, fh);
			}
			return pixels;
		}

		private static Color SampleComposed(int tx, int ty, Color32[] fillPixels, int fw, int fh)
		{
			int i = ty * width + tx;
			Color paper = template[i];
			if (paper.a < 0.01f || !fillMask[i])
				return paper;

			float u = (tx - fillBounds.x + 0.5f) / Mathf.Max(1, fillBounds.width);
			float v = (ty - fillBounds.y + 0.5f) / Mathf.Max(1, fillBounds.height);
			Color fill = SampleBilinear(fillPixels, fw, fh, u, v);
			fill.a *= paper.a;
			return fill;
		}

		private static Color SampleBilinear(Color32[] px, int w, int h, float u, float v)
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
			Color c00 = px[y0 * w + x0];
			Color c10 = px[y0 * w + x1];
			Color c01 = px[y1 * w + x0];
			Color c11 = px[y1 * w + x1];
			return Color.Lerp(Color.Lerp(c00, c10, tx), Color.Lerp(c01, c11, tx), ty);
		}

		private static void EnsureTemplate()
		{
			if (ready || attempted)
				return;

			if (!TryExtractUi(TemplateAnims, out template, out width, out height))
			{
				if (AssetsAreReady())
				{
					attempted = true;
					Debug.LogWarning("[DrywallTileSkins] could not read a vanilla wallpaper ui symbol");
				}
				return;
			}

			attempted = true;

			if (!TryBuildDiffMask() && !TryBuildLumaMask())
			{
				Debug.LogWarning("[DrywallTileSkins] wallpaper overlay mask failed");
				return;
			}

			ComputeFillBounds();
			ready = fillBounds.width > 4 && fillBounds.height > 4;
			if (ready)
			{
				int fillCount = 0;
				for (int i = 0; i < fillMask.Length; i++)
				{
					if (fillMask[i])
						fillCount++;
				}
				Debug.Log("[DrywallTileSkins] wallpaper ui overlay " + width + "x" + height
					+ ", fill " + fillBounds.width + "x" + fillBounds.height
					+ " (" + (100 * fillCount / fillMask.Length) + "%)");
			}
		}

		private static bool TryBuildDiffMask()
		{
			if (!TryExtractUi(DiffAnims, out Color[] other, out int ow, out int oh))
				return false;
			if (ow != width || oh != height || other.Length != template.Length)
				return false;

			fillMask = new bool[template.Length];
			int fillCount = 0;
			for (int i = 0; i < template.Length; i++)
			{
				Color a = template[i];
				Color b = other[i];
				if (a.a < 0.12f && b.a < 0.12f)
					continue;
				float dr = a.r - b.r;
				float dg = a.g - b.g;
				float db = a.b - b.b;
				bool fill = (dr * dr + dg * dg + db * db) > 0.012f;
				fillMask[i] = fill;
				if (fill)
					fillCount++;
			}

			float ratio = (float)fillCount / template.Length;
			if (ratio < 0.18f || ratio > 0.92f)
			{
				fillMask = null;
				return false;
			}
			return true;
		}

		private static bool TryBuildLumaMask()
		{
			fillMask = new bool[template.Length];
			int fillCount = 0;
			for (int i = 0; i < template.Length; i++)
			{
				Color c = template[i];
				if (c.a < 0.12f)
					continue;
				float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
				float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
				float lum = (max + min) * 0.5f;
				float sat = max - min;
				bool overlay = (lum < 0.16f && sat < 0.22f) || (lum > 0.88f && sat < 0.18f);
				if (overlay)
					continue;
				fillMask[i] = true;
				fillCount++;
			}

			float ratio = (float)fillCount / template.Length;
			return ratio >= 0.18f && ratio <= 0.92f;
		}

		private static void ComputeFillBounds()
		{
			int minX = width;
			int minY = height;
			int maxX = -1;
			int maxY = -1;
			for (int y = 0; y < height; y++)
			{
				int row = y * width;
				for (int x = 0; x < width; x++)
				{
					if (!fillMask[row + x])
						continue;
					if (x < minX) minX = x;
					if (y < minY) minY = y;
					if (x > maxX) maxX = x;
					if (y > maxY) maxY = y;
				}
			}

			if (maxX < minX || maxY < minY)
				fillBounds = new RectInt(0, 0, width, height);
			else
				fillBounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
		}

		private static bool TryExtractUi(string[] names, out Color[] pixels, out int w, out int h)
		{
			pixels = null;
			w = h = 0;
			for (int i = 0; i < names.Length; i++)
			{
				if (TryExtractUi(Assets.GetAnim(names[i]), out pixels, out w, out h))
					return true;
			}

			try
			{
				BuildingFacades facades = Db.GetBuildingFacades();
				if (facades != null)
				{
					foreach (BuildingFacadeResource resource in facades.resources)
					{
						if (resource == null || resource.PrefabID != ExteriorWallConfig.ID)
							continue;
						if (Mod.IsOurFacade(resource.Id) || string.IsNullOrEmpty(resource.AnimFile))
							continue;
						if (TryExtractUi(Assets.GetAnim(resource.AnimFile), out pixels, out w, out h))
							return true;
					}
				}
			}
			catch (System.Exception)
			{
			}

			return false;
		}

		private static bool AssetsAreReady()
		{
			return Assets.Anims != null && Assets.Anims.Count > 50;
		}

		private static bool TryExtractUi(KAnimFile file, out Color[] pixels, out int w, out int h)
		{
			pixels = null;
			w = h = 0;
			if (file == null)
				return false;

			KAnimFileData data = file.GetData();
			if (data == null || data.build == null)
				return false;

			KAnim.Build.Symbol symbol = data.build.GetSymbol(new KAnimHashedString("ui"));
			if (symbol == null || symbol.numFrames <= 0)
				return false;

			KAnim.Build.SymbolFrameInstance frame = symbol.GetFrame(0);
			Texture2D source = null;
			if (file.textureList != null && file.textureList.Count > 0)
				source = file.textureList[0];
			if (source == null)
				source = data.build.GetTexture(0);
			Texture2D readable = TextureUtil.GetReadable(source);
			if (readable == null)
				return false;

			RectInt rect = UvToPixels(frame.uvMin, frame.uvMax, readable.width, readable.height);
			if (rect.width < 16 || rect.height < 16)
				return false;

			pixels = readable.GetPixels(rect.x, rect.y, rect.width, rect.height);
			w = rect.width;
			h = rect.height;
			return pixels != null && pixels.Length == w * h;
		}

		private static RectInt UvToPixels(Vector2 uvMin, Vector2 uvMax, int texW, int texH)
		{
			int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(uvMin.x, uvMax.x) * texW), 0, texW - 1);
			int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(uvMin.x, uvMax.x) * texW), 0, texW);
			int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(uvMin.y, uvMax.y) * texH), 0, texH - 1);
			int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(uvMin.y, uvMax.y) * texH), 0, texH);
			return new RectInt(x0, y0, Mathf.Max(0, x1 - x0), Mathf.Max(0, y1 - y0));
		}
	}
}
