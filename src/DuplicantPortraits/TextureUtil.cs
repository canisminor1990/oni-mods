using UnityEngine;

namespace DuplicantPortraits
{
	internal static class TextureUtil
	{
		public static Texture2D GetReadable(Texture source)
		{
			if (source == null)
				return null;

			if (source is Texture2D tex2d)
			{
				try
				{
					tex2d.GetPixel(0, 0);
					return tex2d;
				}
				catch (UnityException)
				{
				}
			}

			RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = rt;
			GL.Clear(true, true, Color.clear);
			Graphics.Blit(source, rt);
			Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
			{
				name = source.name + "_readable",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			copy.ReadPixels(new Rect(0f, 0f, source.width, source.height), 0, 0);
			copy.Apply(false, false);
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(rt);
			return copy;
		}

		public static Texture2D FromSprite(Sprite sprite)
		{
			if (sprite == null || sprite.texture == null)
				return null;

			Texture2D source = GetReadable(sprite.texture);
			if (source == null)
				return null;

			Rect rect = sprite.textureRect;
			int x = Mathf.Clamp(Mathf.RoundToInt(rect.x), 0, source.width - 1);
			int y = Mathf.Clamp(Mathf.RoundToInt(rect.y), 0, source.height - 1);
			int w = Mathf.Clamp(Mathf.RoundToInt(rect.width), 1, source.width - x);
			int h = Mathf.Clamp(Mathf.RoundToInt(rect.height), 1, source.height - y);

			Color[] pixels = source.GetPixels(x, y, w, h);
			if (sprite.packingRotation == SpritePackingRotation.FlipHorizontal)
				FlipHorizontal(pixels, w, h);
			else if (sprite.packingRotation == SpritePackingRotation.FlipVertical)
				FlipVertical(pixels, w, h);
			else if (sprite.packingRotation == SpritePackingRotation.Rotate180)
			{
				FlipHorizontal(pixels, w, h);
				FlipVertical(pixels, w, h);
			}

			Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false)
			{
				name = sprite.name,
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			result.SetPixels(pixels);
			result.Apply(false, false);
			return ClearMatteAndCrop(result);
		}

		public static Texture2D ClearMatteAndCrop(Texture2D source)
		{
			if (source == null)
				return null;

			Color32[] px = source.GetPixels32();
			int w = source.width;
			int h = source.height;
			int x0 = w;
			int y0 = h;
			int x1 = 0;
			int y1 = 0;
			for (int i = 0; i < px.Length; i++)
			{
				if (IsMatte(px[i]))
				{
					px[i] = new Color32(0, 0, 0, 0);
					continue;
				}
				int x = i % w;
				int y = i / w;
				if (x < x0) x0 = x;
				if (y < y0) y0 = y;
				if (x + 1 > x1) x1 = x + 1;
				if (y + 1 > y1) y1 = y + 1;
			}

			if (x1 <= x0 || y1 <= y0)
			{
				source.SetPixels32(px);
				source.Apply(false, false);
				return source;
			}

			int cw = x1 - x0;
			int ch = y1 - y0;
			var cropped = new Color32[cw * ch];
			for (int y = 0; y < ch; y++)
			{
				int src = (y0 + y) * w + x0;
				int dst = y * cw;
				for (int x = 0; x < cw; x++)
					cropped[dst + x] = px[src + x];
			}

			Texture2D result = new Texture2D(cw, ch, TextureFormat.RGBA32, false)
			{
				name = source.name + "_cutout",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			result.SetPixels32(cropped);
			result.Apply(false, false);
			return result;
		}

		public static void PlaceInFrame(Color32[] dst, int dstW, int dstH, RectInt hole, Texture2D portrait, Color32 paper, float fill)
		{
			if (portrait == null || hole.width <= 0 || hole.height <= 0)
				return;
			if (!Clip(hole, dstW, dstH, out int x0, out int y0, out int x1, out int y1))
				return;

			FillPaper(dst, dstW, x0, y0, x1, y1, paper);

			Color32[] src = portrait.GetPixels32();
			int sw = Mathf.Max(1, portrait.width);
			int sh = Mathf.Max(1, portrait.height);
			float f = Mathf.Clamp(fill, 0.2f, 1f);
			float scale = Mathf.Min((x1 - x0) * f / sw, (y1 - y0) * f / sh);
			float drawnW = sw * scale;
			float drawnH = sh * scale;
			BlitCutout(dst, dstW, x0, y0, x1, y1, src, sw, sh,
				x0 + ((x1 - x0) - drawnW) * 0.5f,
				y0 + ((y1 - y0) - drawnH) * 0.5f,
				drawnW, drawnH);
		}

		public static void PutPaintingInFrame(Color32[] atlas, int w, int h, RectInt painting, RectInt frameHole, Color32 paper)
		{
			if (!Clip(frameHole, w, h, out int dx0, out int dy0, out int dx1, out int dy1))
				return;

			FillPaper(atlas, w, dx0, dy0, dx1, dy1, paper);

			RectInt src = OpaqueBounds(atlas, w, h, painting);
			if (src.width < 4 || src.height < 4)
				src = painting;

			float scale = Mathf.Min((float)(dx1 - dx0) / src.width, (float)(dy1 - dy0) / src.height);
			float drawnW = src.width * scale;
			float drawnH = src.height * scale;
			float ox = dx0 + ((dx1 - dx0) - drawnW) * 0.5f;
			float oy = dy0 + ((dy1 - dy0) - drawnH) * 0.5f;

			int px0 = Mathf.Max(dx0, Mathf.FloorToInt(ox));
			int py0 = Mathf.Max(dy0, Mathf.FloorToInt(oy));
			int px1 = Mathf.Min(dx1, Mathf.CeilToInt(ox + drawnW));
			int py1 = Mathf.Min(dy1, Mathf.CeilToInt(oy + drawnH));
			for (int y = py0; y < py1; y++)
			{
				int row = y * w;
				for (int x = px0; x < px1; x++)
				{
					int i = row + x;
					if (atlas[i].a < 16)
						continue;
					float u = (x + 0.5f - ox) / drawnW;
					float v = (y + 0.5f - oy) / drawnH;
					if (u < 0f || v < 0f || u > 1f || v > 1f)
						continue;
					Color32 sample = SampleAt(atlas, w, h, src.x + u * (src.width - 1), src.y + v * (src.height - 1));
					if (sample.a < 16)
						continue;
					sample.a = atlas[i].a;
					atlas[i] = sample;
				}
			}
		}

		private static void FillPaper(Color32[] dst, int dstW, int x0, int y0, int x1, int y1, Color32 paper)
		{
			for (int y = y0; y < y1; y++)
			{
				int row = y * dstW;
				for (int x = x0; x < x1; x++)
				{
					int i = row + x;
					if (dst[i].a < 16)
						continue;
					paper.a = dst[i].a;
					dst[i] = paper;
				}
			}
		}

		private static void BlitCutout(Color32[] dst, int dstW, int x0, int y0, int x1, int y1, Color32[] src, int sw, int sh, float ox, float oy, float drawnW, float drawnH)
		{
			int px0 = Mathf.Max(x0, Mathf.FloorToInt(ox));
			int py0 = Mathf.Max(y0, Mathf.FloorToInt(oy));
			int px1 = Mathf.Min(x1, Mathf.CeilToInt(ox + drawnW));
			int py1 = Mathf.Min(y1, Mathf.CeilToInt(oy + drawnH));
			for (int y = py0; y < py1; y++)
			{
				int row = y * dstW;
				for (int x = px0; x < px1; x++)
				{
					int i = row + x;
					if (dst[i].a < 16)
						continue;
					float u = (x + 0.5f - ox) / drawnW;
					float v = (y + 0.5f - oy) / drawnH;
					if (u < 0f || v < 0f || u > 1f || v > 1f)
						continue;
					Color32 sample = SampleBilinear(src, sw, sh, u, v);
					if (sample.a < 8)
						continue;
					Color32 under = dst[i];
					Color32 painted = sample.a >= 250 ? sample : Blend(under, sample);
					painted.a = under.a;
					dst[i] = painted;
				}
			}
		}

		private static bool Clip(RectInt hole, int w, int h, out int x0, out int y0, out int x1, out int y1)
		{
			x0 = Mathf.Max(0, hole.x);
			y0 = Mathf.Max(0, hole.y);
			x1 = Mathf.Min(w, hole.xMax);
			y1 = Mathf.Min(h, hole.yMax);
			return x1 > x0 && y1 > y0;
		}

		private static RectInt OpaqueBounds(Color32[] px, int w, int h, RectInt hole)
		{
			int x0 = w;
			int y0 = h;
			int x1 = 0;
			int y1 = 0;
			int hx0 = Mathf.Max(0, hole.x);
			int hy0 = Mathf.Max(0, hole.y);
			int hx1 = Mathf.Min(w, hole.xMax);
			int hy1 = Mathf.Min(h, hole.yMax);
			for (int y = hy0; y < hy1; y++)
			{
				int row = y * w;
				for (int x = hx0; x < hx1; x++)
				{
					if (px[row + x].a < 16)
						continue;
					if (x < x0) x0 = x;
					if (y < y0) y0 = y;
					if (x + 1 > x1) x1 = x + 1;
					if (y + 1 > y1) y1 = y + 1;
				}
			}
			if (x1 <= x0 || y1 <= y0)
				return hole;
			return new RectInt(x0, y0, x1 - x0, y1 - y0);
		}

		private static bool IsMatte(Color32 c)
		{
			if (c.a < 16)
				return true;
			int min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
			int max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
			return min > 225 && (max - min) < 18;
		}

		private static Color32 SampleBilinear(Color32[] px, int w, int h, float u, float v)
		{
			if (w <= 1 || h <= 1)
				return px[0];
			return SampleAt(px, w, h, Mathf.Clamp01(u) * (w - 1), Mathf.Clamp01(v) * (h - 1));
		}

		private static Color32 SampleAt(Color32[] px, int w, int h, float x, float y)
		{
			x = Mathf.Clamp(x, 0f, w - 1);
			y = Mathf.Clamp(y, 0f, h - 1);
			int x0 = Mathf.FloorToInt(x);
			int y0 = Mathf.FloorToInt(y);
			int x1 = Mathf.Min(x0 + 1, w - 1);
			int y1 = Mathf.Min(y0 + 1, h - 1);
			float tx = x - x0;
			float ty = y - y0;
			return Color32.Lerp(
				Color32.Lerp(px[y0 * w + x0], px[y0 * w + x1], tx),
				Color32.Lerp(px[y1 * w + x0], px[y1 * w + x1], tx),
				ty);
		}

		private static Color32 Blend(Color32 under, Color32 over)
		{
			float a = over.a / 255f;
			float ia = 1f - a;
			return new Color32(
				(byte)Mathf.Clamp(Mathf.RoundToInt(over.r * a + under.r * ia), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(over.g * a + under.g * ia), 0, 255),
				(byte)Mathf.Clamp(Mathf.RoundToInt(over.b * a + under.b * ia), 0, 255),
				255);
		}

		private static void FlipHorizontal(Color[] pixels, int w, int h)
		{
			for (int y = 0; y < h; y++)
			{
				int row = y * w;
				for (int x = 0; x < w / 2; x++)
				{
					int a = row + x;
					int b = row + (w - 1 - x);
					Color tmp = pixels[a];
					pixels[a] = pixels[b];
					pixels[b] = tmp;
				}
			}
		}

		private static void FlipVertical(Color[] pixels, int w, int h)
		{
			for (int y = 0; y < h / 2; y++)
			{
				int rowA = y * w;
				int rowB = (h - 1 - y) * w;
				for (int x = 0; x < w; x++)
				{
					Color tmp = pixels[rowA + x];
					pixels[rowA + x] = pixels[rowB + x];
					pixels[rowB + x] = tmp;
				}
			}
		}
	}
}
