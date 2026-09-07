using System;
using System.Collections.Generic;
using STRINGS;
using UnityEngine;

public class ComplexRecipe : IHasDlcRestrictions
{
	public enum RecipeNameDisplay
	{
		Ingredient,
		Result,
		IngredientToResult,
		ResultWithIngredient,
		Composite,
		HEP,
		Custom
	}

	public class RecipeElement
	{
		public struct IngredientDataSet
		{
			public Tag[] substitutionOptions;

			public float[] amounts;

			public IngredientDataSet(Tag[] substitutionOptions, float[] amounts)
			{
				this.substitutionOptions = substitutionOptions;
				this.amounts = amounts;
			}
		}

		public enum TemperatureOperation
		{
			AverageTemperature,
			Heated,
			Melted,
			Dehydrated
		}

		public Tag material;

		public Tag[] possibleMaterials;

		public float[] possibleMaterialAmounts;

		public TemperatureOperation temperatureOperation;

		public bool storeElement;

		public bool inheritElement;

		public string facadeID;

		public bool doNotConsume;

		public float amount { get; set; }

		public RecipeElement(Tag[] materialOptions, float amount)
		{
			material = null;
			possibleMaterials = materialOptions;
			this.amount = amount;
			temperatureOperation = TemperatureOperation.AverageTemperature;
		}

		public RecipeElement(Tag[] materialOptions, float[] amounts)
		{
			material = null;
			possibleMaterials = materialOptions;
			possibleMaterialAmounts = amounts;
			temperatureOperation = TemperatureOperation.AverageTemperature;
		}

		public RecipeElement(Tag[] materialOptions, float amount, TemperatureOperation temperatureOperation, string facadeID, bool storeElement = false, bool inheritElement = false)
		{
			material = null;
			possibleMaterials = materialOptions;
			this.amount = amount;
			this.temperatureOperation = temperatureOperation;
			this.storeElement = storeElement;
			this.facadeID = facadeID;
			this.inheritElement = inheritElement;
		}

		public RecipeElement(Tag[] materialOptions, float[] amounts, TemperatureOperation temperatureOperation, string facadeID, bool storeElement = false, bool inheritElement = false, bool doNotConsume = false)
		{
			material = null;
			possibleMaterials = materialOptions;
			possibleMaterialAmounts = amounts;
			amount = amount;
			this.temperatureOperation = temperatureOperation;
			this.storeElement = storeElement;
			this.facadeID = facadeID;
			this.inheritElement = inheritElement;
			this.doNotConsume = doNotConsume;
		}

		public RecipeElement(Tag material, float amount, bool inheritElement)
		{
			this.material = material;
			possibleMaterials = new Tag[1] { material };
			this.amount = amount;
			temperatureOperation = TemperatureOperation.AverageTemperature;
			this.inheritElement = inheritElement;
		}

		public RecipeElement(Tag material, float amount)
		{
			this.material = material;
			possibleMaterials = new Tag[1] { material };
			this.amount = amount;
			temperatureOperation = TemperatureOperation.AverageTemperature;
		}

		public RecipeElement(Tag material, float amount, TemperatureOperation temperatureOperation, bool storeElement = false)
		{
			this.material = material;
			possibleMaterials = new Tag[1] { material };
			this.amount = amount;
			this.temperatureOperation = temperatureOperation;
			this.storeElement = storeElement;
		}

		public RecipeElement(Tag material, float amount, TemperatureOperation temperatureOperation, string facadeID, bool storeElement = false)
		{
			this.material = material;
			possibleMaterials = new Tag[1] { material };
			this.amount = amount;
			this.temperatureOperation = temperatureOperation;
			this.storeElement = storeElement;
			this.facadeID = facadeID;
		}

		public RecipeElement(EdiblesManager.FoodInfo foodInfo, float amount, bool DoNotConsume = false)
		{
			material = foodInfo.Id;
			possibleMaterials = new Tag[1] { material };
			this.amount = amount;
			doNotConsume = DoNotConsume;
		}
	}

	public string id;

	public string recipeCategoryID;

	public RecipeElement[] ingredients;

	public RecipeElement[] results;

	public float time;

	public GameObject FabricationVisualizer;

	public int consumedHEP;

	public int producedHEP;

	private string[] requiredDlcIds;

	private string[] forbiddenDlcIds;

	public RecipeNameDisplay nameDisplay;

	public string customName;

	public string customSpritePrefabID;

	public string description;

	public Func<string> runTimeDescription;

	public List<Tag> fabricators;

	public int sortOrder;

	public string requiredTech;

	public bool ProductHasFacade { get; set; }

	public bool RequiresAllIngredientsDiscovered { get; set; }

	public Tag FirstResult => results[0].material;

	public void SetDLCRestrictions(string[] required, string[] forbidden)
	{
		requiredDlcIds = required;
		forbiddenDlcIds = forbidden;
	}

	public string[] GetRequiredDlcIds()
	{
		return requiredDlcIds;
	}

	public string[] GetForbiddenDlcIds()
	{
		return forbiddenDlcIds;
	}

	public bool IsAnyProductDeprecated()
	{
		RecipeElement[] array = results;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject prefab = Assets.GetPrefab(array[i].material);
			if (prefab != null && prefab.HasTag(GameTags.DeprecatedContent))
			{
				return true;
			}
		}
		return false;
	}

	private static GameObject CreateFabricationVisualizer(string anim, string nameRoot = null)
	{
		GameObject gameObject = new GameObject();
		if (nameRoot != null)
		{
			gameObject.name = nameRoot + "Visualizer";
		}
		gameObject.SetActive(value: false);
		gameObject.transform.SetLocalPosition(Vector3.zero);
		KBatchedAnimController kBatchedAnimController = gameObject.AddComponent<KBatchedAnimController>();
		kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim(anim) };
		kBatchedAnimController.initialAnim = "fabricating";
		kBatchedAnimController.isMovable = true;
		KBatchedAnimTracker kBatchedAnimTracker = gameObject.AddComponent<KBatchedAnimTracker>();
		kBatchedAnimTracker.symbol = new HashedString("meter_ration");
		kBatchedAnimTracker.offset = Vector3.zero;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		return gameObject;
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results)
	{
		this.id = id;
		this.ingredients = ingredients;
		this.results = results;
		recipeCategoryID = ComplexRecipeManager.MakeRecipeCategoryID(id, "Default", results[0].material.ToString());
		if (!ComplexRecipeManager.Get().IsPostProcessing)
		{
			ComplexRecipeManager.Get().preProcessRecipes.Add(this);
		}
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results, int consumedHEP, int producedHEP)
		: this(id, ingredients, results)
	{
		this.consumedHEP = consumedHEP;
		this.producedHEP = producedHEP;
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results, int consumedHEP)
		: this(id, ingredients, results, consumedHEP, 0)
	{
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results, string[] requiredDlcIds)
		: this(id, ingredients, results, requiredDlcIds, null)
	{
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: this(id, ingredients, results)
	{
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results, int consumedHEP, int producedHEP, string[] requiredDlcIds)
		: this(id, ingredients, results, consumedHEP, producedHEP, requiredDlcIds, null)
	{
	}

	public ComplexRecipe(string id, RecipeElement[] ingredients, RecipeElement[] results, int consumedHEP, int producedHEP, string[] requiredDlcIds, string[] forbiddenDlcIds)
		: this(id, ingredients, results, consumedHEP, producedHEP)
	{
		this.requiredDlcIds = requiredDlcIds;
		this.forbiddenDlcIds = forbiddenDlcIds;
	}

	public void SetFabricationAnim(string anim)
	{
		FabricationVisualizer = CreateFabricationVisualizer(anim, id);
	}

	public float TotalResultUnits()
	{
		float num = 0f;
		RecipeElement[] array = results;
		foreach (RecipeElement recipeElement in array)
		{
			num += recipeElement.amount;
		}
		return num;
	}

	public bool RequiresTechUnlock()
	{
		return !string.IsNullOrEmpty(requiredTech);
	}

	public bool IsRequiredTechUnlocked()
	{
		if (string.IsNullOrEmpty(requiredTech))
		{
			return true;
		}
		return Db.Get().Techs.Get(requiredTech).IsComplete();
	}

	public bool IsRequiredTechOrPOIUnlocked()
	{
		if (IsRequiredTechUnlocked())
		{
			return true;
		}
		if (string.IsNullOrEmpty(requiredTech))
		{
			return false;
		}
		Tech tech = Db.Get().Techs.Get(requiredTech);
		if (tech == null || tech.unlockedItems == null)
		{
			return false;
		}
		foreach (TechItem unlockedItem in tech.unlockedItems)
		{
			if (unlockedItem.isPOIUnlock && unlockedItem.IsComplete())
			{
				return true;
			}
		}
		return false;
	}

	public Sprite GetUIIcon()
	{
		Sprite result = null;
		Tag tag = ((nameDisplay == RecipeNameDisplay.Ingredient) ? ingredients[0].material : results[0].material);
		if (nameDisplay == RecipeNameDisplay.Custom && !string.IsNullOrEmpty(customSpritePrefabID))
		{
			tag = customSpritePrefabID;
		}
		KBatchedAnimController component = Assets.GetPrefab(tag).GetComponent<KBatchedAnimController>();
		if (component != null)
		{
			result = Def.GetUISpriteFromMultiObjectAnim(component.AnimFiles[0]);
		}
		return result;
	}

	public Color GetUIColor()
	{
		return Color.white;
	}

	public string GetUIName(bool includeAmounts)
	{
		string text = (results[0].facadeID.IsNullOrWhiteSpace() ? results[0].material.ProperName() : GameTagExtensions.ProperName(results[0].facadeID));
		switch (nameDisplay)
		{
		case RecipeNameDisplay.Result:
			if (includeAmounts)
			{
				return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_SIMPLE_INCLUDE_AMOUNTS, text, results[0].amount);
			}
			return text;
		case RecipeNameDisplay.IngredientToResult:
			if (includeAmounts)
			{
				return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_FROM_TO_INCLUDE_AMOUNTS, ingredients[0].material.ProperName(), text, ingredients[0].amount, results[0].amount);
			}
			return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_FROM_TO, ingredients[0].material.ProperName(), text);
		case RecipeNameDisplay.ResultWithIngredient:
			if (includeAmounts)
			{
				return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_WITH_INCLUDE_AMOUNTS, ingredients[0].material.ProperName(), text, ingredients[0].amount, results[0].amount);
			}
			return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_WITH, ingredients[0].material.ProperName(), text);
		case RecipeNameDisplay.Composite:
			if (includeAmounts)
			{
				return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_FROM_TO_COMPOSITE_INCLUDE_AMOUNTS, ingredients[0].material.ProperName(), text, results[1].material.ProperName(), ingredients[0].amount, results[0].amount, results[1].amount);
			}
			return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_FROM_TO_COMPOSITE, ingredients[0].material.ProperName(), text, results[1].material.ProperName());
		case RecipeNameDisplay.HEP:
			if (includeAmounts)
			{
				return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_FROM_TO_HEP_INCLUDE_AMOUNTS, ingredients[0].material.ProperName(), results[1].material.ProperName(), ingredients[0].amount, producedHEP, results[1].amount);
			}
			return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_FROM_TO_HEP, ingredients[0].material.ProperName(), text);
		case RecipeNameDisplay.Custom:
			return customName;
		default:
			if (includeAmounts)
			{
				return string.Format(UI.UISIDESCREENS.REFINERYSIDESCREEN.RECIPE_SIMPLE_INCLUDE_AMOUNTS, ingredients[0].material.ProperName(), ingredients[0].amount);
			}
			return ingredients[0].material.ProperName();
		}
	}
}
