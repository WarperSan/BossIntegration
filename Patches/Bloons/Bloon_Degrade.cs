using HarmonyLib;
using Il2CppAssets.Scripts.Simulation.Bloons;
namespace BossIntegration.Patches.Bloons;

[HarmonyPatch(typeof(Bloon), nameof(Bloon.Degrade))]
internal class Bloon_Degrade
{
    [HarmonyPostfix]
    internal static void Postfix(Bloon __instance)
    {
        if (!ModBoss.TryGetBoss(__instance, out ModBoss? boss) || boss == null)
            return;

        boss.Pop(__instance);
    }
}