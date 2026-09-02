using UnityEngine;

namespace DuplicantPortraits
{
	internal static class PortraitUtil
	{
		public static Texture2D GetPortrait(Personality personality)
		{
			if (personality == null)
				return null;

			Texture2D tex = FromSprite(SafeMiniIcon(personality));
			if (IsUsable(tex))
				return tex;

			tex = FromSprite(Assets.GetSprite(personality.nameStringKey));
			if (IsUsable(tex))
				return tex;

			if (!string.IsNullOrEmpty(personality.Id))
			{
				tex = FromSprite(Assets.GetSprite(personality.Id));
				if (IsUsable(tex))
					return tex;
			}

			return tex;
		}

		private static Sprite SafeMiniIcon(Personality personality)
		{
			try
			{
				return personality.GetMiniIcon();
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "GetMiniIcon failed for " + personality.Id + ": " + ex.Message);
				return null;
			}
		}

		private static Texture2D FromSprite(Sprite sprite)
		{
			if (sprite == null)
				return null;
			try
			{
				return TextureUtil.FromSprite(sprite);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "sprite extract failed " + sprite.name + ": " + ex.Message);
				return null;
			}
		}

		private static bool IsUsable(Texture2D tex)
		{
			return tex != null && tex.width >= 8 && tex.height >= 8;
		}
	}
}
