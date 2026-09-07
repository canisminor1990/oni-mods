using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace ThermalInfoCards
{
	internal static class HoverHooks
	{
		private static MethodInfo drawText;
		private static MethodInfo getComponent;
		private static MethodInfo posToCell;

		private static void AddThermalInfoEntities(HoverTextDrawer drawer, string text, TextStyleSetting style)
		{
			HoverTooltip instance = Patches.Tooltip;
			PrimaryElement primary = instance?.PrimaryElement;
			if (instance == null || primary == null || primary.gameObject == null)
			{
				drawer.DrawText(text, style);
				return;
			}

			float insulation = 1.0f;
			BuildingDef def = null;
			if (primary.TryGetComponent(out Building building) && building.Def != null)
			{
				insulation = building.Def.ThermalConductivity;
				def = building.Def;
			}

			instance.Drawer = drawer;
			instance.Style = style;
			try
			{
				instance.DisplayThermalInfo(
					primary.Element,
					primary.Temperature,
					ThermalUtil.GetAdjustedMass(primary.gameObject, def, primary.Mass),
					insulation);
			}
			finally
			{
				instance.Drawer = null;
				instance.Style = null;
				instance.PrimaryElement = null;
			}
		}

		private static void AddThermalInfoElements(HoverTextDrawer drawer, string text, TextStyleSetting style)
		{
			HoverTooltip instance = Patches.Tooltip;
			int cell = instance != null ? instance.Cell : 0;
			if (instance == null || !Grid.IsValidCell(cell))
			{
				drawer.DrawText(text, style);
				return;
			}

			Element element = Grid.Element[cell];
			float mass = Grid.Mass[cell];
			instance.Cell = 0;
			if (element == null || mass <= 0.0f)
			{
				drawer.DrawText(text, style);
				return;
			}

			instance.Drawer = drawer;
			instance.Style = style;
			try
			{
				instance.DisplayThermalInfo(element, Grid.Temperature[cell], mass);
			}
			finally
			{
				instance.Drawer = null;
				instance.Style = null;
			}
		}

		private static int SetCell(int cell)
		{
			HoverTooltip instance = Patches.Tooltip;
			if (instance != null)
				instance.Cell = cell;
			return cell;
		}

		private static PrimaryElement SetElement(PrimaryElement element)
		{
			HoverTooltip instance = Patches.Tooltip;
			if (instance != null && element != null)
				instance.PrimaryElement = element;
			return element;
		}

		private static bool IsCallTo(CodeInstruction instruction, MethodInfo method)
		{
			return instruction != null
				&& (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
				&& method != null
				&& instruction.operand is MethodInfo operand
				&& operand == method;
		}

		private static bool IsFormattedTemperature(CodeInstruction instruction)
		{
			if (instruction == null || (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt))
				return false;
			return instruction.operand is MethodInfo method
				&& method.DeclaringType == typeof(GameUtil)
				&& method.Name == nameof(GameUtil.GetFormattedTemperature);
		}

		internal static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
		{
			ResolveTargets();
			List<CodeInstruction> method = new List<CodeInstruction>(instructions);
				int n = method.Count;
				bool patchCell = false;
				bool patchElement = false;
				bool patchEntityDraw = false;
				bool patchCellDraw = false;
				int i;

				for (i = 0; i < n && (!patchCell || !patchElement); i++)
				{
					CodeInstruction instruction = method[i];
					if (!patchCell && IsCallTo(instruction, posToCell))
					{
						method.Insert(++i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HoverHooks), nameof(SetCell))));
						patchCell = true;
						n++;
					}
					if (!patchElement && IsCallTo(instruction, getComponent))
					{
						method.Insert(++i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HoverHooks), nameof(SetElement))));
						patchElement = true;
						n++;
					}
				}

				for (; i < n && !IsFormattedTemperature(method[i]); i++)
				{
				}
				for (i++; i < n; i++)
				{
					if (IsCallTo(method[i], drawText))
					{
						method[i].opcode = OpCodes.Call;
						method[i].operand = AccessTools.Method(typeof(HoverHooks), nameof(AddThermalInfoEntities));
						patchEntityDraw = true;
						break;
					}
				}

				for (i++; i < n && !IsFormattedTemperature(method[i]); i++)
				{
				}
				for (i++; i < n; i++)
				{
					if (IsCallTo(method[i], drawText))
					{
						method[i].opcode = OpCodes.Call;
						method[i].operand = AccessTools.Method(typeof(HoverHooks), nameof(AddThermalInfoElements));
						patchCellDraw = true;
						break;
					}
				}

				Debug.Log(Mod.LogPrefix + "hover transpiler cell=" + patchCell
					+ " element=" + patchElement
					+ " entityDraw=" + patchEntityDraw
					+ " cellDraw=" + patchCellDraw);
			if (!patchCell || !patchElement || !patchEntityDraw || !patchCellDraw)
				Debug.LogWarning(Mod.LogPrefix + "hover transpiler missed a site; extra thermal lines may not appear");
			return method;
		}

		private static void ResolveTargets()
		{
			if (drawText != null)
				return;

			drawText = AccessTools.Method(typeof(HoverTextDrawer), nameof(HoverTextDrawer.DrawText), new[]
			{
				typeof(string), typeof(TextStyleSetting)
			});
			MethodInfo generic = AccessTools.Method(typeof(Component), nameof(Component.GetComponent), Type.EmptyTypes);
			if (generic != null && generic.IsGenericMethodDefinition)
				getComponent = generic.MakeGenericMethod(typeof(PrimaryElement));
			posToCell = AccessTools.Method(typeof(Grid), nameof(Grid.PosToCell), new[] { typeof(Vector3) });

			Debug.Log(Mod.LogPrefix + "hover targets drawText=" + (drawText != null)
				+ " getComponent=" + (getComponent != null)
				+ " posToCell=" + (posToCell != null));
		}
	}
}
