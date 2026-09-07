using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using STRINGS;
using UnityEngine;
using UnityEngine.UI;

public class SelectedRecipeQueueScreen : KScreen
{
	private class DescriptorWithSprite
	{
		public bool showFilterRow;

		public Descriptor descriptor { get; }

		public Tuple<Sprite, Color> tintedSprite { get; }

		public DescriptorWithSprite(Descriptor desc, Tuple<Sprite, Color> sprite, bool filterRowVisible = false)
		{
			descriptor = desc;
			tintedSprite = sprite;
			showFilterRow = filterRowVisible;
		}
	}

	public Image recipeIcon;

	public LocText recipeName;

	public LocText recipeMainDescription;

	public LocText recipeDuration;

	public ToolTip recipeDurationTooltip;

	public GameObject IngredientsDescriptorPanel;

	public GameObject radboltSpacer;

	public GameObject radboltHeader;

	public GameObject RadboltDescriptorPanel;

	public LocText radboltLabel;

	public GameObject EffectsDescriptorPanel;

	public KNumberInputField QueueCount;

	public MultiToggle DecrementButton;

	public MultiToggle IncrementButton;

	public KButton InfiniteButton;

	public GameObject InfiniteIcon;

	public GameObject ResearchRequiredContainer;

	public GameObject UndiscoveredMaterialsContainer;

	[SerializeField]
	private GameObject materialFilterRowPrefab;

	[SerializeField]
	private GameObject materialSelectionContainerPrefab;

	private List<GameObject> materialSelectionContainers = new List<GameObject>();

	private Dictionary<GameObject, List<GameObject>> materialSelectionRowsByContainer = new Dictionary<GameObject, List<GameObject>>();

	private ComplexFabricator target;

	private ComplexFabricatorSideScreen ownerScreen;

	private List<Tag> selectedMaterialOption = new List<Tag>();

	private string selectedRecipeCategoryID;

	[SerializeField]
	private GameObject recipeElementDescriptorPrefab;

	private Dictionary<DescriptorWithSprite, GameObject> recipeIngredientDescriptorRows = new Dictionary<DescriptorWithSprite, GameObject>();

	private Dictionary<DescriptorWithSprite, GameObject> recipeEffectsDescriptorRows = new Dictionary<DescriptorWithSprite, GameObject>();

	[SerializeField]
	private FullBodyUIMinionWidget minionWidget;

	[SerializeField]
	private MultiToggle previousRecipeButton;

	[SerializeField]
	private MultiToggle nextRecipeButton;

	[SerializeField]
	private LayoutElement scrollContainer;

	private int cycleRecipeVariantIdx;

	private ComplexRecipe selectedRecipe => CalculateSelectedRecipe();

	private List<ComplexRecipe> selectedRecipes => target.GetRecipesWithCategoryID(selectedRecipeCategoryID);

	private ComplexRecipe firstSelectedRecipe => selectedRecipes[0];

	protected override void OnSpawn()
	{
		base.OnSpawn();
		DecrementButton.onClick = delegate
		{
			if (selectedRecipe != null)
			{
				target.DecrementRecipeQueueCount(selectedRecipe, respectInfinite: false);
				RefreshIngredientDescriptors();
				RefreshQueueCountDisplay();
				ownerScreen.RefreshQueueCountDisplayForRecipeCategory(selectedRecipeCategoryID, target);
			}
		};
		IncrementButton.onClick = delegate
		{
			if (selectedRecipe != null)
			{
				target.IncrementRecipeQueueCount(selectedRecipe);
				RefreshIngredientDescriptors();
				RefreshQueueCountDisplay();
				ownerScreen.RefreshQueueCountDisplayForRecipeCategory(selectedRecipeCategoryID, target);
			}
		};
		InfiniteButton.GetComponentInChildren<LocText>().text = UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_FOREVER;
		InfiniteButton.onClick += delegate
		{
			if (selectedRecipe != null)
			{
				if (target.GetRecipeQueueCount(selectedRecipe) != ComplexFabricator.QUEUE_INFINITE)
				{
					target.SetRecipeQueueCount(selectedRecipe, ComplexFabricator.QUEUE_INFINITE);
				}
				else
				{
					target.SetRecipeQueueCount(selectedRecipe, 0);
				}
				RefreshQueueCountDisplay();
				ownerScreen.RefreshQueueCountDisplayForRecipeCategory(selectedRecipeCategoryID, target);
			}
		};
		QueueCount.onEndEdit += delegate
		{
			base.isEditing = false;
			if (selectedRecipe != null)
			{
				target.SetRecipeQueueCount(selectedRecipe, Mathf.RoundToInt(QueueCount.currentValue));
				RefreshIngredientDescriptors();
				RefreshQueueCountDisplay();
				ownerScreen.RefreshQueueCountDisplayForRecipeCategory(selectedRecipeCategoryID, target);
			}
		};
		QueueCount.onStartEdit += delegate
		{
			base.isEditing = true;
			KScreenManager.Instance.RefreshStack();
		};
		MultiToggle multiToggle = previousRecipeButton;
		multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, new System.Action(CyclePreviousRecipe));
		MultiToggle multiToggle2 = nextRecipeButton;
		multiToggle2.onClick = (System.Action)Delegate.Combine(multiToggle2.onClick, new System.Action(CycleNextRecipe));
	}

	protected override void OnCmpDisable()
	{
		base.OnCmpDisable();
		if (firstSelectedRecipe != null)
		{
			GameObject prefab = Assets.GetPrefab(firstSelectedRecipe.results[0].material);
			Equippable equippable = ((prefab != null) ? prefab.GetComponent<Equippable>() : null);
			if (equippable != null && equippable.GetBuildOverride() != null)
			{
				minionWidget.RemoveEquipment(equippable);
			}
		}
	}

	private void AutoSelectBestRecipeInCategory()
	{
		int num = -1;
		List<ComplexRecipe> list = new List<ComplexRecipe>();
		selectedMaterialOption.Clear();
		ComplexRecipe complexRecipe = null;
		if (target.mostRecentRecipeSelectionByCategory.ContainsKey(selectedRecipeCategoryID))
		{
			complexRecipe = target.GetRecipe(target.mostRecentRecipeSelectionByCategory[selectedRecipeCategoryID]);
		}
		if (complexRecipe != null)
		{
			ComplexRecipe.RecipeElement[] ingredients = complexRecipe.ingredients;
			foreach (ComplexRecipe.RecipeElement recipeElement in ingredients)
			{
				selectedMaterialOption.Add(recipeElement.material);
			}
		}
		else
		{
			foreach (ComplexRecipe selectedRecipe in selectedRecipes)
			{
				int num2 = target.GetRecipeQueueCount(selectedRecipe);
				if (num2 == ComplexFabricator.QUEUE_INFINITE)
				{
					num2 = int.MaxValue;
				}
				if (num2 >= num)
				{
					if (num2 > num)
					{
						list.Clear();
						num = num2;
					}
					list.Add(selectedRecipe);
				}
			}
			int num3 = list[0].ingredients.Length;
			Tag[] array = new Tag[num3];
			for (int j = 0; j < num3; j++)
			{
				float num4 = -1f;
				foreach (ComplexRecipe item in list)
				{
					float amount = target.GetMyWorld().worldInventory.GetAmount(item.ingredients[j].material, includeRelatedWorlds: true);
					if (amount > num4)
					{
						array[j] = item.ingredients[j].material;
						num4 = amount;
					}
				}
			}
			selectedMaterialOption.AddRange(array);
		}
		RefreshIngredientDescriptors();
		RefreshQueueCountDisplay();
	}

	public bool IsSelectedMaterials(ComplexRecipe recipe)
	{
		if (selectedRecipeCategoryID != recipe.recipeCategoryID)
		{
			return false;
		}
		for (int i = 0; i < recipe.ingredients.Length; i++)
		{
			if (recipe.ingredients[i].material != selectedMaterialOption[i])
			{
				return false;
			}
		}
		return true;
	}

	public void SelectNextQueuedRecipeInCategory()
	{
		cycleRecipeVariantIdx++;
		selectedMaterialOption.Clear();
		List<ComplexRecipe> list = selectedRecipes.Where((ComplexRecipe match) => target.IsRecipeQueued(match)).ToList();
		if (list.Count == 0)
		{
			AutoSelectBestRecipeInCategory();
			return;
		}
		ComplexRecipe complexRecipe = list[cycleRecipeVariantIdx % list.Count];
		for (int i = 0; i < complexRecipe.ingredients.Length; i++)
		{
			selectedMaterialOption.Add(complexRecipe.ingredients[i].material);
		}
		RefreshIngredientDescriptors();
		RefreshQueueCountDisplay();
	}

	public void SetRecipeCategory(ComplexFabricatorSideScreen owner, ComplexFabricator target, string recipeCategoryID)
	{
		ownerScreen = owner;
		this.target = target;
		selectedRecipeCategoryID = recipeCategoryID;
		AutoSelectBestRecipeInCategory();
		recipeName.text = firstSelectedRecipe.GetUIName(includeAmounts: false);
		Tuple<Sprite, Color> tuple = ((firstSelectedRecipe.nameDisplay == ComplexRecipe.RecipeNameDisplay.Ingredient) ? Def.GetUISprite(firstSelectedRecipe.ingredients[0].material) : ((firstSelectedRecipe.nameDisplay != ComplexRecipe.RecipeNameDisplay.Custom || string.IsNullOrEmpty(firstSelectedRecipe.customSpritePrefabID)) ? Def.GetUISprite(firstSelectedRecipe.results[0].material, firstSelectedRecipe.results[0].facadeID) : Def.GetUISprite(firstSelectedRecipe.customSpritePrefabID)));
		if (firstSelectedRecipe.nameDisplay == ComplexRecipe.RecipeNameDisplay.HEP)
		{
			recipeIcon.sprite = owner.radboltSprite;
			recipeIcon.sprite = owner.radboltSprite;
		}
		else
		{
			recipeIcon.sprite = tuple.first;
			recipeIcon.color = tuple.second;
		}
		string text = (firstSelectedRecipe.time + " " + UI.UNITSUFFIXES.SECONDS).ToLower();
		recipeMainDescription.SetText(firstSelectedRecipe.description);
		recipeDuration.SetText(text);
		string simpleTooltip = string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.TOOLTIPS.RECIPE_WORKTIME, text);
		recipeDurationTooltip.SetSimpleTooltip(simpleTooltip);
		cycleRecipeVariantIdx = 0;
		RefreshIngredientDescriptors();
		RefreshResultDescriptors();
		RefreshSizeScrollContainerSize();
		RefreshQueueCountDisplay();
		ToggleAndRefreshMinionDisplay();
	}

	private void RefreshSizeScrollContainerSize()
	{
		float num = 16f;
		float num2 = 0f;
		float num3 = ((selectedRecipe.consumedHEP > 0) ? 94 : 0);
		num2 += (float)(materialSelectionRowsByContainer.Count * 32);
		foreach (KeyValuePair<GameObject, List<GameObject>> item in materialSelectionRowsByContainer)
		{
			num2 += (float)(Mathf.Max(1, item.Value.Count) * 48);
		}
		num2 += (float)((materialSelectionRowsByContainer.Count - 1) * 12);
		float num4 = Mathf.Max(selectedRecipes[0].results.Length * 32 + (recipeEffectsDescriptorRows.Count - selectedRecipes[0].results.Length) * 16, 40);
		num4 += 46f;
		float b = num + num2 + num3 + num4;
		scrollContainer.minHeight = Mathf.Min(Screen.height - 448, b);
	}

	private void CyclePreviousRecipe()
	{
		ownerScreen.CycleRecipe(-1);
	}

	private void CycleNextRecipe()
	{
		ownerScreen.CycleRecipe(1);
	}

	private void ToggleAndRefreshMinionDisplay()
	{
		minionWidget.gameObject.SetActive(RefreshMinionDisplayAnim());
	}

	private bool RefreshMinionDisplayAnim()
	{
		GameObject prefab = Assets.GetPrefab(firstSelectedRecipe.results[0].material);
		if (prefab == null)
		{
			return false;
		}
		Equippable component = prefab.GetComponent<Equippable>();
		if (component == null)
		{
			return false;
		}
		KAnimFile buildOverride = component.GetBuildOverride();
		if (buildOverride == null)
		{
			return false;
		}
		minionWidget.SetDefaultPortraitAnimator();
		KAnimFile animFile = buildOverride;
		if (!firstSelectedRecipe.results[0].facadeID.IsNullOrWhiteSpace())
		{
			EquippableFacadeResource equippableFacadeResource = Db.GetEquippableFacades().TryGet(firstSelectedRecipe.results[0].facadeID);
			if (equippableFacadeResource != null)
			{
				animFile = Assets.GetAnim(equippableFacadeResource.BuildOverride);
			}
		}
		minionWidget.UpdateEquipment(component, animFile);
		return true;
	}

	private ComplexRecipe CalculateSelectedRecipe()
	{
		foreach (ComplexRecipe item in target.GetRecipesWithCategoryID(selectedRecipeCategoryID))
		{
			bool flag = true;
			for (int i = 0; i < selectedMaterialOption.Count; i++)
			{
				if (item.ingredients[i].material != selectedMaterialOption[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return item;
			}
		}
		return null;
	}

	private void RefreshQueueCountDisplay()
	{
		ResearchRequiredContainer.SetActive(!selectedRecipes[0].IsRequiredTechOrPOIUnlocked());
		if (selectedRecipe == null)
		{
			return;
		}
		bool flag = true;
		foreach (Tag item in selectedMaterialOption)
		{
			if (!DiscoveredResources.Instance.IsDiscovered(item))
			{
				flag = DebugHandler.InstantBuildMode;
			}
		}
		UndiscoveredMaterialsContainer.SetActive(!flag);
		int recipeQueueCount = target.GetRecipeQueueCount(selectedRecipe);
		bool flag2 = recipeQueueCount == ComplexFabricator.QUEUE_INFINITE;
		if (!flag2)
		{
			QueueCount.SetAmount(recipeQueueCount);
		}
		else
		{
			QueueCount.SetDisplayValue("");
		}
		InfiniteIcon.gameObject.SetActive(flag2);
	}

	private void RefreshResultDescriptors()
	{
		List<DescriptorWithSprite> list = new List<DescriptorWithSprite>();
		list.AddRange(GetResultDescriptions(selectedRecipes[0]));
		foreach (Descriptor item in target.AdditionalEffectsForRecipe(selectedRecipes[0]))
		{
			list.Add(new DescriptorWithSprite(item, null));
		}
		if (list.Count <= 0)
		{
			return;
		}
		EffectsDescriptorPanel.gameObject.SetActive(value: true);
		foreach (KeyValuePair<DescriptorWithSprite, GameObject> recipeEffectsDescriptorRow in recipeEffectsDescriptorRows)
		{
			Util.KDestroyGameObject(recipeEffectsDescriptorRow.Value);
		}
		recipeEffectsDescriptorRows.Clear();
		bool flag = true;
		foreach (DescriptorWithSprite item2 in list)
		{
			GameObject gameObject = Util.KInstantiateUI(recipeElementDescriptorPrefab, EffectsDescriptorPanel.gameObject, force_active: true);
			HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
			Image reference = component.GetReference<Image>("Icon");
			bool flag2 = item2.tintedSprite != null && item2.tintedSprite.first != null;
			reference.sprite = ((item2.tintedSprite == null) ? null : item2.tintedSprite.first);
			reference.gameObject.SetActive(value: true);
			if (!flag2)
			{
				reference.color = Color.clear;
				if (flag)
				{
					gameObject.GetComponent<VerticalLayoutGroup>().padding.top = -8;
					flag = false;
				}
			}
			else
			{
				reference.color = ((item2.tintedSprite == null) ? Color.white : item2.tintedSprite.second);
				flag = true;
			}
			reference.gameObject.GetComponent<LayoutElement>().minWidth = (flag2 ? 32 : 40);
			reference.gameObject.GetComponent<LayoutElement>().minHeight = (flag2 ? 32 : 0);
			reference.gameObject.GetComponent<LayoutElement>().preferredHeight = (flag2 ? 32 : 0);
			component.GetReference<LocText>("Label").SetText(flag2 ? item2.descriptor.IndentedText() : item2.descriptor.text);
			component.GetReference<RectTransform>("FilterControls").gameObject.SetActive(value: false);
			component.GetReference<ToolTip>("Tooltip").SetSimpleTooltip(item2.descriptor.tooltipText);
			recipeEffectsDescriptorRows.Add(item2, gameObject);
		}
	}

	private List<DescriptorWithSprite> GetResultDescriptions(ComplexRecipe recipe)
	{
		List<DescriptorWithSprite> list = new List<DescriptorWithSprite>();
		if (recipe.producedHEP > 0)
		{
			list.Add(new DescriptorWithSprite(new Descriptor(string.Format("<b>{0}</b>: {1}", UI.FormatAsLink(ITEMS.RADIATION.HIGHENERGYPARITCLE.NAME, "HEP"), recipe.producedHEP), $"<b>{ITEMS.RADIATION.HIGHENERGYPARITCLE.NAME}</b>: {recipe.producedHEP}", Descriptor.DescriptorType.Requirement), new Tuple<Sprite, Color>(Assets.GetSprite("radbolt"), Color.white)));
		}
		ComplexRecipe.RecipeElement[] results = recipe.results;
		foreach (ComplexRecipe.RecipeElement recipeElement in results)
		{
			GameObject prefab = Assets.GetPrefab(recipeElement.material);
			string formattedByTag = GameUtil.GetFormattedByTag(recipeElement.material, recipeElement.amount);
			list.Add(new DescriptorWithSprite(new Descriptor(string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPEPRODUCT, recipeElement.facadeID.IsNullOrWhiteSpace() ? recipeElement.material.ProperName() : GameTagExtensions.ProperName(recipeElement.facadeID), formattedByTag), string.Format(UI.UISIDESCREENS.FABRICATORSIDESCREEN.TOOLTIPS.RECIPEPRODUCT, recipeElement.facadeID.IsNullOrWhiteSpace() ? recipeElement.material.ProperName() : GameTagExtensions.ProperName(recipeElement.facadeID), formattedByTag), Descriptor.DescriptorType.Requirement), Def.GetUISprite(recipeElement.material, recipeElement.facadeID)));
			Element element = ElementLoader.GetElement(recipeElement.material);
			if (element != null)
			{
				List<DescriptorWithSprite> list2 = new List<DescriptorWithSprite>();
				foreach (Descriptor materialDescriptor in GameUtil.GetMaterialDescriptors(element))
				{
					list2.Add(new DescriptorWithSprite(materialDescriptor, null));
				}
				foreach (DescriptorWithSprite item in list2)
				{
					item.descriptor.IncreaseIndent();
				}
				list.AddRange(list2);
				continue;
			}
			List<DescriptorWithSprite> list3 = new List<DescriptorWithSprite>();
			foreach (Descriptor effectDescriptor in GameUtil.GetEffectDescriptors(GameUtil.GetAllDescriptors(prefab)))
			{
				list3.Add(new DescriptorWithSprite(effectDescriptor, null));
			}
			foreach (DescriptorWithSprite item2 in list3)
			{
				item2.descriptor.IncreaseIndent();
			}
			list.AddRange(list3);
		}
		return list;
	}

	private void RefreshIngredientDescriptors()
	{
		new List<DescriptorWithSprite>();
		IngredientsDescriptorPanel.gameObject.SetActive(value: true);
		radboltSpacer.gameObject.SetActive(selectedRecipe.consumedHEP > 0);
		radboltHeader.gameObject.SetActive(selectedRecipe.consumedHEP > 0);
		RadboltDescriptorPanel.gameObject.SetActive(selectedRecipe.consumedHEP > 0);
		radboltLabel.SetText(GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_RADBOLTS_REQUIRED, selectedRecipe.consumedHEP.ToString()));
		materialSelectionContainers.ForEach(delegate(GameObject container)
		{
			Util.KDestroyGameObject(container);
		});
		materialSelectionContainers.Clear();
		materialSelectionRowsByContainer.Clear();
		for (int i = 0; i < selectedRecipes[0].ingredients.Length; i++)
		{
			GameObject gameObject = Util.KInstantiateUI(materialSelectionContainerPrefab, IngredientsDescriptorPanel.gameObject, force_active: true);
			materialSelectionContainers.Add(gameObject);
			materialSelectionRowsByContainer.Add(materialSelectionContainers[i], new List<GameObject>());
			HierarchyReferences component = gameObject.GetComponent<HierarchyReferences>();
			int idx = i;
			List<Tag> list = new List<Tag>();
			bool flag = false;
			HashSet<Tag> hashSet = new HashSet<Tag>();
			for (int j = 0; j < selectedRecipes.Count; j++)
			{
				Tag newTag = selectedRecipes[j].ingredients[idx].material;
				if (list.Contains(newTag))
				{
					continue;
				}
				bool num = DiscoveredResources.Instance.IsDiscovered(newTag);
				if (!num)
				{
					hashSet.Add(newTag);
				}
				if (num || DebugHandler.InstantBuildMode)
				{
					flag = true;
					GameObject gameObject2 = Util.KInstantiateUI(materialFilterRowPrefab, materialSelectionContainers[idx].gameObject, force_active: true);
					materialSelectionRowsByContainer[materialSelectionContainers[idx]].Add(gameObject2);
					list.Add(newTag);
					LocText reference = gameObject2.GetComponent<HierarchyReferences>().GetReference<LocText>("Label");
					bool hasEnoughMaterial = false;
					string ingredientDescription = GetIngredientDescription(selectedRecipes[j].ingredients[idx], out hasEnoughMaterial);
					bool flag2 = selectedMaterialOption[i] == selectedRecipes[j].ingredients[i].material;
					if (flag2)
					{
						component.GetReference<Image>("HeaderBG").color = (hasEnoughMaterial ? Util.ColorFromHex("D9DAE3") : Util.ColorFromHex("E3DAD9"));
					}
					reference.color = (hasEnoughMaterial ? Color.black : new Color(0.2f, 0.2f, 0.2f, 1f));
					HierarchyReferences component2 = gameObject2.GetComponent<HierarchyReferences>();
					component2.GetReference<RectTransform>("SelectionHover").gameObject.SetActive(flag2);
					component2.GetReference<RectTransform>("SelectionHover").GetComponent<Image>().color = (hasEnoughMaterial ? Util.ColorFromHex("F0F6FC") : Util.ColorFromHex("FBE9EB"));
					component2.GetReference<LocText>("OrderCountLabel").SetText(target.GetIngredientQueueCount(selectedRecipeCategoryID, newTag).ToString());
					Image reference2 = component2.GetReference<Image>("Icon");
					reference2.material = ((!hasEnoughMaterial) ? GlobalResources.Instance().AnimMaterialUIDesaturated : GlobalResources.Instance().AnimUIMaterial);
					reference2.color = (hasEnoughMaterial ? Color.white : new Color(1f, 1f, 1f, 0.55f));
					reference.SetText(ingredientDescription);
					reference2.sprite = Def.GetUISprite(newTag, "").first;
					MultiToggle component3 = gameObject2.GetComponent<MultiToggle>();
					component3.ChangeState(flag2 ? 1 : 0);
					component3.onClick = (System.Action)Delegate.Combine(component3.onClick, (System.Action)delegate
					{
						Tag value = newTag;
						selectedMaterialOption[idx] = value;
						RefreshIngredientDescriptors();
						RefreshQueueCountDisplay();
						ownerScreen.RefreshQueueCountDisplayForRecipeCategory(selectedRecipeCategoryID, target);
					});
				}
			}
			ToolTip reference3 = component.GetReference<ToolTip>("HeaderTooltip");
			string text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.UNDISCOVERED_INGREDIENTS_IN_CATEGORY, "    • " + string.Join("\n    • ", hashSet.Select((Tag t) => t.ProperName()).ToArray()));
			reference3.SetSimpleTooltip((hashSet.Count == 0) ? ((string)UI.UISIDESCREENS.FABRICATORSIDESCREEN.ALL_INGREDIENTS_IN_CATEGORY_DISOVERED) : text);
			RectTransform reference4 = component.GetReference<RectTransform>("NoDiscoveredRow");
			reference4.gameObject.SetActive(!flag);
			if (!flag)
			{
				reference4.GetComponent<ToolTip>().SetSimpleTooltip(text);
			}
			string text2 = GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.INGREDIENT_CATEGORY, i + 1);
			if (!flag)
			{
				component.GetReference<Image>("HeaderBG").color = Util.ColorFromHex("E3DAD9");
			}
			if (hashSet.Count > 0)
			{
				text2 = text2 + " <color=#bf5858>(" + list.Count + "/" + (list.Count + hashSet.Count) + ")" + UIConstants.ColorSuffix;
			}
			component.GetReference<LocText>("HeaderLabel").SetText(text2);
		}
		if (!target.mostRecentRecipeSelectionByCategory.ContainsKey(selectedRecipeCategoryID))
		{
			target.mostRecentRecipeSelectionByCategory.Add(selectedRecipeCategoryID, null);
		}
		target.mostRecentRecipeSelectionByCategory[selectedRecipeCategoryID] = selectedRecipe.id;
	}

	private string GetIngredientDescription(ComplexRecipe.RecipeElement ingredient, out bool hasEnoughMaterial)
	{
		GameObject prefab = Assets.GetPrefab(ingredient.material);
		string formattedByTag = GameUtil.GetFormattedByTag(ingredient.material, ingredient.amount);
		float amount = target.GetMyWorld().worldInventory.GetAmount(ingredient.material, includeRelatedWorlds: true);
		string formattedByTag2 = GameUtil.GetFormattedByTag(ingredient.material, amount);
		hasEnoughMaterial = amount >= ingredient.amount;
		string text = GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_REQUIREMENT, prefab.GetProperName(), formattedByTag);
		text += "\n";
		if (hasEnoughMaterial)
		{
			return text + "<size=12>" + GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_AVAILABLE, formattedByTag2) + "</size>";
		}
		return text + "<size=12><color=#E68280>" + GameUtil.SafeStringFormat(UI.UISIDESCREENS.FABRICATORSIDESCREEN.RECIPE_AVAILABLE, formattedByTag2) + "</color></size>";
	}
}
