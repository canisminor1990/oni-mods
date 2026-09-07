using ThermalInfoCards;

namespace BetterInfoCards
{
	public static class Options
	{
		public static CardOptions Opts
		{
			get { return CardOptions.Instance; }
		}
	}

	public sealed class CardOptions
	{
		internal static readonly CardOptions Instance = new CardOptions();

		public int InfoCardOpacity
		{
			get { return ModSettings.InfoCardOpacity; }
		}

		public bool HideElementCategories
		{
			get { return ModSettings.HideElementCategories; }
		}

		public bool UseBaseSelection
		{
			get { return ModSettings.UseBaseSelection; }
		}

		public bool ForceFirstSelectionToHover
		{
			get { return ModSettings.ForceFirstSelectionToHover; }
		}

		public float TemperatureBandWidth
		{
			get { return ModSettings.TemperatureBandWidth; }
		}

		public CardSize InfoCardSize
		{
			get { return CardSize.Instance; }
		}
	}

	public sealed class CardSize
	{
		internal static readonly CardSize Instance = new CardSize();

		public bool ShouldOverride
		{
			get { return ModSettings.OverrideCardSize; }
		}

		public int FontSizeChange
		{
			get { return ModSettings.FontSizeChange; }
		}

		public int LineSpacing
		{
			get { return ModSettings.LineSpacing; }
		}

		public int IconSizeChange
		{
			get { return ModSettings.IconSizeChange; }
		}

		public int YPadding
		{
			get { return ModSettings.YPadding; }
		}
	}
}
