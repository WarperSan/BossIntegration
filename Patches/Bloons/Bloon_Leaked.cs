using BossIntegration;
using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons;
namespace BTD_Mod_Helper.Patches.Bloons;

[HarmonyPatch(typeof(Bloon), nameof(Bloon.Leaked))]
internal class Blooon_Leaked
{
    [HarmonyPrefix]
    internal static bool Prefix(Bloon __instance)
    {
        SessionData.Instance.LeakedBloons.Add(__instance);

        // If the bloon wasn't a boss
        if (!ModBoss.IsAliveBoss(__instance, out ModBoss? boss) || boss == null)
            return true;

        boss.Leak(__instance);

        return true;
    }
}