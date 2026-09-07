using UnityEngine;

namespace StockProduction
{
	internal static class StockInventory
	{
		public static int GetCount(ComplexFabricator fabricator, ComplexRecipe recipe)
		{
			Tag tag = ResultTag(recipe);
			if (!tag.IsValid)
				return 0;
			return Mathf.FloorToInt(GetAmount(fabricator, tag));
		}

		public static float GetAmount(ComplexFabricator fabricator, Tag tag)
		{
			WorldInventory inventory = GetInventory(fabricator);
			if (inventory == null)
				return 0f;
			return inventory.GetTotalAmount(tag, false);
		}

		public static bool IsReady(ComplexFabricator fabricator)
		{
			WorldInventory inventory = GetInventory(fabricator);
			return inventory != null && inventory.HasValidCount;
		}

		public static string Format(float amount, Tag tag)
		{
			if (UsesUnits(tag))
				return GameUtil.GetFormattedUnits(amount);
			return GameUtil.GetFormattedMass(amount);
		}

		public static string Format(ComplexFabricator fabricator, ComplexRecipe recipe, float amount)
		{
			Tag tag = ResultTag(recipe);
			if (!tag.IsValid)
				return Mathf.FloorToInt(amount).ToString();
			return Format(amount, tag);
		}

		public static bool HasEnough(ComplexFabricator fabricator, ComplexRecipe recipe, int target)
		{
			return GetCount(fabricator, recipe) >= target;
		}

		public static bool UsesMass(ComplexRecipe recipe)
		{
			Tag tag = ResultTag(recipe);
			return tag.IsValid && !UsesUnits(tag);
		}

		public static Tag ResultTag(ComplexRecipe recipe)
		{
			if (recipe == null || recipe.results == null || recipe.results.Length == 0)
				return Tag.Invalid;
			return recipe.results[0].material;
		}

		public static int DefaultTarget(ComplexRecipe recipe)
		{
			int output = 1;
			if (recipe != null && recipe.results != null && recipe.results.Length > 0)
				output = Mathf.Max(1, Mathf.RoundToInt(recipe.results[0].amount));
			return Mathf.Max(10, output);
		}

		private static WorldInventory GetInventory(ComplexFabricator fabricator)
		{
			if (fabricator == null)
				return null;
			WorldContainer world = fabricator.GetMyWorld();
			if (world == null)
				return null;
			return world.worldInventory;
		}

		private static bool UsesUnits(Tag tag)
		{
			try
			{
				if (!tag.IsValid || GameTags.UnitCategories == null)
					return false;
				GameObject prefab = Assets.GetPrefab(tag);
				KPrefabID prefabId = prefab != null ? prefab.GetComponent<KPrefabID>() : null;
				foreach (Tag category in GameTags.UnitCategories)
				{
					if (tag == category)
						return true;
					if (prefabId != null && prefabId.HasTag(category))
						return true;
				}
			}
			catch
			{
			}
			return false;
		}
	}
}
