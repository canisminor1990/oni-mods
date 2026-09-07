using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace StockProduction
{
	[SerializationConfig(MemberSerialization.OptIn)]
	public class StockRecipeController : KMonoBehaviour
	{
		public const int MaxAmount = 999999;

		[Serialize]
		private Dictionary<string, int> targets = new Dictionary<string, int>();

		[Serialize]
		private Dictionary<string, bool> paused = new Dictionary<string, bool>();

		public bool Applying { get; private set; }

		public bool IsEnabled(string recipeId)
		{
			return !string.IsNullOrEmpty(recipeId) && targets != null && targets.ContainsKey(recipeId);
		}

		public int GetTarget(string recipeId)
		{
			if (targets != null && targets.TryGetValue(recipeId, out int value))
				return value;
			return 0;
		}

		public bool IsPaused(string recipeId)
		{
			return paused != null && paused.TryGetValue(recipeId, out bool value) && value;
		}

		public void Enable(ComplexRecipe recipe, int preferredTarget)
		{
			if (recipe == null)
				return;
			int target = preferredTarget > 0 ? preferredTarget : StockInventory.DefaultTarget(recipe);
			SetTargetValue(recipe.id, target);
			RefreshRecipe(GetComponent<ComplexFabricator>(), recipe, force: true);
		}

		public void Disable(string recipeId, int queueCount)
		{
			if (string.IsNullOrEmpty(recipeId) || targets == null || !targets.ContainsKey(recipeId))
				return;
			targets.Remove(recipeId);
			paused?.Remove(recipeId);
			ComplexFabricator fabricator = GetComponent<ComplexFabricator>();
			ComplexRecipe recipe = fabricator != null ? fabricator.GetRecipe(recipeId) : null;
			if (recipe != null)
				ApplyQueue(fabricator, recipe, queueCount);
		}

		public void SetTarget(string recipeId, int target)
		{
			if (!IsEnabled(recipeId))
				return;
			SetTargetValue(recipeId, target);
			RefreshRecipe(GetComponent<ComplexFabricator>(), GetComponent<ComplexFabricator>()?.GetRecipe(recipeId), force: true);
		}

		public void CopyFrom(StockRecipeController other, ComplexFabricator dest)
		{
			targets.Clear();
			paused.Clear();
			if (other == null || other.targets == null)
				return;
			foreach (KeyValuePair<string, int> pair in other.targets)
			{
				targets[pair.Key] = pair.Value;
				if (other.paused != null && other.paused.TryGetValue(pair.Key, out bool isPaused))
					paused[pair.Key] = isPaused;
			}
			RefreshAll(dest);
		}

		public void RefreshAll(ComplexFabricator fabricator)
		{
			if (fabricator == null || targets == null || targets.Count == 0)
				return;
			if (!StockInventory.IsReady(fabricator))
				return;

			List<string> ids = new List<string>(targets.Keys);
			for (int i = 0; i < ids.Count; i++)
			{
				ComplexRecipe recipe = fabricator.GetRecipe(ids[i]);
				if (recipe != null)
					RefreshRecipe(fabricator, recipe, force: false);
			}
		}

		private void SetTargetValue(string recipeId, int target)
		{
			if (targets == null)
				targets = new Dictionary<string, int>();
			if (paused == null)
				paused = new Dictionary<string, bool>();
			targets[recipeId] = Mathf.Clamp(target, 1, MaxAmount);
			if (!paused.ContainsKey(recipeId))
				paused[recipeId] = false;
		}

		private void RefreshRecipe(ComplexFabricator fabricator, ComplexRecipe recipe, bool force)
		{
			if (fabricator == null || recipe == null || !IsEnabled(recipe.id))
				return;

			int stock = StockInventory.GetCount(fabricator, recipe);
			int target = GetTarget(recipe.id);
			bool wantPause = stock >= target;
			paused[recipe.id] = wantPause;

			int desired = ComplexFabricator.QUEUE_INFINITE;
			if (wantPause)
			{
				bool workingThis = fabricator.CurrentWorkingOrder != null
					&& fabricator.CurrentWorkingOrder.id == recipe.id;
				desired = workingThis ? 1 : 0;
			}

			if (force || QueueCount(fabricator, recipe) != desired)
				ApplyQueue(fabricator, recipe, desired);
		}

		private static int QueueCount(ComplexFabricator fabricator, ComplexRecipe recipe)
		{
			try
			{
				return fabricator.GetRecipeQueueCount(recipe);
			}
			catch
			{
				return 0;
			}
		}

		private void ApplyQueue(ComplexFabricator fabricator, ComplexRecipe recipe, int desired)
		{
			if (fabricator == null || recipe == null)
				return;
			Applying = true;
			try
			{
				fabricator.SetRecipeQueueCount(recipe, desired);
			}
			finally
			{
				Applying = false;
			}
		}
	}
}
