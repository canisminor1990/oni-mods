using TTS = ThermalInfoCards.STRINGS.THERMAL_INFO_CARDS;

namespace ThermalInfoCards
{
	internal sealed class BuildTooltip
	{
		public BuildingDef Def { get; set; }

		public void AddThermalInfo(DescriptorPanel effectsPane, Tag elementTag)
		{
			Element element = ElementLoader.GetElement(elementTag);
			Descriptor desc = default;
			var descriptors = GameUtil.GetMaterialDescriptors(elementTag);
			desc.SetupDescriptor(
				global::STRINGS.ELEMENTS.MATERIAL_MODIFIERS.EFFECTS_HEADER,
				global::STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.EFFECTS_HEADER);
			if (descriptors.Count > 0)
				descriptors.Insert(0, desc);

			if (element != null && Def != null)
			{
				float[] masses = Def.Mass;
				string name = Def.Name ?? "";
				float mass = ThermalUtil.GetAdjustedMass(
					Def.BuildingComplete,
					Def,
					masses != null && masses.Length > 0 ? masses[0] : 0.0f);
				float tc = element.thermalConductivity * Def.ThermalConductivity;
				float tMass = GameUtil.GetDisplaySHC(mass * element.specificHeatCapacity);
				string deg = GameUtil.GetTemperatureUnitSuffix().Trim();
				string kDtu = global::STRINGS.UI.UNITSUFFIXES.HEAT.KDTU.text?.Trim();

				if (descriptors.Count == 0)
					descriptors.Add(desc);

				desc.SetupDescriptor(
					string.Format(TTS.EFFECT_CONDUCTIVITY, tc),
					string.Format(
						TTS.BUILDING_CONDUCTIVITY,
						name,
						GameUtil.GetFormattedThermalConductivity(tc),
						tc,
						deg,
						global::STRINGS.UI.UNITSUFFIXES.HEAT.DTU_S.text?.Trim()));
				desc.IncreaseIndent();
				descriptors.Add(desc);

				desc.SetupDescriptor(
					string.Format(TTS.EFFECT_THERMAL_MASS, tMass, kDtu, deg),
					string.Format(TTS.BUILDING_THERMAL_MASS, name, tMass, kDtu, deg));
				descriptors.Add(desc);

				Element hotElement = element.highTempTransition;
				if (ThermalUtil.IsValidTransition(hotElement, element))
				{
					string meltTemp = GameUtil.GetFormattedTemperature(element.highTemp + ThermalUtil.TransitionHysteresis);
					desc.SetupDescriptor(
						string.Format(TTS.EFFECT_MELT_TEMPERATURE, meltTemp),
						string.Format(
							TTS.BUILDING_MELT_TEMPERATURE,
							name,
							meltTemp,
							ThermalUtil.FormatName(hotElement, global::STRINGS.UI.StripLinkFormatting(element.name))));
					descriptors.Add(desc);
				}
			}

			if (descriptors.Count > 0)
			{
				effectsPane.gameObject.SetActive(true);
				effectsPane.SetDescriptors(descriptors);
			}
			else
			{
				effectsPane.gameObject.SetActive(false);
			}
		}

		public void ClearDef()
		{
			Def = null;
		}
	}
}
