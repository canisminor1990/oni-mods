using STRINGS;

namespace MoreRoomTypes
{
	public class STRINGS
	{
		public class TRANSLATION
		{
			public class AUTHOR
			{
				public static LocString NAME = "DarrenLee";
			}
		}

		public class ROOMS
		{
			public class TYPES
			{
				public class GRAVEYARD
				{
					public static LocString NAME = "Graveyard";
					public static LocString EFFECT = " - Stress: +10% or -10% for {0} cycle";
					public static LocString TOOLTIP = "It makes duplicants happy to think that they are still alive.";
				}

				public class GYMROOM
				{
					public static LocString NAME = "Gym Room";
					public static LocString EFFECT = " - Faster athletics increase by {0}";
					public static LocString TOOLTIP = "Professional Gym Room allows duplicants to exercise more efficiently.";
				}

				public class INDUSTRIAL
				{
					public static LocString NAME = "Industrial Room";
					public static LocString EFFECT = " - (no additional effect)";
					public static LocString TOOLTIP = "This room contains some noisy, greasy machinery.";
				}

				public class MUSEUM
				{
					public static LocString NAME = "Art Museum";
					public static LocString EFFECT = " • <style=\"KKeyword\">Morale</style>: +{0} of duplicant's Creativity skill as morale (up to +10)";
					public static LocString TOOLTIP = "It used to be a storehouse, before Meep confused Pedestal content with art.";
				}

				public class MUSEUMSPACE
				{
					public static LocString NAME = "Space Museum";
					public static LocString EFFECT = " • <style=\"KKeyword\">Morale</style>: \n\t + {0} of duplicant's Piloting skill as morale (up to +10)\n    After \"Cosmic Archaeology\" completion: \n\t additional +1 for each unique artifact (up to +5) \n\t additional +1 for each 2 more unique artifacts (up to +5)\n\t additional +1 for each 3 more unique artifacts (up to +5)";
					public static LocString TOOLTIP = "Perfect for storing Ancient Knowledge on the pedestals.";
				}

				public class PRIVATEBATHROOM
				{
					public static LocString NAME = "Private Bathroom";
					public static LocString EFFECT = " • Same morale bonus as a Washroom\n • Showers take 20% less time";
					public static LocString TOOLTIP = "Exactly one flush toilet, one sink, and one shower, with decor and a finished back wall.";
				}

				public class WAREHOUSE
				{
					public static LocString NAME = "Warehouse";
					public static LocString EFFECT = " - Organized storage area";
					public static LocString TOOLTIP = "Requires at least 4 storage buildings.";
				}

				public class BATTERYROOM
				{
					public static LocString NAME = "Battery Room";
					public static LocString EFFECT = " - Batteries lose charge more slowly";
					public static LocString TOOLTIP = "Requires at least 8 batteries. Rocket battery modules count.";
				}

				public class OXYGENROOM
				{
					public static LocString NAME = "Oxygen Room";
					public static LocString EFFECT = " - Equipment in this room runs 10% more efficiently";
					public static LocString TOOLTIP = "Requires an oxygen producer and a gas pump.";
				}

				public class WASTEROOM
				{
					public static LocString NAME = "Waste Processing Room";
					public static LocString EFFECT = " - Equipment in this room runs 10% more efficiently";
					public static LocString TOOLTIP = "Requires Compost plus a Sludge Press or Fertilizer Synthesizer.";
				}

				public class WATERROOM
				{
					public static LocString NAME = "Water Treatment Room";
					public static LocString EFFECT = " - Equipment in this room runs 10% more efficiently";
					public static LocString TOOLTIP = "Requires a Water Sieve or Desalinator.";
				}

				public class HALLWAY
				{
					public static LocString NAME = "Hallway";
					public static LocString EFFECT = " - Athletics bonus while inside";
					public static LocString TOOLTIP = "A transit shaft with a Ladder and a Fire Pole.";
				}

				public class NUCLEARPLANT
				{
					public static LocString NAME = "Nuclear Power Plant";
					public static LocString EFFECT = " - Same bonus as a Power Plant: microchip tune-ups on generators";
					public static LocString TOOLTIP = "Requires a Research Reactor and a Steam Turbine.";
				}
			}

			public class CRITERIA
			{
				public class GRAVE
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Tasteful Memorial", "GRAVE");
					public static LocString DESCRIPTION = "At least one Tasteful Memorial";
				}

				public class MANUALGENERATOR
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Manual Generator", "MANUALGENERATOR");
					public static LocString DESCRIPTION = "At least one Manual Generator";
				}

				public class WATERCOOLER
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Water Cooler", "WATERCOOLER");
					public static LocString DESCRIPTION = "At least one Water Cooler";
				}

				public class INDUSTRIAL
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Industrial Machinery", "BUILDCATEGORYREQUIREMENTCLASSINDUSTRIALMACHINERY");
					public static LocString DESCRIPTION = "At least one Industrial Machinery building";
				}

				public class PEDESTAL
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Pedestal", "ITEMPEDESTAL");
					public static LocString DESCRIPTION = "At least one Pedestal";
				}

				public class MASTERPIECES
				{
					public static LocString NAME = "{0} Masterpieces";
					public static LocString DESCRIPTION = "At least {0} Masterpieces.";
				}

				public class ARTIIFACTS
				{
					public static LocString NAME = "{0} Unique Artifacts";
					public static LocString DESCRIPTION = "At least {0} unique Artifacts.";
				}

				public class FLUSHTOILET_ONE
				{
					public static LocString NAME = "Exactly one Flush Toilet";
					public static LocString DESCRIPTION = "The room must contain exactly one Flush Toilet.";
				}

				public class SINK_ONE
				{
					public static LocString NAME = "Exactly one Sink";
					public static LocString DESCRIPTION = "The room must contain exactly one Sink.";
				}

				public class SHOWER_ONE
				{
					public static LocString NAME = "Exactly one Shower";
					public static LocString DESCRIPTION = "The room must contain exactly one Shower.";
				}

				public class STORAGE_BUILDINGS
				{
					public static LocString NAME = "4 Storage Buildings";
					public static LocString DESCRIPTION = "At least 4 storage buildings.";
				}

				public class BATTERIES
				{
					public static LocString NAME = "8 Batteries";
					public static LocString DESCRIPTION = "At least 8 batteries. Rocket battery modules count.";
				}

				public class OXYGEN_PRODUCER
				{
					public static LocString NAME = "Oxygen Producer";
					public static LocString DESCRIPTION = "At least one oxygen-producing building.";
				}

				public class GAS_PUMP
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Gas Pump", "GASPUMP");
					public static LocString DESCRIPTION = "At least one Gas Pump.";
				}

				public class COMPOST
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Compost", "COMPOST");
					public static LocString DESCRIPTION = "At least one Compost.";
				}

				public class WASTE_PROCESSOR
				{
					public static LocString NAME = "Waste Processor";
					public static LocString DESCRIPTION = "A Sludge Press or Fertilizer Synthesizer.";
				}

				public class WATER_TREATMENT
				{
					public static LocString NAME = "Water Treatment";
					public static LocString DESCRIPTION = "A Water Sieve or Desalinator.";
				}

				public class LADDER
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Ladder", "LADDER");
					public static LocString DESCRIPTION = "At least one Ladder.";
				}

				public class FIRE_POLE
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Fire Pole", "FIREPOLE");
					public static LocString DESCRIPTION = "At least one Fire Pole.";
				}

				public class NUCLEAR_REACTOR
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Research Reactor", "NUCLEARREACTOR");
					public static LocString DESCRIPTION = "At least one Research Reactor.";
				}

				public class STEAM_TURBINE
				{
					public static LocString NAME = (LocString)UI.FormatAsLink("Steam Turbine", "STEAMTURBINE2");
					public static LocString DESCRIPTION = "At least one Steam Turbine.";
				}
			}

			public class EFFECTS
			{
				public class MUSEUM
				{
					public static LocString NAME = "Art Museum";
					public static LocString DESCRIPTION = "Visited Art Museum";
				}

				public class MUSEUMSPACE
				{
					public static LocString NAME = "Space Museum";
					public static LocString DESCRIPTION = "Visited Space Museum";
				}

				public class GRAVE_GOOD
				{
					public static LocString NAME = "Graveyard Serenity";
					public static LocString DESCRIPTION = "This Duplicant has accepted their inevitable fate and is ready to embrace it.";
				}

				public class GRAVE_BAD
				{
					public static LocString NAME = "Graveyard Dread";
					public static LocString DESCRIPTION = "This Duplicant feels cold claws of Death crawling up their spine.";
				}

				public class HALLWAY
				{
					public static LocString NAME = "Hallway Transit";
					public static LocString DESCRIPTION = "This Duplicant is moving through a Hallway.";
				}
			}
		}
	}
}
