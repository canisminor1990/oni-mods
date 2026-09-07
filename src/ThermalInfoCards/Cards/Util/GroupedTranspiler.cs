using BetterInfoCards;
using HarmonyLib;
using System.Collections.Generic;
using ThermalInfoCards;

namespace BetterInfoCards.Util;

[HarmonyPatch(typeof(SelectToolHoverTextCard), nameof(SelectToolHoverTextCard.UpdateHoverElements))]
class GroupedTranspiler
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return DetectRunStart_Patch.ChildTranspiler(
            ExportSelectToolData.GetSelectInfo_Patch.ChildTranspiler(
                HideElementCategory.ChildTranspiler(
                    HoverHooks.Transpile(instructions))));
    }
}
