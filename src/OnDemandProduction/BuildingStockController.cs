using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace StockProduction
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class BuildingStockController : KMonoBehaviour, ISim1000ms
	{
		public enum Kind
		{
			Fertilizer,
			Microchip,
			FarmKit,
			Compost
		}

		public enum Mode
		{
			Auto,
			Maintain,
			Forever
		}

		private const int DigitMax = 99;
		private const int KgPerTon = 1000;

		public static readonly Operational.Flag HoldFlag = new Operational.Flag(
			"OnDemandHold",
			Operational.Flag.Type.Requirement);

		[Serialize]
		private Kind kind = Kind.Fertilizer;

		[Serialize]
		private Mode mode = Mode.Forever;

		[Serialize]
		private int target = 2000;

		[Serialize]
		private bool useTons = true;

		private Tag productTag = Tag.Invalid;
		private bool massProduct = true;
		private Operational operational;
		private TinkerStation tinker;

		public Kind BuildingKind => kind;

		public Mode CurrentMode => mode;

		public bool HasAutoMode => kind == Kind.Microchip || kind == Kind.FarmKit;

		public bool CanToggleUnit => massProduct;

		public bool UseTons => massProduct && useTons;

		public string ModeButtonText
		{
			get
			{
				if (mode == Mode.Maintain)
					return STRINGS.STOCK_PRODUCTION.MODE_STOCK;
				if (mode == Mode.Forever)
					return STRINGS.STOCK_PRODUCTION.MODE_FOREVER;
				return STRINGS.STOCK_PRODUCTION.MODE_AUTO;
			}
		}

		public void Configure(Kind configured)
		{
			kind = configured;
			if (HasAutoMode)
			{
				mode = Mode.Auto;
				target = 5;
				massProduct = false;
				useTons = false;
			}
			else
			{
				mode = Mode.Forever;
				target = 2000;
				massProduct = true;
				useTons = true;
			}
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			Subscribe(-905833192, OnCopySettings);
			gameObject.AddOrGet<CopyBuildingSettings>();
			GetComponent<Operational>()?.SetFlag(HoldFlag, true);
		}

		protected override void OnSpawn()
		{
			base.OnSpawn();
			DetectKind();
			if (HasAutoMode && mode == Mode.Forever)
				mode = Mode.Auto;
			ResolveProduct();
			operational = GetComponent<Operational>();
			tinker = GetComponent<TinkerStation>();
			Apply();
		}

		public void Sim1000ms(float dt)
		{
			Apply();
		}

		public int GetDisplayDigits()
		{
			if (!massProduct || !useTons)
				return Mathf.Clamp(target, 1, DigitMax);
			int tons = Mathf.RoundToInt(target / (float)KgPerTon);
			if (tons < 1)
				tons = 1;
			return Mathf.Clamp(tons, 1, DigitMax);
		}

		public void SetDisplayDigits(int digits)
		{
			digits = Mathf.Clamp(digits, 1, DigitMax);
			target = massProduct && useTons ? digits * KgPerTon : digits;
			Apply();
		}

		public void Step(int delta)
		{
			SetDisplayDigits(GetDisplayDigits() + delta);
		}

		public void ToggleUnit()
		{
			if (!massProduct)
				return;
			int digits = GetDisplayDigits();
			useTons = !useTons;
			SetDisplayDigits(digits);
		}

		public void CycleMode()
		{
			if (HasAutoMode)
				mode = mode == Mode.Maintain ? Mode.Auto : Mode.Maintain;
			else if (mode == Mode.Maintain)
				mode = Mode.Forever;
			else
				mode = Mode.Maintain;
			Apply();
		}

		private void DetectKind()
		{
			if (GetComponent<Compost>() != null)
			{
				kind = Kind.Compost;
				return;
			}

			TinkerStation station = GetComponent<TinkerStation>();
			if (station == null)
				return;
			if (station.outputPrefab == FarmStationConfig.TINKER_TOOLS)
				kind = Kind.FarmKit;
			else
				kind = Kind.Microchip;
		}

		private void ResolveProduct()
		{
			if (kind == Kind.Microchip)
			{
				productTag = PowerControlStationConfig.TINKER_TOOLS;
				massProduct = false;
			}
			else if (kind == Kind.FarmKit)
			{
				productTag = FarmStationConfig.TINKER_TOOLS;
				massProduct = false;
			}
			else if (kind == Kind.Compost)
			{
				productTag = SimHashes.Dirt.CreateTag();
				massProduct = true;
			}
			else
			{
				productTag = GameTags.Fertilizer;
				if (!productTag.IsValid)
					productTag = new Tag("Fertilizer");
				massProduct = true;
			}
		}

		internal static void NotifySettingsChanged()
		{
			BuildingStockController[] all = UnityEngine.Object.FindObjectsOfType<BuildingStockController>();
			for (int i = 0; i < all.Length; i++)
				all[i].Apply();

			DetailsScreen details = DetailsScreen.Instance;
			if (details == null)
				return;
			GameObject selected = AccessTools.Field(typeof(DetailsScreen), "target")?.GetValue(details) as GameObject
				?? AccessTools.Property(typeof(DetailsScreen), "target")?.GetValue(details) as GameObject;
			if (selected != null)
				details.Refresh(selected);
		}

		private void Apply()
		{
			if (!ModSettings.ExtraBuildings)
			{
				RestoreVanilla();
				return;
			}

			if (HasAutoMode)
			{
				if (tinker != null)
					tinker.alwaysTinker = mode == Mode.Forever || (mode == Mode.Maintain && !AtTarget());
				return;
			}

			if (operational == null)
				operational = GetComponent<Operational>();
			if (operational != null)
				operational.SetFlag(HoldFlag, !ShouldHold());
		}

		private void RestoreVanilla()
		{
			if (HasAutoMode)
			{
				if (tinker != null)
					tinker.alwaysTinker = false;
				return;
			}

			if (operational == null)
				operational = GetComponent<Operational>();
			if (operational != null)
				operational.SetFlag(HoldFlag, true);
		}

		private bool ShouldHold()
		{
			return mode == Mode.Maintain && AtTarget();
		}

		private bool AtTarget()
		{
			if (!StockInventory.IsReady(this) || !productTag.IsValid)
				return false;
			return StockInventory.GetCount(this, productTag) >= target;
		}

		private void OnCopySettings(object data)
		{
			GameObject sourceGo = data as GameObject;
			BuildingStockController other = sourceGo != null
				? sourceGo.GetComponent<BuildingStockController>()
				: null;
			if (other == null || other.kind != kind)
				return;
			mode = other.mode;
			target = other.target;
			useTons = other.useTons;
			Apply();
		}
	}
}
