using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DrywallTileSkins
{
	internal static class TextureUtil
	{
		private static int interiorLogCount;
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
			Graphics.Blit(source, rt);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = rt;
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

		public static Texture2D Crop(Texture2D source, int x, int y, int width, int height)
		{
			x = Mathf.Clamp(x, 0, source.width - 1);
			y = Mathf.Clamp(y, 0, source.height - 1);
			width = Mathf.Clamp(width, 1, source.width - x);
			height = Mathf.Clamp(height, 1, source.height - y);

			Color[] pixels = source.GetPixels(x, y, width, height);
			Texture2D cropped = new Texture2D(width, height, TextureFormat.RGBA32, false)
			{
				name = source.name + "_interior",
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Repeat
			};
			cropped.SetPixels(pixels);
			cropped.Apply(false, false);
			return cropped;
		}

		public static Texture2D LoadPng(string path)
		{
			if (!System.IO.File.Exists(path))
				return null;

			byte[] bytes = System.IO.File.ReadAllBytes(path);
			Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
			{
				name = System.IO.Path.GetFileNameWithoutExtension(path),
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Repeat
			};
			if (!tex.LoadImage(bytes))
				return null;
			tex.Apply(false, false);
			return tex;
		}

		public static Sprite MakeSprite(Texture2D tex)
		{
			if (tex == null)
				return null;
			return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
		}

		public static Texture2D ExtractInterior(TextureAtlas layout, Texture2D texture = null)
		{
			if (layout == null && texture == null)
				return null;

			Texture2D source = texture != null ? texture : layout.texture;
			if (source == null)
				return null;

			Texture2D readable = GetReadable(source);
			if (readable == null)
				return null;

			if (layout != null && TryGetInteriorRect(layout, readable.width, readable.height, out int x, out int y, out int w, out int h, out string itemName))
			{
				if (interiorLogCount < 8)
				{
					interiorLogCount++;
					Debug.Log("[DrywallTileSkins] interior " + source.name + " crop " + w + "x" + h + " from " + itemName);
				}
				return Crop(readable, x, y, w, h);
			}

			int size = Mathf.Min(readable.width, readable.height);
			if (layout != null && layout.tileDimension > 0)
				size = Mathf.Min(size, layout.tileDimension);
			else
				size = Mathf.Max(32, size / 4);

			int cx = (readable.width - size) / 2;
			int cy = (readable.height - size) / 2;
			return Crop(readable, cx, cy, size, size);
		}

		private static bool TryGetInteriorRect(TextureAtlas atlas, int texW, int texH, out int x, out int y, out int w, out int h, out string itemName)
		{
			x = y = w = h = 0;
			itemName = "";
			if (atlas.items == null || atlas.items.Length == 0)
				return false;

			TextureAtlas.Item best = default;
			int bestScore = -1;
			for (int i = 0; i < atlas.items.Length; i++)
			{
				TextureAtlas.Item item = atlas.items[i];
				if (string.IsNullOrEmpty(item.name) || !TryParseConnectionBits(item.name, out int required, out int forbidden))
					continue;
				if (forbidden != 0)
					continue;

				int score = CountBits(required);
				if (required == 0xFF)
					score += 32;
				else if ((required & 0x5A) == 0x5A)
					score += 16;

				if (score > bestScore)
				{
					bestScore = score;
					best = item;
				}
			}

			if (bestScore < 0)
				return false;

			itemName = best.name;
			Vector4 uv = best.uvBox;
			float u0 = Mathf.Min(uv.x, uv.z);
			float u1 = Mathf.Max(uv.x, uv.z);
			float v0 = Mathf.Min(uv.y, uv.w);
			float v1 = Mathf.Max(uv.y, uv.w);
			x = Mathf.Clamp(Mathf.RoundToInt(u0 * texW), 0, texW - 1);
			y = Mathf.Clamp(Mathf.RoundToInt(v0 * texH), 0, texH - 1);
			w = Mathf.Clamp(Mathf.RoundToInt((u1 - u0) * texW), 1, texW - x);
			h = Mathf.Clamp(Mathf.RoundToInt((v1 - v0) * texH), 1, texH - y);

			if (w > 8 && h > 8)
			{
				// 9-slice uvBox covers the 1.5-cell mesh (1 cell + 0.25 trim each side).
				// Keep the center 1 cell: 1/6 inset each side. 192 -> 128.
				int insetX = Mathf.Max(1, Mathf.RoundToInt(w / 6f));
				int insetY = Mathf.Max(1, Mathf.RoundToInt(h / 6f));
				x += insetX;
				y += insetY;
				w = Mathf.Max(8, w - insetX * 2);
				h = Mathf.Max(8, h - insetY * 2);
			}

			return w > 4 && h > 4;
		}

		private static bool TryParseConnectionBits(string name, out int required, out int forbidden)
		{
			required = 0;
			forbidden = 0;
			int forbiddenStart = name.Length - 4 - 8;
			int requiredStart = forbiddenStart - 1 - 8;
			if (requiredStart >= 0 && forbiddenStart + 8 <= name.Length
				&& TryParseBits(name, requiredStart, out required)
				&& TryParseBits(name, forbiddenStart, out forbidden))
				return true;

			MatchCollection matches = Regex.Matches(name, @"[01]{8}");
			if (matches.Count < 2)
				return false;
			required = Convert.ToInt32(matches[matches.Count - 2].Value, 2);
			forbidden = Convert.ToInt32(matches[matches.Count - 1].Value, 2);
			return true;
		}

		private static bool TryParseBits(string name, int start, out int value)
		{
			value = 0;
			if (start < 0 || start + 8 > name.Length)
				return false;
			try
			{
				value = Convert.ToInt32(name.Substring(start, 8), 2);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static int CountBits(int value)
		{
			int count = 0;
			while (value != 0)
			{
				count += value & 1;
				value >>= 1;
			}
			return count;
		}
	}
}
