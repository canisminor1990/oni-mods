using BetterInfoCards.Util;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ThermalInfoCards;
using UnityEngine;

namespace BetterInfoCards;

public class ExportSelectToolData
{
	private static KSelectable curSelectable;
	private static (string id, object data) curTextInfo = (string.Empty, null);

	public static KSelectable ConsumeSelectable()
	{
		KSelectable sel = curSelectable;
		curSelectable = null;
		return sel;
	}

	public static (string id, object data) ConsumeTextInfo()
	{
		(string id, object data) ti = curTextInfo;
		curTextInfo = (string.Empty, null);
		return ti;
	}

	public class GetSelectInfo_Patch
	{
		public static IEnumerable<CodeInstruction> ChildTranspiler(IEnumerable<CodeInstruction> codes)
		{
			List<CodeInstruction> method = new List<CodeInstruction>(codes);
			MethodInfo getPrimary = AccessTools.Method(typeof(Component), nameof(Component.GetComponent), System.Type.EmptyTypes);
			if (getPrimary != null && getPrimary.IsGenericMethodDefinition)
				getPrimary = getPrimary.MakeGenericMethod(typeof(PrimaryElement));

			MethodInfo unitName = AccessTools.Method(typeof(GameUtil), nameof(GameUtil.GetUnitFormattedName), new[] { typeof(GameObject), typeof(bool) });
			MethodInfo disease = FindFormattedDisease();
			MethodInfo statusGroup = AccessTools.Method(typeof(KSelectable), nameof(KSelectable.GetStatusItemGroup));
			MethodInfo isWarning = AccessTools.Method(typeof(SelectToolHoverTextCard), "IsStatusItemWarning");
			MethodInfo temperature = FindFormattedTemperature();

			MethodInfo setSel = AccessTools.Method(typeof(GetSelectInfo_Patch), nameof(SetSelectable));
			MethodInfo exportTitle = AccessTools.Method(typeof(GetSelectInfo_Patch), nameof(ExportTitle));
			MethodInfo exportGerms = AccessTools.Method(typeof(GetSelectInfo_Patch), nameof(ExportGerms));
			MethodInfo exportStatus = AccessTools.Method(typeof(GetSelectInfo_Patch), nameof(ExportStatus));
			MethodInfo exportTemp = AccessTools.Method(typeof(GetSelectInfo_Patch), nameof(ExportTemp));

			int first = TranspilerUtil.FindCall(method, getPrimary, 0);
			int applied = 0;
			if (first >= 0 && setSel != null)
			{
				TranspilerUtil.InsertBefore(method, first, new CodeInstruction(OpCodes.Call, setSel));
				applied++;
			}

			int cursor = first;
			applied += InsertActionBefore(method, unitName, exportTitle, cursor);
			applied += InsertActionBefore(method, disease, exportGerms, cursor);
			cursor = InsertStatusExport(method, statusGroup, isWarning, exportStatus, cursor);
			if (cursor >= 0)
				applied++;
			int status2 = InsertStatusExport(method, statusGroup, isWarning, exportStatus, cursor >= 0 ? cursor + 1 : first);
			if (status2 >= 0)
				applied++;
			applied += InsertActionBefore(method, temperature, exportTemp, first);

			Debug.Log(Mod.LogPrefix + "select-export transpiler sites=" + applied);
			if (applied < 6)
				Debug.LogWarning(Mod.LogPrefix + "select-export transpiler missed a site; stacked card lines may be incomplete");
			return method;
		}

		public static void Export(string name, object data)
		{
			curTextInfo = (name, data);
		}

		public static KSelectable SetSelectable(KSelectable selectable)
		{
			curSelectable = selectable;
			return selectable;
		}

		public static void ExportTitle()
		{
			ExportGO(ConverterManager.title);
		}

		public static void ExportGerms()
		{
			ExportGO(ConverterManager.germs);
		}

		public static void ExportTemp()
		{
			ExportGO(ConverterManager.temp);
		}

		public static StatusItemGroup.Entry ExportStatus(StatusItemGroup.Entry entry)
		{
			if (entry.item != null)
				Export(entry.item.Id, entry.data);
			return entry;
		}

		private static void ExportGO(string name)
		{
			Export(name, curSelectable != null ? curSelectable.gameObject : null);
		}

		private static int InsertActionBefore(List<CodeInstruction> method, MethodInfo target, MethodInfo insert, int start)
		{
			int index = TranspilerUtil.FindCall(method, target, start < 0 ? 0 : start);
			if (index < 0 || insert == null)
				return 0;
			TranspilerUtil.InsertBefore(method, index, new CodeInstruction(OpCodes.Call, insert));
			return 1;
		}

		private static int InsertStatusExport(List<CodeInstruction> method, MethodInfo statusGroup, MethodInfo isWarning, MethodInfo insert, int start)
		{
			int group = TranspilerUtil.FindCall(method, statusGroup, start < 0 ? 0 : start);
			if (group < 0)
				return -1;
			int warning = TranspilerUtil.FindCall(method, isWarning, group);
			if (warning < 0 || insert == null)
				return -1;
			TranspilerUtil.InsertBefore(method, warning, new CodeInstruction(OpCodes.Call, insert));
			return warning + 1;
		}

		private static MethodInfo FindFormattedDisease()
		{
			foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(GameUtil)))
			{
				if (method.Name == nameof(GameUtil.GetFormattedDisease) && method.GetParameters().Length >= 2)
					return method;
			}
			return AccessTools.Method(typeof(GameUtil), nameof(GameUtil.GetFormattedDisease));
		}

		private static MethodInfo FindFormattedTemperature()
		{
			foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(GameUtil)))
			{
				if (method.Name == nameof(GameUtil.GetFormattedTemperature))
					return method;
			}
			return AccessTools.Method(typeof(GameUtil), nameof(GameUtil.GetFormattedTemperature));
		}
	}
}
