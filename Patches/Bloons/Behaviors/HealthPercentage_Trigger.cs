using BossIntegration.Boss;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons.Behaviors;

namespace BossIntegration.Patches.Bloons.Behaviors;

[HarmonyPatch(typeof(HealthPercentTrigger), nameof(HealthPercentTrigger.Trigger))]
internal static class HealthPercentage_Trigger
{
    [HarmonyPrefix]
    private static void Prefix(HealthPercentTrigger __instance)
    {
        if (!__instance.bloon.TryGetBoss(out ModBoss? boss) || boss == null)
            return;

        boss.SkullEffectUI(__instance.bloon);
        boss.SkullEffect(__instance.bloon);
    }
}