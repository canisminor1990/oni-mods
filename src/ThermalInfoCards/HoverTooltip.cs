using BetterInfoCards;
using System;
using UnityEngine;
using TTS = ThermalInfoCards.STRINGS.THERMAL_INFO_CARDS;
using TEMP_SUFFIXES = STRINGS.UI.UNITSUFFIXES.TEMPERATURE;

namespace ThermalInfoCards
{
	internal sealed class HoverTooltip
	{
		internal const string ExportHeatEnergy = "CanisMinor.ThermalInfoCards.HeatEnergy";
		internal const string ExportThermalMass = "CanisMinor.ThermalInfoCards.ThermalMass";

		private const string AllTempsFormat = "{0} / {1} / {2}";
		private const string TempFormat = "{0:##0.#}{1}";

		public HoverTextDrawer Drawer;
		public TextStyleSetting Style;
		public PrimaryElement PrimaryElement;
		public int Cell;

		private readonly Sprite spriteCold;
		private readonly Sprite spriteDash;
		private readonly Sprite spriteHot;

		public HoverTooltip()
		{
			spriteDash = Assets.GetSprite("dash");
			spriteCold = Assets.GetSprite("crew_state_temp_down");
			spriteHot = Assets.GetSprite("crew_state_temp_up");
		}

		public void DisplayThermalInfo(Element element, float temperature, float mass, float insulation = 1.0f)
		{
			if (Drawer == null || Style == null)
				return;

			try
			{
				DisplayThermalInfoInner(element, temperature, mass, insulation);
			}
			catch (Exception ex)
			{
				Debug.LogWarning(Mod.LogPrefix + "DisplayThermalInfo: " + ex.Message);
				try
				{
					Drawer.DrawText(GameUtil.GetFormattedTemperature(temperature), Style);
				}
				catch
				{
				}
			}
		}

		private void DisplayThermalInfoInner(Element element, float temperature, float mass, float insulation)
		{
			bool showExtra = !ModSettings.OnlyOnThermalOverlay
				|| (SimDebugView.Instance != null && SimDebugView.Instance.GetMode() == OverlayModes.Temperature.ID);
			if (element == null || element.specificHeatCapacity <= 0.0f || !showExtra)
			{
				Drawer.DrawText(GetTemperatureString(temperature), Style);
				return;
			}

			string name = global::STRINGS.UI.StripLinkFormatting(element.name);
			DisplayThermalStats(element, temperature, mass, insulation);

			Element coldElement = element.lowTempTransition;
			if (ThermalUtil.IsValidTransition(coldElement, element))
			{
				DisplayTransitionSprite();
				DisplayTransition(
					coldElement,
					Mathf.Max(0.1f, element.lowTemp - ThermalUtil.TransitionHysteresis),
					element.lowTempTransitionOreID,
					element.lowTempTransitionOreMassConversion,
					name);
			}

			Element hotElement = element.highTempTransition;
			if (ThermalUtil.IsValidTransition(hotElement, element))
			{
				DisplayTransitionSprite(hot: true);
				DisplayTransition(
					hotElement,
					element.highTemp + ThermalUtil.TransitionHysteresis,
					element.highTempTransitionOreID,
					element.highTempTransitionOreMassConversion,
					name);
			}
		}

		private void DisplayThermalStats(Element element, float temp, float mass, float insulation)
		{
			float shc = element.specificHeatCapacity;
			float tMass = GameUtil.GetDisplaySHC(mass * shc);
			float tEnergy = mass * shc * temp;
			string kDtu = global::STRINGS.UI.UNITSUFFIXES.HEAT.KDTU.text.Trim();

			Drawer.DrawText(GetTemperatureString(temp), Style);
			Drawer.NewLine();
			Drawer.DrawIcon(spriteDash);
			Drawer.DrawText(
				string.Format(
					TTS.HOVER_CONDUCTIVITY,
					GameUtil.GetFormattedThermalConductivity(element.thermalConductivity * insulation)),
				Style);
			Drawer.NewLine();
			Drawer.DrawIcon(spriteDash);
			ExportSelectToolData.GetSelectInfo_Patch.Export(ExportThermalMass, tMass);
			Drawer.DrawText(
				string.Format(TTS.THERMAL_MASS, ThermalUtil.DoScientific(tMass), kDtu, GameUtil.GetTemperatureUnitSuffix()?.Trim()),
				Style);
			Drawer.NewLine();
			Drawer.DrawIcon(spriteDash);
			ExportSelectToolData.GetSelectInfo_Patch.Export(ExportHeatEnergy, tEnergy);
			Drawer.DrawText(string.Format(TTS.HEAT_ENERGY, ThermalUtil.DoScientific(tEnergy), kDtu), Style);
		}

		private void DisplayElement(Element element, string oldElementName)
		{
			GameObject prefab = Assets.GetPrefab(element.tag);
			if (prefab != null)
			{
				Tuple<Sprite, Color> pair = Def.GetUISprite(prefab);
				if (pair != null)
					Drawer.DrawIcon(pair.first, pair.second, 22);
			}
			Drawer.DrawText(ThermalUtil.FormatName(element, oldElementName), Style);
		}

		private void DisplayTransition(Element newElement, float temp, SimHashes secondary, float ratio, string oldName)
		{
			DisplayElement(newElement, oldName);
			if (secondary != SimHashes.Vacuum && secondary != SimHashes.Void && ratio > 0.0f)
			{
				Element altElement = ElementLoader.FindElementByHash(secondary);
				ratio *= 100.0f;
				if (altElement != null)
				{
					Drawer.DrawText(string.Format(TTS.AND_JOIN, GameUtil.GetFormattedPercent(100.0f - ratio)), Style);
					DisplayElement(altElement, oldName);
					Drawer.DrawText(string.Format("[{0}]", GameUtil.GetFormattedPercent(ratio)), Style);
				}
			}
			Drawer.DrawText(string.Format(" ({0:##0.#})", GetTemperatureString(temp)), Style);
		}

		private void DisplayTransitionSprite(bool hot = false)
		{
			Sprite sprite = hot ? spriteHot : spriteCold;
			Drawer.NewLine();
			Drawer.DrawIcon(spriteDash);
			if (sprite != null)
				Drawer.DrawIcon(sprite, Color.white, 22);
			else
				Drawer.DrawText(TTS.CHANGES, Style);
			Drawer.DrawText(TTS.TO_JOIN, Style);
		}

		private static string GetTemperatureString(float temp)
		{
			if (!ModSettings.AllUnits)
				return GameUtil.GetFormattedTemperature(temp);

			string c = string.Format(TempFormat, GameUtil.GetTemperatureConvertedFromKelvin(temp, GameUtil.TemperatureUnit.Celsius), TEMP_SUFFIXES.CELSIUS);
			string f = string.Format(TempFormat, GameUtil.GetTemperatureConvertedFromKelvin(temp, GameUtil.TemperatureUnit.Fahrenheit), TEMP_SUFFIXES.FAHRENHEIT);
			string k = string.Format(TempFormat, temp, TEMP_SUFFIXES.KELVIN);
			switch (GameUtil.temperatureUnit)
			{
				case GameUtil.TemperatureUnit.Celsius:
					return string.Format(AllTempsFormat, c, f, k);
				case GameUtil.TemperatureUnit.Fahrenheit:
					return string.Format(AllTempsFormat, f, c, k);
				default:
					return string.Format(AllTempsFormat, k, c, f);
			}
		}
	}
}
