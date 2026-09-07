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

		public class SETTINGS
		{
			public static LocString BUTTON = "Settings";
			public static LocString TITLE = "More Room Types";
			public static LocString HINT = "Enable or disable each extra room type. Fully quit Oxygen Not Included after changing these, then launch again.";
			public static LocString ALL_ON = "Enable all";
			public static LocString ALL_OFF = "Disable all";
		}

		public class ROOMS
		{
			public class TYPES
			{
				public class GRAVEYARD
				{
					public static LocString NAME = "Graveyard";
					public static LocString DESCRIPTION = "It makes Duplicants happy to think that they are still alive.\n\nVisiting a Graveyard will raise or lower Duplicants' Stress.";
					public static LocString EFFECT = "- Stress bonus";
					public static LocString TOOLTIP = "Visiting a Graveyard will raise or lower Duplicants' Stress";
				}

				public class GYMROOM
				{
					public static LocString NAME = "Gym Room";
					public static LocString DESCRIPTION = "Professional equipment lets Duplicants exercise more efficiently.\n\nWorking a Manual Generator in a Gym Room improves Athletics training.";
					public static LocString EFFECT = "- Athletics training bonus";
					public static LocString TOOLTIP = "Working a Manual Generator in a Gym Room improves Athletics training";
				}

				public class MUSEUM
				{
					public static LocString NAME = "Art Museum";
					public static LocString DESCRIPTION = "It used to be a storehouse, before Meep confused Pedestal content with art.\n\nVisiting an Art Museum will improve Duplicants' Morale.";
					public static LocString EFFECT = "- Morale bonus";
					public static LocString TOOLTIP = "Visiting an Art Museum will improve Duplicants' Morale";
				}

				public class MUSEUMSPACE
				{
					public static LocString NAME = "Space Museum";
					public static LocString DESCRIPTION = "Perfect for storing Ancient Knowledge on the pedestals.\n\nVisiting a Space Museum will improve Duplicants' Morale.";
					public static LocString EFFECT = "- Morale bonus";
					public static LocString TOOLTIP = "Visiting a Space Museum will improve Duplicants' Morale";
				}

				public class PRIVATEBATHROOM
				{
					public static LocString NAME = "Private Bathroom";
					public static LocString DESCRIPTION = "Finally, a place to truly be alone with one's thoughts.\n\nUsing a Private Bathroom will greatly improve Duplicants' Morale.\nShowers in this room finish faster.";
					public static LocString EFFECT = "- Morale bonus";
					public static LocString TOOLTIP = "Using a Private Bathroom will greatly improve Duplicants' Morale";
				}

				public class WAREHOUSE
				{
					public static LocString NAME = "Warehouse";
					public static LocString DESCRIPTION = "An enclosed space for keeping supplies in order.\n\nA Warehouse does not change storage capacity.";
					public static LocString EFFECT = "- No effect";
					public static LocString TOOLTIP = "A Warehouse does not change storage capacity";
				}

				public class BATTERYROOM
				{
					public static LocString NAME = "Battery Room";
					public static LocString DESCRIPTION = "A dedicated space for stored power.\n\nBatteries in a Battery Room lose charge more slowly.";
					public static LocString EFFECT = "- Reduced battery leakage";
					public static LocString TOOLTIP = "Batteries in a Battery Room lose charge more slowly";
				}

				public class WASTEROOM
				{
					public static LocString NAME = "Waste Processing Room";
					public static LocString DESCRIPTION = "Where refuse is put back to work.\n\nCompost, Sludge Presses and Fertilizer Synthesizers in a Waste Processing Room function more efficiently.";
					public static LocString EFFECT = "- Efficiency bonus";
					public static LocString TOOLTIP = "Compost, Sludge Presses and Fertilizer Synthesizers in a Waste Processing Room function more efficiently";
				}

				public class WATERROOM
				{
					public static LocString NAME = "Water Treatment Room";
					public static LocString DESCRIPTION = "Where dirty water gets a second chance.\n\nWater Sieves and Desalinators in a Water Treatment Room function more efficiently.";
					public static LocString EFFECT = "- Efficiency bonus";
					public static LocString TOOLTIP = "Water Sieves and Desalinators in a Water Treatment Room function more efficiently";
				}

				public class HALLWAY
				{
					public static LocString NAME = "Hallway";
					public static LocString DESCRIPTION = "A transit shaft for moving between floors.\n\nDuplicants in a Hallway gain Athletics.";
					public static LocString EFFECT = "- Athletics bonus";
					public static LocString TOOLTIP = "Duplicants in a Hallway gain Athletics";
				}

				public class NUCLEARPLANT
				{
					public static LocString NAME = "Nuclear Power Plant";
					public static LocString DESCRIPTION = "The perfect place for Duplicants to flex their Electrical Engineering skills.\n\nHeavy-duty generators built within a Nuclear Power Plant can be tuned up using microchips from power control stations to improve their <link=\"POWER\">Power</link> production.";
					public static LocString EFFECT = "- Enables <link=\"POWERSTATIONTOOLS\">Microchip</link> tune-ups on heavy-duty generators";
					public static LocString TOOLTIP = "Heavy-duty generators built in a Nuclear Power Plant can be tuned up using microchips from Power Control Stations to improve their Power production";
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
					public static LocString NAME = "4 Batteries";
					public static LocString DESCRIPTION = "At least 4 batteries. Rocket battery modules count.";
				}

				public class NO_EXTRA_INDUSTRIAL
				{
					public static LocString NAME = "No extra industrial machinery";
					public static LocString DESCRIPTION = "No industrial machinery except batteries, rocket battery modules, and power transformers.";
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

				public class PRIVATEBATHROOM
				{
					public static LocString NAME = "Private Bathroom";
					public static LocString DESCRIPTION = "This Duplicant used a Private Bathroom.";
					public static LocString SHOWER = "Shower time: -20%";
				}
			}
		}
	}
}
