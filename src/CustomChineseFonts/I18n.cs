namespace CustomChineseFonts
{
	internal static class I18n
	{
		public static void Register()
		{
			CanisMinor.Shared.LocalePo.Register(typeof(STRINGS), Mod.ContentPath, Mod.LogPrefix);
		}
	}
}
