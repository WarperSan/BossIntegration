﻿using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons.Behaviors;

namespace BossIntegration.Patches.Bloons.Behaviors;

[HarmonyPatch(typeof(TimeTrigger), nameof(TimeTrigger.Trigger))]
internal static class TimeTrigger_Trigger
{
    [HarmonyPrefix]
    private static void Prefix(TimeTrigger __instance)
    {
        if (!ModBoss.IsAliveBoss(__instance.bloon, out ModBoss? boss) || boss == null)
            return;

        boss.TimerTick(__instance.bloon);
    }
}