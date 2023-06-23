﻿using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons.Behaviors;

namespace BossIntegration;

[HarmonyPatch(typeof(HealthPercentTrigger), nameof(HealthPercentTrigger.Trigger))]
internal static class HealthPercentage_Trigger
{
    [HarmonyPrefix]
    private static void Prefix(HealthPercentTrigger __instance)
    {
        if (ModBoss.Cache.ContainsKey(__instance.bloon.bloonModel.name))
        {
            ModBoss.Cache[__instance.bloon.bloonModel.name].SkullEffect(__instance.bloon);
        }
    }
}