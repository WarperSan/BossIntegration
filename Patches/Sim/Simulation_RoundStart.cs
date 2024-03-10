using BossIntegration.UI;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation;
using System.Collections.Generic;

namespace BossIntegration.Patches.Sim;

[HarmonyPatch(typeof(Simulation), nameof(Simulation.RoundStart))]
internal class Simulation_RoundStart
{
    [HarmonyPostfix]
    internal static void Postfix()
    {
        if (!ModBoss.HasBosses)
            return;

        ModBossUI.Update();

        // Get current round
        var currentRound = Il2CppAssets.Scripts.Unity.UI_New.InGame.InGame.Bridge.GetCurrentRound() + 1;

        IEnumerable<ModBoss> bosses = ModBoss.GetBossesForRound(currentRound);

        foreach (ModBoss boss in bosses)
        {
            if (!ModBoss.GetPermission(boss, currentRound))
                continue;

            boss.Spawn(currentRound);
        }
    }
}