using BossIntegration.Boss;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons;

namespace BossIntegration.Patches.Bloons;

[HarmonyPatch(typeof(Bloon), nameof(Bloon.Damage))]
internal class Bloon_Damage
{
    [HarmonyPostfix]
    internal static void Postfix(Bloon __instance, float totalAmount)
    {
        if (!__instance.TryGetBoss(out ModBoss? boss) || boss == null)
            return;

        boss.Damage(__instance, totalAmount);
    }
}