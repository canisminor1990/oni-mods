using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterInfoCards.Util;

internal static class TranspilerUtil
{
	public static bool IsCallTo(CodeInstruction instruction, MethodInfo method)
	{
		return instruction != null
			&& (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
			&& method != null
			&& instruction.operand is MethodInfo operand
			&& operand == method;
	}

	public static int FindCall(List<CodeInstruction> method, MethodInfo target, int start)
	{
		if (target == null)
			return -1;
		for (int i = start; i < method.Count; i++)
		{
			if (IsCallTo(method[i], target))
				return i;
		}
		return -1;
	}

	public static int FindCallBackwards(List<CodeInstruction> method, MethodInfo target, int start)
	{
		if (target == null)
			return -1;
		for (int i = start; i >= 0; i--)
		{
			if (IsCallTo(method[i], target))
				return i;
		}
		return -1;
	}

	public static void InsertBefore(List<CodeInstruction> method, int index, params CodeInstruction[] added)
	{
		method.InsertRange(index, added);
	}

	public static void InsertAfter(List<CodeInstruction> method, int index, params CodeInstruction[] added)
	{
		method.InsertRange(index + 1, added);
	}
}
