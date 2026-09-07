using System.Globalization;
using UnityEngine;

namespace ThermalInfoCards
{
	internal static class ThermalUtil
	{
		public const float TransitionHysteresis = 3.0f;
		private const float ScientificThreshold = 1E+6f;
		private const string SmallFormat = "##0.#";

		public static string DoScientific(float value)
		{
			if (value < ScientificThreshold && value > -ScientificThreshold)
				return value.ToString(SmallFormat, CultureInfo.InvariantCulture);

			string scientific = value.ToString("E3", CultureInfo.InvariantCulture).ToLowerInvariant();
			int index = scientific.IndexOf('e');
			if (index > 0 && int.TryParse(scientific.Substring(index + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int exp))
				return string.Format("{0}x10<sup>{1}</sup>", scientific.Substring(0, index), exp);
			return scientific;
		}

		public static string FormatName(Element element, string originalName)
		{
			string name = global::STRINGS.UI.StripLinkFormatting(element.name);
			if (name == originalName)
			{
				if (element.IsLiquid)
					name = global::STRINGS.ELEMENTS.STATE.LIQUID + " " + name;
				else if (element.IsSolid)
					name = global::STRINGS.ELEMENTS.STATE.SOLID + " " + name;
				else if (element.IsGas)
					name = global::STRINGS.ELEMENTS.STATE.GAS + " " + name;
			}
			return name;
		}

		public static bool IsValidTransition(Element element, Element original)
		{
			if (element == null)
				return false;
			SimHashes id = element.id;
			if (id == SimHashes.Void || id == SimHashes.Vacuum)
				return false;
			return original == null || id != original.id;
		}

		public static float GetAdjustedMass(GameObject entity, BuildingDef def, float originalMass)
		{
			if (entity == null || def == null)
				return originalMass;
			if (entity.TryGetComponent(out SimCellOccupier sco) && sco.IsVisuallySolid)
				return originalMass;
			return def.MassForTemperatureModification;
		}
	}
}
