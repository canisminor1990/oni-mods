using System;
using System.Collections.Generic;
using System.Linq;
using KMod;
using STRINGS;
using UnityEngine;

public class ModsScreen : KModalScreen
{
	private struct DisplayedMod
	{
		public RectTransform rect_transform;

		public int mod_index;
	}

	private class ModOrderingDragListener : DragMe.IDragListener
	{
		private List<DisplayedMod> mods;

		private ModsScreen screen;

		private int startDragIdx = -1;

		public ModOrderingDragListener(ModsScreen screen, List<DisplayedMod> mods)
		{
			this.screen = screen;
			this.mods = mods;
		}

		public void OnBeginDrag(Vector2 pos)
		{
			startDragIdx = GetDragIdx(pos, halfPosition: false);
		}

		public void OnEndDrag(Vector2 pos)
		{
			if (startDragIdx >= 0)
			{
				int dragIdx = GetDragIdx(pos, halfPosition: true);
				if (dragIdx != startDragIdx)
				{
					int mod_index = mods[startDragIdx].mod_index;
					int target_index = ((0 <= dragIdx && dragIdx < mods.Count) ? mods[dragIdx].mod_index : (-1));
					Global.Instance.modManager.Reinsert(mod_index, target_index, dragIdx >= mods.Count, this);
					screen.BuildDisplay();
				}
			}
		}

		private int GetDragIdx(Vector2 pos, bool halfPosition)
		{
			int result = -1;
			for (int i = 0; i < mods.Count; i++)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(mods[i].rect_transform, pos, null, out var localPoint);
				if (!halfPosition)
				{
					localPoint += mods[i].rect_transform.rect.min;
				}
				if (!(localPoint.y < 0f))
				{
					break;
				}
				result = i;
			}
			return result;
		}
	}

	[SerializeField]
	private KButton closeButtonTitle;

	[SerializeField]
	private KButton closeButton;

	[SerializeField]
	private KButton toggleAllButton;

	[SerializeField]
	private KButton workshopButton;

	[SerializeField]
	private GameObject entryPrefab;

	[SerializeField]
	private Transform entryParent;

	[SerializeField]
	private LocText windowTitle;

	private List<DisplayedMod> displayedMods = new List<DisplayedMod>();

	private List<Label> mod_footprint = new List<Label>();

	protected override void OnActivate()
	{
		base.OnActivate();
		if (Global.Instance.modManager.safe_mode_enabled)
		{
			windowTitle.text = UI.FRONTEND.MODS.TITLE_SAFE_MODE;
			windowTitle.ApplySettings();
			Global.Instance.modManager.ShowSafeModeDialog(base.gameObject);
		}
		else
		{
			windowTitle.text = UI.FRONTEND.MODS.TITLE;
			windowTitle.ApplySettings();
		}
		closeButtonTitle.onClick += Exit;
		closeButton.onClick += Exit;
		System.Action value = delegate
		{
			App.OpenWebURL("http://steamcommunity.com/workshop/browse/?appid=457140");
		};
		workshopButton.onClick += value;
		UpdateToggleAllButton();
		toggleAllButton.onClick += OnToggleAllClicked;
		Global.Instance.modManager.Sanitize(base.gameObject);
		mod_footprint.Clear();
		foreach (Mod mod in Global.Instance.modManager.mods)
		{
			if (mod.IsEnabledForActiveDlc())
			{
				mod_footprint.Add(mod.label);
				if ((mod.loaded_content & (Content.LayerableFiles | Content.Strings | Content.DLL | Content.Translation | Content.Animation)) == (mod.available_content & (Content.LayerableFiles | Content.Strings | Content.DLL | Content.Translation | Content.Animation)))
				{
					mod.Uncrash();
				}
			}
		}
		BuildDisplay();
		Manager modManager = Global.Instance.modManager;
		modManager.on_update = (Manager.OnUpdate)Delegate.Combine(modManager.on_update, new Manager.OnUpdate(RebuildDisplay));
	}

	protected override void OnDeactivate()
	{
		Manager modManager = Global.Instance.modManager;
		modManager.on_update = (Manager.OnUpdate)Delegate.Remove(modManager.on_update, new Manager.OnUpdate(RebuildDisplay));
		base.OnDeactivate();
	}

	private void Exit()
	{
		Global.Instance.modManager.Save();
		if (!Global.Instance.modManager.MatchFootprint(mod_footprint, Content.LayerableFiles | Content.Strings | Content.DLL | Content.Translation | Content.Animation))
		{
			Global.Instance.modManager.RestartDialog(UI.FRONTEND.MOD_DIALOGS.MODS_SCREEN_CHANGES.TITLE, UI.FRONTEND.MOD_DIALOGS.MODS_SCREEN_CHANGES.MESSAGE, Deactivate, with_details: true, base.gameObject);
		}
		else
		{
			Deactivate();
		}
		Global.Instance.modManager.events.Clear();
	}

	private void RebuildDisplay(object change_source)
	{
		if (change_source != this)
		{
			BuildDisplay();
		}
	}

	private bool ShouldDisplayMod(Mod mod)
	{
		if (mod.status != 0 && mod.status != Mod.Status.UninstallPending)
		{
			return !mod.HasOnlyTranslationContent();
		}
		return false;
	}

	private void BuildDisplay()
	{
		foreach (DisplayedMod displayedMod in displayedMods)
		{
			if (displayedMod.rect_transform != null)
			{
				UnityEngine.Object.Destroy(displayedMod.rect_transform.gameObject);
			}
		}
		displayedMods.Clear();
		ModOrderingDragListener listener = new ModOrderingDragListener(this, displayedMods);
		for (int i = 0; i != Global.Instance.modManager.mods.Count; i++)
		{
			Mod mod = Global.Instance.modManager.mods[i];
			if (!ShouldDisplayMod(mod))
			{
				continue;
			}
			HierarchyReferences hierarchyReferences = Util.KInstantiateUI<HierarchyReferences>(entryPrefab, entryParent.gameObject);
			displayedMods.Add(new DisplayedMod
			{
				rect_transform = hierarchyReferences.gameObject.GetComponent<RectTransform>(),
				mod_index = i
			});
			hierarchyReferences.GetComponent<DragMe>().listener = listener;
			LocText reference = hierarchyReferences.GetReference<LocText>("Title");
			string text = mod.title;
			if (Strings.TryGet(mod.title, out var result))
			{
				text = result;
			}
			hierarchyReferences.name = mod.title;
			ToolTip reference2 = hierarchyReferences.GetReference<ToolTip>("Description");
			if (mod.available_content == (Content)0)
			{
				switch (mod.contentCompatability)
				{
				case ModContentCompatability.OldAPI:
					text += UI.FRONTEND.MODS.CONTENT_FAILURE.OLD_API;
					reference2.toolTip = UI.FRONTEND.MODS.CONTENT_FAILURE.OLD_API_TOOLTIP;
					break;
				case ModContentCompatability.NoContent:
					text += UI.FRONTEND.MODS.CONTENT_FAILURE.NO_CONTENT;
					reference2.toolTip = UI.FRONTEND.MODS.CONTENT_FAILURE.NO_CONTENT_TOOLTIP;
					break;
				default:
				{
					string text2 = GlobalAssets.Instance.colorSet.GetColorByName("statusItemBad").ToHexString();
					string text3 = string.Concat(UI.FRONTEND.MODS.CONTENT_FAILURE.DISABLED_CONTENT_TOOLTIP, "\n\n");
					if (mod.GetRequiredDlcIds() != null)
					{
						text3 += UI.FRONTEND.MODS.CONTENT_FAILURE.DISABLED_CONTENT_TOOLTIP_REQUIRED;
						string[] requiredDlcIds = mod.GetRequiredDlcIds();
						foreach (string dlcId in requiredDlcIds)
						{
							text3 = ((!DlcManager.IsContentSubscribed(dlcId)) ? (text3 + "\n     •  <i><color=#" + text2 + ">" + DlcManager.GetDlcTitleNoFormatting(dlcId) + "</color></i>") : (text3 + "\n     •  <i>" + DlcManager.GetDlcTitleNoFormatting(dlcId) + "</i>"));
						}
						if (mod.GetForbiddenDlcIds() != null)
						{
							text3 += "\n\n";
						}
					}
					if (mod.GetForbiddenDlcIds() != null)
					{
						text3 += UI.FRONTEND.MODS.CONTENT_FAILURE.DISABLED_CONTENT_TOOLTIP_FORBIDDEN_DLC;
						string[] requiredDlcIds = mod.GetForbiddenDlcIds();
						foreach (string dlcId2 in requiredDlcIds)
						{
							text3 = (DlcManager.IsContentSubscribed(dlcId2) ? (text3 + "\n     •  <i><color=#" + text2 + ">" + DlcManager.GetDlcTitleNoFormatting(dlcId2) + "</color></i>") : (text3 + "\n     •  <i>" + DlcManager.GetDlcTitleNoFormatting(dlcId2) + "</i>"));
						}
					}
					reference2.toolTip = text3;
					text += UI.FRONTEND.MODS.CONTENT_FAILURE.DISABLED_CONTENT;
					break;
				}
				}
			}
			reference.text = text;
			LocText reference3 = hierarchyReferences.GetReference<LocText>("Version");
			if (mod.packagedModInfo != null && mod.packagedModInfo.version != null && mod.packagedModInfo.version.Length > 0)
			{
				string text4;
				if (mod.status == Mod.Status.ReinstallPending)
				{
					text4 = UI.FRONTEND.MODS.INSTALLED_VERSION_NEWER_THAN_LOADED_VERSION;
				}
				else
				{
					text4 = mod.packagedModInfo.version;
					if (text4.StartsWith("V"))
					{
						text4 = "v" + text4.Substring(1, text4.Length - 1);
					}
					else if (!text4.StartsWith("v"))
					{
						text4 = "v" + text4;
					}
				}
				reference3.text = text4;
				reference3.gameObject.SetActive(value: true);
			}
			else
			{
				reference3.gameObject.SetActive(value: false);
			}
			if ((int)mod.available_content > 0)
			{
				if (Strings.TryGet(mod.description, out var result2))
				{
					reference2.toolTip = result2;
				}
				else
				{
					reference2.toolTip = mod.description;
				}
			}
			if (mod.crash_count != 0)
			{
				reference.color = Color.Lerp(Color.white, Color.red, (float)mod.crash_count / 3f);
			}
			KButton reference4 = hierarchyReferences.GetReference<KButton>("ManageButton");
			reference4.GetComponentInChildren<LocText>().text = (mod.IsLocal ? UI.FRONTEND.MODS.MANAGE_LOCAL : UI.FRONTEND.MODS.MANAGE);
			reference4.isInteractable = mod.is_managed;
			if (reference4.isInteractable)
			{
				reference4.GetComponent<ToolTip>().toolTip = mod.manage_tooltip;
				reference4.onClick += mod.on_managed;
			}
			KImage reference5 = hierarchyReferences.GetReference<KImage>("BG");
			MultiToggle toggle = hierarchyReferences.GetReference<MultiToggle>("EnabledToggle");
			toggle.ChangeState(mod.IsEnabledForActiveDlc() ? 1 : 0);
			if (mod.available_content != 0)
			{
				reference5.defaultState = KImage.ColorSelector.Inactive;
				reference5.ColorState = KImage.ColorSelector.Inactive;
				MultiToggle multiToggle = toggle;
				multiToggle.onClick = (System.Action)Delegate.Combine(multiToggle.onClick, (System.Action)delegate
				{
					OnToggleClicked(toggle, mod.label);
				});
				toggle.GetComponent<ToolTip>().OnToolTip = () => mod.IsEnabledForActiveDlc() ? UI.FRONTEND.MODS.TOOLTIPS.ENABLED : UI.FRONTEND.MODS.TOOLTIPS.DISABLED;
			}
			else
			{
				reference5.defaultState = KImage.ColorSelector.Disabled;
				reference5.ColorState = KImage.ColorSelector.Disabled;
			}
			hierarchyReferences.gameObject.SetActive(value: true);
		}
		foreach (DisplayedMod displayedMod2 in displayedMods)
		{
			displayedMod2.rect_transform.gameObject.SetActive(value: true);
		}
		_ = displayedMods.Count;
	}

	private void OnToggleClicked(MultiToggle toggle, Label mod)
	{
		Manager modManager = Global.Instance.modManager;
		bool flag = modManager.IsModEnabled(mod);
		flag = !flag;
		toggle.ChangeState(flag ? 1 : 0);
		modManager.EnableMod(mod, flag, this);
		UpdateToggleAllButton();
	}

	private bool AreAnyModsDisabled()
	{
		return Global.Instance.modManager.mods.Any((Mod mod) => !mod.IsEmpty() && !mod.IsEnabledForActiveDlc() && ShouldDisplayMod(mod));
	}

	private void UpdateToggleAllButton()
	{
		toggleAllButton.GetComponentInChildren<LocText>().text = (AreAnyModsDisabled() ? UI.FRONTEND.MODS.ENABLE_ALL : UI.FRONTEND.MODS.DISABLE_ALL);
	}

	private void OnToggleAllClicked()
	{
		bool flag = AreAnyModsDisabled();
		Manager modManager = Global.Instance.modManager;
		foreach (Mod mod in modManager.mods)
		{
			if (ShouldDisplayMod(mod))
			{
				modManager.EnableMod(mod.label, flag, this);
			}
		}
		BuildDisplay();
		UpdateToggleAllButton();
	}
}
