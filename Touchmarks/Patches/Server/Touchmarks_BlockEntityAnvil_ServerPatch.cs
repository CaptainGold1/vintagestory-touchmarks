using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Touchmarks.Utils;
using System.Reflection.Emit;
using Touchmarks;

namespace Touchmarks.Patches.Server {
    [HarmonyPatch(typeof(BlockEntityAnvil))]
    [HarmonyPatchCategory("Server")]
    [HarmonyDebug]
    internal class Touchmarks_BlockEntityAnvil_ServerPatch {
        static void SetTouchmarkOwner(IServerPlayer owner, ItemStack outStack) {
            if (TouchmarksModSystem.config.debugMessages) {
                TouchmarksModSystem.Logger.Debug($"SetTouchmarkOwner called on item code {outStack.Item.Code.ToString()}");
            }
            
            if (owner != null) {
                if (TouchmarksUtils.IsItemPersonalTouchmark(outStack.Item)) {
                    if (TouchmarksModSystem.config.debugMessages) {
                        TouchmarksModSystem.Logger.Debug($"Assigning touchmark owner {owner.PlayerName}");
                    }
                    outStack.Attributes.SetString("touchmarkOwner", owner.PlayerName);
                }
            }
        }

        // Thanks to Blacksmith Name for the code for this patch <3
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BlockEntityAnvil.CheckIfFinished))]
        static IEnumerable<CodeInstruction> CheckIfFinished_Transpiler(IEnumerable<CodeInstruction> instructions) {
            var code = new List<CodeInstruction>(instructions);

            bool found = false;

            var m_setTouchmarkOwner = AccessTools.Method(typeof(Touchmarks_BlockEntityAnvil_ServerPatch), nameof(SetTouchmarkOwner));

            for (int i = 0; i < code.Count; i++) {
                if (!found && i > 1 && i < code.Count - 2 &&
                    code[i - 1].opcode == OpCodes.Stfld &&
                    code[i    ].opcode == OpCodes.Ldarg_1 && 
                    code[i + 1].opcode == OpCodes.Brfalse_S &&
                    code[i + 2].opcode == OpCodes.Ldarg_1
                ) {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, m_setTouchmarkOwner);
                    found = true;
                }
                yield return code[i];
            }
        }
    }
}
