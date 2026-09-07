using System;
using System.Reflection;

namespace DrywallTileSkins
{
	internal static class I18n
	{
		public static void Register()
		{
			CanisMinor.Shared.LocalePo.Register(typeof(STRINGS), Mod.ContentPath, "[DrywallTileSkins] ");
		}

		public static string BuiltinName(string stem)
		{
			if (string.IsNullOrEmpty(stem))
				return null;
			FieldInfo field = typeof(STRINGS.DRYWALL_TILE_SKINS.BUILTIN).GetField(
				stem.ToUpperInvariant(),
				BindingFlags.Public | BindingFlags.Static);
			if (field == null || field.FieldType != typeof(LocString))
				return null;
			LocString loc = field.GetValue(null) as LocString;
			return loc;
		}

		public static string DescriptionForGroup(string groupId)
		{
			if (groupId == ModSettings.BuiltinGroupId)
				return STRINGS.DRYWALL_TILE_SKINS.BUILTIN_DESC;
			if (groupId == ModSettings.CustomGroupId)
				return STRINGS.DRYWALL_TILE_SKINS.CUSTOM_DESC;
			return STRINGS.DRYWALL_TILE_SKINS.FACADE_DESC;
		}
	}
}
