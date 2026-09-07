namespace StockProduction
{
	public class STRINGS
	{
		public class STOCK_PRODUCTION
		{
			public static LocString MODE_ONCE = "Once";
			public static LocString MODE_STOCK = "Maintain";
			public static LocString MODE_FOREVER = "Forever";
			public static LocString MODE_TOOLTIP = "Click to cycle Once, Maintain, and Forever.\n\nMaintain pauses when this world's Resources list reaches the number, and starts again when it drops below.";
			public static LocString TARGET_TOOLTIP = "Pause when stock reaches this amount (same units as the Resources list).";
			public static LocString QUEUE_TOOLTIP = "Maintain: {0}\nCurrent: {1}\n\nPauses when the colony has this many. Starts again when stock drops below.";
			public static LocString UNIT_KG = "kg";
			public static LocString UNIT_T = "t";
			public static LocString UNIT_TOOLTIP = "Click to switch kilograms and tonnes.\n1 t = 1000 kg.";
		}
	}
}
