using BossIntegration.Boss;
using BossIntegration.UI;
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
        if (!Cache.HasBosses)
            return;

        ModBossUI.Update();

        // Get current round
        var currentRound = Il2CppAssets.Scripts.Unity.UI_New.InGame.InGame.Bridge.GetCurrentRound() + 1;

        IEnumerable<ModBoss> bosses = Cache.GetBossesForRound(currentRound);

        foreach (ModBoss boss in bosses)
        {
            if (!boss.GetPermission(currentRound))
                continue;

            boss.Spawn(currentRound);
        }
    }
}