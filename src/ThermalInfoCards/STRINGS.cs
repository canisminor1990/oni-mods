namespace ThermalInfoCards
{
	public class STRINGS
	{
		public class THERMAL_INFO_CARDS
		{
			public static LocString SETTINGS_BUTTON = "Settings";
			public static LocString SETTINGS_TITLE = "Thermal Info Cards";
			public static LocString SETTINGS_HINT = "Thermal lines apply immediately. Card grouping and size tweaks may need a restart.";

			public static LocString DISPLAY_ALL = "Display All Units";
			public static LocString DISPLAY_ALL_TOOLTIP = "Show temperatures in Fahrenheit, Celsius, and Kelvin.";

			public static LocString ONLY_THERMAL = "Only on Thermal Overlay";
			public static LocString ONLY_THERMAL_TOOLTIP = "Show extra thermal stats only on the Temperature Overlay.";

			public static LocString HIDE_ELEMENT_CATEGORIES = "Hide Element Categories";
			public static LocString HIDE_ELEMENT_CATEGORIES_TOOLTIP = "Remove element category lines from hover cards.";

			public static LocString RESTRICT_SELECTION = "Restrict Selections";
			public static LocString RESTRICT_SELECTION_TOOLTIP = "On: only cards the base game would allow. Off: any visible card can be selected.";

			public static LocString FIRST_SELECTION_HOVER = "First Selection Hover";
			public static LocString FIRST_SELECTION_HOVER_TOOLTIP = "On: first click selects the highlighted object. Off: first click selects the first card.";

			public static LocString OVERRIDE_CARD_SIZE = "Compact Cards";
			public static LocString OVERRIDE_CARD_SIZE_TOOLTIP = "Slightly smaller fonts and icons on hover cards. Restart after changing.";

			public static LocString AND_JOIN = "[{0}] and ";
			public static LocString CHANGES = "Changes";
			public static LocString TO_JOIN = " to ";
			public static LocString SUM = " (\u03A3)";

			public static LocString HEAT_ENERGY = "Heat Energy: {0} {1}";
			public static LocString THERMAL_MASS = "Thermal Mass: {0} {1}/{2}";
			public static LocString HOVER_CONDUCTIVITY = "Thermal Conductivity: {0}";

			public static LocString EFFECT_CONDUCTIVITY = global::STRINGS.UI.FormatAsLink("Thermal Conductivity", "HEAT") + ": {0}";
			public static LocString EFFECT_MELT_TEMPERATURE = global::STRINGS.UI.FormatAsLink("Melting Point", "HEAT") + ": {0}";
			public static LocString EFFECT_THERMAL_MASS = global::STRINGS.UI.FormatAsLink("Thermal Mass", "HEAT") + ": {0:##0.#} {1}/{2}";

			public static LocString BUILDING_CONDUCTIVITY = "The completed {0} will have a thermal conductivity of <b>{1}</b>\n\nFor every 1 {3} of difference between the building's " +
				global::STRINGS.UI.FormatAsLink("Temperature", "HEAT") + " and its surroundings, {2:##0.#} {4} will be transferred";

			public static LocString BUILDING_MELT_TEMPERATURE = "The completed {0} will melt at <b>{1}</b> into {2}";

			public static LocString BUILDING_THERMAL_MASS = "The completed {0} will have a thermal mass of <b>{1:##0.#} {2}/{3}</b>\n\nAdding or removing {1:##0.#} {2} will change the building's " +
				global::STRINGS.UI.FormatAsLink("Temperature", "HEAT") + " by 1 {3}";
		}
	}
}
