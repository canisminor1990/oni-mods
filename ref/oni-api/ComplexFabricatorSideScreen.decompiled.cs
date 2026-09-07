using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class ComplexFabricatorSideScreen : SideScreenContent
{
	public enum StyleSetting
	{
		GridResult,
		ListResult,
		GridInput,
		ListInput,
		ListInputOutput,
		GridInputOutput,
		ClassicFabricator,
		ListQueueHybrid
	}

	[Header("Recipe List")]
	[SerializeField]
	private GameObject recipeGrid;

	[Header("Recipe button variants")]
	[SerializeField]
	private GameObject recipeButton;

	[SerializeField]
	private GameObject recipeButtonMultiple;

	[SerializeField]
	private GameObject recipeButtonQueueHybrid;

	[SerializeField]
	private GameObject recipeCategoryHeader;

	[SerializeField]
	private Sprite buttonSelectedBG;

	[SerializeField]
	private Sprite buttonNormalBG;

	[SerializeField]
	private Sprite elementPlaceholderSpr;

	[SerializeField]
	public Sprite radboltSprite;

	private KToggle selectedToggle;

	public LayoutElement buttonScrollContainer;

	public RectTransform buttonContentContainer;

	[SerializeField]
	private GameObject elementContainer;

	[SerializeField]
	private LocText currentOrderLabel;

	[SerializeField]
	private LocText nextOrderLabel;

	private Dictionary<ComplexFabricator, int> selectedRecipeFabricatorMap = new Dictionary<ComplexFabricator, int>();

	public EventReference createOrderSound;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private LocText subtitleLabel;

	[SerializeField]
	private ToolTip subtitleTooltip;

	[SerializeField]
	private LocText noRecipesDiscoveredLabel;

	public TextStyleSetting styleTooltipHeader;

	public TextStyleSetting styleTooltipBody;

	public ColorStyleSetting emptyQueueColorStyle;

	public ColorStyleSetting standardQueueColorStyle;

	private ComplexFabricator targetFab;

	private string selectedRecipeCategory;

	private Dictionary<GameObject, List<ComplexRecipe>> recipeCategoryToggleMap;

	private Dictionary<string, GameObject> recipeCategories = new Dictionary<string, GameObject>();

	private List<GameObject> recipeToggles = new List<GameObject>();

	public SelectedRecipeQueueScreen recipeScreenPrefab;

	private SelectedRecipeQueueScreen recipeScreen;

	private int targetOrdersUpdatedSubHandle = -1;

	public override string GetTitle()
	{
		if (targetFab == null)
		{
			return Strings.Get(titleKey).ToString().Replace("{0}", "");
		}
		return string.Format(Strings.Get(titleKey), targetFab.GetProperName());
	}

	public override bool IsValidForTarget(GameObject target)
	{
		ComplexFabricator component = target.GetComponent<ComplexFabricator>();
		if (component != null)
		{
			return component.enabled;
		}
		return false;
	}

	public override void SetTarget(GameObject target)
	{
		ComplexFabricator component = target.GetComponent<ComplexFabricator>();
		if (component == null)
		{
			Debug.LogError("The object selected doesn't have a ComplexFabricator!");
			return;
		}
		UnsubscribeTarget();
		Initialize(component);
		targetOrdersUpdatedSubHandle = targetFab.Subscribe(1721324763, UpdateQueueCountLabels);
		UpdateQueueCountLabels();
	}

	private void UpdateQueueCountLabels(object data = null)
	{
		ComplexRecipe[] recipes = targetFab.GetRecipes();
		foreach (ComplexRecipe r in recipes)
		{
			GameObject gameObject = recipeToggles.Find((GameObject match) => recipeCategoryToggleMap[match].Contains(r));
			if (gameObject != null)
			{
				RefreshQueueCountDisplay(gameObject, targetFab);
				RefreshQueueTooltip(gameObject);
			}
		}
		if (targetFab.CurrentWorkingOrder != null)
		{
			currentOrderLabel.text = string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.CURRENT_ORDER, targetFab.CurrentWorkingOrder.GetUIName(includeAmounts: false));
		}
		else
		{
			currentOrderLabel.text = string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.CURRENT_ORDER, UI.UISIDESCREENS.FABRICATORSIDESCREEN.NO_WORKABLE_ORDER);
		}
		if (targetFab.NextOrder != null)
		{
			nextOrderLabel.text = string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.NEXT_ORDER, targetFab.NextOrder.GetUIName(includeAmounts: false));
		}
		else
		{
			nextOrderLabel.text = string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.NEXT_ORDER, UI.UISIDESCREENS.FABRICATORSIDESCREEN.NO_WORKABLE_ORDER);
		}
	}

	protected override void OnShow(bool show)
	{
		if (show)
		{
			AudioMixer.instance.Start(AudioMixerSnapshots.Get().FabricatorSideScreenOpenSnapshot);
		}
		else
		{
			AudioMixer.instance.Stop(AudioMixerSnapshots.Get().FabricatorSideScreenOpenSnapshot);
			DetailsScreen.Instance.ClearSecondarySideScreen();
			selectedRecipeCategory = "";
			selectedToggle = null;
		}
		base.OnShow(show);
	}

	public void Initialize(ComplexFabricator target)
	{
		if (target == null)
		{
			Debug.LogError("ComplexFabricator provided was null.");
			return;
		}
		targetFab = target;
		base.gameObject.SetActive(value: true);
		recipeCategoryToggleMap = new Dictionary<GameObject, List<ComplexRecipe>>();
		recipeToggles.ForEach(delegate(GameObject rbi)
		{
			Object.Destroy(rbi.gameObject);
		});
		recipeToggles.Clear();
		foreach (KeyValuePair<string, GameObject> recipeCategory in recipeCategories)
		{
			Object.Destroy(recipeCategory.Value.transform.parent.gameObject);
		}
		recipeCategories.Clear();
		int num = 0;
		ComplexRecipe[] recipes = targetFab.GetRecipes();
		Dictionary<string, List<ComplexRecipe>> dictionary = new Dictionary<string, List<ComplexRecipe>>();
		ComplexRecipe[] array = recipes;
		foreach (ComplexRecipe complexRecipe in array)
		{
			if (!dictionary.ContainsKey(complexRecipe.recipeCategoryID))
			{
				dictionary.Add(complexRecipe.recipeCategoryID, new List<ComplexRecipe>());
			}
			dictionary[complexRecipe.recipeCategoryID].Add(complexRecipe);
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, List<ComplexRecipe>> item in dictionary)
		{
			ComplexRecipe complexRecipe2 = item.Value[0];
			bool flag = false;
			if (DebugHandler.InstantBuildMode)
			{
				flag = true;
			}
			else if (item.Value[0].RequiresTechUnlock())
			{
				if ((item.Value[0].IsRequiredTechOrPOIUnlocked() || Db.Get().Techs.Get(item.Value[0].requiredTech).ArePrerequisitesComplete()) && (!item.Value[0].RequiresAllIngredientsDiscovered || item.Value.Find((ComplexRecipe match) => AllRecipeRequirementsDiscovered(match)) != null))
				{
					flag = true;
				}
			}
			else if (item.Value.Find((ComplexRecipe match) => target.GetRecipeQueueCount(match) != 0) != null)
			{
				flag = true;
			}
			else if (item.Value[0].RequiresAllIngredientsDiscovered)
			{
				if (item.Value.Find((ComplexRecipe match) => AllRecipeRequirementsDiscovered(match)) != null)
				{
					flag = true;
				}
			}
			else if (item.Value.Find((ComplexRecipe match) => AnyRecipeRequirementsDiscovered(match)) != null)
			{
				flag = true;
			}
			else if (item.Value.Find((ComplexRecipe match) => HasAnyRecipeRequirements(match)) != null)
			{
				flag = true;
			}
			if (!flag)
			{
				hashSet.Add(complexRecipe2.GetUIName(includeAmounts: false));
				continue;
			}
			num++;
			Tuple<Sprite, Color> uISprite = Def.GetUISprite(complexRecipe2.ingredients[0].material);
			Tuple<Sprite, Color> uISprite2 = Def.GetUISprite(complexRecipe2.results[0].material, complexRecipe2.results[0].facadeID);
			KToggle newToggle = null;
			GameObject gameObject;
			if (target.sideScreenStyle == StyleSetting.ListQueueHybrid)
			{
				newToggle = Util.KInstantiateUI<KToggle>(recipeButtonQueueHybrid, recipeGrid);
				gameObject = newToggle.gameObject;
				recipeCategoryToggleMap.Add(gameObject, item.Value);
				Image image = gameObject.GetComponentsInChildrenOnly<Image>()[2];
				if (complexRecipe2.nameDisplay == ComplexRecipe.RecipeNameDisplay.Ingredient)
				{
					image.sprite = uISprite.first;
					image.color = uISprite.second;
				}
				else if (complexRecipe2.nameDisplay == ComplexRecipe.RecipeNameDisplay.HEP)
				{
					image.sprite = radboltSprite;
				}
				else if (complexRecipe2.nameDisplay == ComplexRecipe.RecipeNameDisplay.Custom)
				{
					image.sprite = complexRecipe2.GetUIIcon();
				}
				else
				{
					image.sprite = uISprite2.first;
					image.color = uISprite2.second;
				}
				gameObject.GetComponentInChildren<LocText>().text = complexRecipe2.GetUIName(includeAmounts: false);
				bool flag2 = item.Value.Find((ComplexRecipe match) => HasAllRecipeRequirements(match)) != null;
				image.material = (flag2 ? Assets.UIPrefabs.TableScreenWidgets.DefaultUIMaterial : Assets.UIPrefabs.TableScreenWidgets.DesaturatedUIMaterial);
				RefreshQueueCountDisplay(gameObject, targetFab);
				RefreshQueueTooltip(gameObject);
				gameObject.gameObject.SetActive(value: true);
			}
			else
			{
				newToggle = Util.KInstantiateUI<KToggle>(recipeButton, recipeGrid);
				gameObject = newToggle.gameObject;
				Image componentInChildrenOnly = newToggle.gameObject.GetComponentInChildrenOnly<Image>();
				if (target.sideScreenStyle == StyleSetting.GridInput || target.sideScreenStyle == StyleSetting.ListInput)
				{
					componentInChildrenOnly.sprite = uISprite.first;
					componentInChildrenOnly.color = uISprite.second;
				}
				else
				{
					componentInChildrenOnly.sprite = uISprite2.first;
					componentInChildrenOnly.color = uISprite2.second;
				}
			}
			ToolTip reference = gameObject.GetComponent<HierarchyReferences>().GetReference<ToolTip>("ButtonTooltip");
			reference.toolTipPosition = ToolTip.TooltipPosition.Custom;
			reference.parentPositionAnchor = new Vector2(0f, 0.5f);
			reference.tooltipPivot = new Vector2(1f, 1f);
			reference.tooltipPositionOffset = new Vector2(-24f, 20f);
			reference.ClearMultiStringTooltip();
			reference.AddMultiStringTooltip(complexRecipe2.GetUIName(includeAmounts: false), styleTooltipHeader);
			reference.AddMultiStringTooltip(complexRecipe2.description, styleTooltipBody);
			if (complexRecipe2.runTimeDescription != null)
			{
				reference.AddMultiStringTooltip("\n" + complexRecipe2.runTimeDescription(), styleTooltipBody);
			}
			if (item.Value.Count > 1)
			{
				reference.AddMultiStringTooltip("\n" + UI.UISIDESCREENS.FABRICATORSIDESCREEN.TOOLTIPS.ADDITIONAL_INGREDIENT_OPTIONS_MESSAGE, styleTooltipBody);
			}
			newToggle.onClick += delegate
			{
				ToggleClicked(newToggle);
			};
			gameObject.SetActive(value: true);
			recipeToggles.Add(gameObject);
		}
		string text = "";
		if (recipeToggles.Count > 0)
		{
			VerticalLayoutGroup component = buttonContentContainer.GetComponent<VerticalLayoutGroup>();
			buttonScrollContainer.GetComponent<LayoutElement>().minHeight = Mathf.Min(451f, (float)(component.padding.top + component.padding.bottom) + (float)num * recipeButtonQueueHybrid.GetComponent<LayoutElement>().minHeight + (float)(num - 1) * component.spacing);
			text = targetFab.SideScreenSubtitleLabel;
			if (hashSet.Count > 0)
			{
				text = text + "  <color=#f5b042>(" + (dictionary.Count - hashSet.Count) + "/" + dictionary.Count + ")</color>";
			}
			subtitleLabel.SetText(text);
			noRecipesDiscoveredLabel.gameObject.SetActive(value: false);
		}
		else
		{
			text = string.Concat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.NORECIPEDISCOVERED, "  <color=#f5b042>(", (dictionary.Count - hashSet.Count).ToString(), "/", dictionary.Count.ToString(), ")</color>");
			subtitleLabel.SetText(text);
			noRecipesDiscoveredLabel.SetText(UI.UISIDESCREENS.FABRICATORSIDESCREEN.NORECIPEDISCOVERED_BODY);
			noRecipesDiscoveredLabel.gameObject.SetActive(value: true);
			buttonScrollContainer.GetComponent<LayoutElement>().minHeight = noRecipesDiscoveredLabel.GetComponent<LayoutElement>().minHeight + 10f;
		}
		if (hashSet.Count > 0)
		{
			subtitleTooltip.SetSimpleTooltip(string.Concat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.UNDISCOVERED_RECIPES, "\n\n    • ", string.Join("\n    • ", hashSet.ToArray())));
		}
		else
		{
			subtitleTooltip.SetSimpleTooltip("");
		}
		RefreshIngredientAvailabilityVis();
	}

	public void RefreshQueueCountDisplayForRecipeCategory(string recipeCategoryID, ComplexFabricator fabricator)
	{
		foreach (GameObject recipeToggle in recipeToggles)
		{
			if (recipeCategoryToggleMap[recipeToggle][0].recipeCategoryID == recipeCategoryID)
			{
				RefreshQueueCountDisplay(recipeToggle, fabricator);
				RefreshQueueTooltip(recipeToggle);
				break;
			}
		}
	}

	private void RefreshQueueCountDisplay(GameObject entryGO, ComplexFabricator fabricator)
	{
		HierarchyReferences component = entryGO.GetComponent<HierarchyReferences>();
		int recipeCategoryQueueCount = fabricator.GetRecipeCategoryQueueCount(recipeCategoryToggleMap[entryGO][0].recipeCategoryID);
		bool flag = recipeCategoryQueueCount == ComplexFabricator.QUEUE_INFINITE;
		component.GetReference<LocText>("CountLabel").text = (flag ? "" : recipeCategoryQueueCount.ToString());
		component.GetReference<RectTransform>("InfiniteIcon").gameObject.SetActive(flag);
		bool flag2 = !recipeCategoryToggleMap[entryGO][0].IsRequiredTechOrPOIUnlocked();
		GameObject obj = component.GetReference<RectTransform>("TechRequired").gameObject;
		obj.SetActive(flag2);
		KButton component2 = obj.GetComponent<KButton>();
		component2.ClearOnClick();
		if (flag2)
		{
			component2.onClick += delegate
			{
				ManagementMenu.Instance.OpenResearch(recipeCategoryToggleMap[entryGO][0].requiredTech);
			};
		}
		KButton reference = component.GetReference<KButton>("QueueBoxButton");
		reference.bgImage.colorStyleSetting = ((recipeCategoryQueueCount == 0) ? emptyQueueColorStyle : standardQueueColorStyle);
		reference.bgImage.ApplyColorStyleSetting();
		reference.ClearOnClick();
		_ = recipeCategoryToggleMap[entryGO][0].recipeCategoryID;
		reference.onClick += delegate
		{
			if (selectedToggle == null || selectedToggle.gameObject != entryGO.gameObject)
			{
				ToggleClicked(entryGO.GetComponent<KToggle>());
			}
			else
			{
				recipeScreen.SelectNextQueuedRecipeInCategory();
			}
			RefreshQueueTooltip(entryGO);
		};
		GameObject gameObject = component.GetReference<RectTransform>("DotContainer").gameObject;
		GameObject gameObject2 = component.GetReference<RectTransform>("DotPrefab").gameObject;
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			if (gameObject.transform.GetChild(i).gameObject != gameObject2)
			{
				Object.Destroy(gameObject.transform.GetChild(i).gameObject);
			}
		}
		int num = (from match in fabricator.GetRecipesWithCategoryID(recipeCategoryToggleMap[entryGO][0].recipeCategoryID)
			where targetFab.GetRecipeQueueCount(match) != 0
			select match).Count();
		if (num > 1)
		{
			for (int j = 0; j < Mathf.Min(num, 5); j++)
			{
				Util.KInstantiateUI(gameObject2, gameObject).SetActive(value: true);
			}
		}
	}

	private void RefreshQueueTooltip(GameObject entryGO)
	{
		HierarchyReferences component = entryGO.GetComponent<HierarchyReferences>();
		string recipeCategoryID = recipeCategoryToggleMap[entryGO][0].recipeCategoryID;
		ToolTip reference = component.GetReference<ToolTip>("QueueTooltip");
		int recipeCategoryQueueCount = targetFab.GetRecipeCategoryQueueCount(recipeCategoryID);
		if (recipeCategoryQueueCount != 0)
		{
			string text = string.Concat("<b>", UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_QUEUE, (recipeCategoryQueueCount == ComplexFabricator.QUEUE_INFINITE) ? "99+" : recipeCategoryQueueCount.ToString(), "</b>\n");
			foreach (ComplexRecipe item in targetFab.GetRecipesWithCategoryID(recipeCategoryToggleMap[entryGO][0].recipeCategoryID))
			{
				int recipeQueueCount = targetFab.GetRecipeQueueCount(item);
				if (recipeQueueCount == 0)
				{
					continue;
				}
				string text2 = "";
				ComplexRecipe.RecipeElement[] ingredients = item.ingredients;
				foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
				{
					if (text2 != "")
					{
						text2 += ", ";
					}
					text2 = text2 + "<color=#C76B99>" + TagManager.GetProperName(recipeElement.material, stripLink: true) + "</color>";
				}
				text2 = ((recipeQueueCount != ComplexFabricator.QUEUE_INFINITE) ? (recipeQueueCount + "x " + text2) : string.Concat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_FOREVER, ": ", text2));
				if (text != "")
				{
					text += "\n";
				}
				if (recipeScreen != null && recipeScreen.gameObject.activeInHierarchy && recipeScreen.IsSelectedMaterials(item))
				{
					text2 = "<b>" + text2 + "</b>";
				}
				text += text2;
			}
			text = text + "\n\n" + UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_QUEUE_CLICK_DESCRIPTION;
			reference.SetSimpleTooltip(text);
		}
		else
		{
			reference.SetSimpleTooltip(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_NONE);
		}
	}

	private void ToggleClicked(KToggle toggle)
	{
		if (!recipeCategoryToggleMap.ContainsKey(toggle.gameObject))
		{
			Debug.LogError("Recipe not found on recipe list.");
			return;
		}
		if (selectedToggle == toggle)
		{
			selectedToggle.isOn = false;
			selectedToggle = null;
			selectedRecipeCategory = "";
		}
		else
		{
			selectedToggle = toggle;
			selectedToggle.isOn = true;
			selectedRecipeCategory = recipeCategoryToggleMap[toggle.gameObject][0].recipeCategoryID;
			selectedRecipeFabricatorMap[targetFab] = recipeToggles.IndexOf(toggle.gameObject);
		}
		RefreshIngredientAvailabilityVis();
		if (toggle.isOn)
		{
			recipeScreen = (SelectedRecipeQueueScreen)DetailsScreen.Instance.SetSecondarySideScreen(recipeScreenPrefab, targetFab.SideScreenRecipeScreenTitle);
			recipeScreen.SetRecipeCategory(this, targetFab, selectedRecipeCategory);
		}
		else
		{
			DetailsScreen.Instance.ClearSecondarySideScreen();
		}
	}

	public void CycleRecipe(int increment)
	{
		int num = 0;
		if (selectedToggle != null)
		{
			num = recipeToggles.IndexOf(selectedToggle.gameObject);
		}
		int num2 = (num + increment) % recipeToggles.Count;
		if (num2 < 0)
		{
			num2 = recipeToggles.Count + num2;
		}
		ToggleClicked(recipeToggles[num2].GetComponent<KToggle>());
	}

	private bool HasAnyRecipeRequirements(ComplexRecipe recipe)
	{
		ComplexRecipe.RecipeElement[] ingredients = recipe.ingredients;
		foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
		{
			if (targetFab.GetMyWorld().worldInventory.GetAmountWithoutTag(recipeElement.material, includeRelatedWorlds: true, targetFab.ForbiddenTags) + targetFab.inStorage.GetAmountAvailable(recipeElement.material, targetFab.ForbiddenTags) + targetFab.buildStorage.GetAmountAvailable(recipeElement.material, targetFab.ForbiddenTags) >= recipeElement.amount)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasAllRecipeRequirements(ComplexRecipe recipe)
	{
		bool result = true;
		ComplexRecipe.RecipeElement[] ingredients = recipe.ingredients;
		foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
		{
			if (targetFab.GetMyWorld().worldInventory.GetAmountWithoutTag(recipeElement.material, includeRelatedWorlds: true, targetFab.ForbiddenTags) + targetFab.inStorage.GetAmountAvailable(recipeElement.material, targetFab.ForbiddenTags) + targetFab.buildStorage.GetAmountAvailable(recipeElement.material, targetFab.ForbiddenTags) < recipeElement.amount)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private bool AnyRecipeRequirementsDiscovered(ComplexRecipe recipe)
	{
		ComplexRecipe.RecipeElement[] ingredients = recipe.ingredients;
		foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
		{
			if (DiscoveredResources.Instance.IsDiscovered(recipeElement.material))
			{
				return true;
			}
		}
		return false;
	}

	private bool AllRecipeRequirementsDiscovered(ComplexRecipe recipe)
	{
		ComplexRecipe.RecipeElement[] ingredients = recipe.ingredients;
		foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
		{
			if (!DiscoveredResources.Instance.IsDiscovered(recipeElement.material))
			{
				return false;
			}
		}
		return true;
	}

	private void Update()
	{
		RefreshIngredientAvailabilityVis();
	}

	private void UnsubscribeTarget()
	{
		if (targetOrdersUpdatedSubHandle != -1 && targetFab != null)
		{
			targetFab.Unsubscribe(targetOrdersUpdatedSubHandle);
			targetOrdersUpdatedSubHandle = -1;
		}
	}

	public override void ClearTarget()
	{
		base.ClearTarget();
		UnsubscribeTarget();
	}

	private void RefreshIngredientAvailabilityVis()
	{
		foreach (KeyValuePair<GameObject, List<ComplexRecipe>> item in recipeCategoryToggleMap)
		{
			HierarchyReferences component = item.Key.GetComponent<HierarchyReferences>();
			bool flag = item.Value.Find((ComplexRecipe match) => HasAllRecipeRequirements(match)) != null;
			KToggle component2 = item.Key.GetComponent<KToggle>();
			if (flag)
			{
				if (item.Value[0].recipeCategoryID == selectedRecipeCategory)
				{
					component2.ActivateFlourish(state: true, ImageToggleState.State.Active);
				}
				else
				{
					component2.ActivateFlourish(state: false, ImageToggleState.State.Inactive);
				}
			}
			else if (item.Value[0].recipeCategoryID == selectedRecipeCategory)
			{
				component2.ActivateFlourish(state: true, ImageToggleState.State.DisabledActive);
			}
			else
			{
				component2.ActivateFlourish(state: false, ImageToggleState.State.Disabled);
			}
			component.GetReference<LocText>("Label").color = (flag ? Color.black : new Color(0.22f, 0.22f, 0.22f, 1f));
		}
	}
}
