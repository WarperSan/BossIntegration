using BossIntegration;
using BossIntegration.Boss;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons;

namespace BTD_Mod_Helper.Patches.Bloons;

[HarmonyPatch(typeof(Bloon), nameof(Bloon.Leaked))]
internal class Blooon_Leaked
{
    [HarmonyPrefix]
    internal static void Prefix(Bloon __instance)
    {
        SessionData.Instance.LeakedBloons.Add(__instance);

        // If the bloon wasn't a boss
        if (!__instance.IsAliveBoss(out ModBoss? boss) || boss == null)
            return;

        boss.Leak(__instance);
    }
}