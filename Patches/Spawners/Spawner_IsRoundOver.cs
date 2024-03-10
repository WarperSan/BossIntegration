using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity;
using System;
using System.Linq;

namespace BossIntegration.Patches.Spawners;

[HarmonyPatch(typeof(Spawner), nameof(Spawner.IsRoundOver))]
internal static class Spawner_IsRoundOver
{
    [HarmonyPrefix]
    private static bool Prefix(Spawner __instance, ref bool __result)
    {
        if (__instance.IsCurrentSpawnRoundEmitting())
            return true;

        var aliveBloons = Il2CppAssets.Scripts.Unity.UI_New.InGame.InGame.instance
            .GetAllBloonToSim()
            .Select(x => x.GetSimBloon())
            .ToList();

        var condition = new Func<Bloon, bool>(bloon =>
        {
            return
                bloon != null &&
                ModBoss.TryGetBoss(bloon, out ModBoss? boss) &&
                boss != null &&
                boss.BlockRounds;
        });

        // If no bloons meets the condition
        if (!aliveBloons.Any(condition))
            return true;

        //TODO: better way to force rounds to keep coming

        ProfileModel playerProfile = Game.instance.GetPlayerProfile();

        if (!playerProfile.inGameSettings.autoPlay)
        {
            playerProfile.inGameSettings.autoPlay = true;
            Il2CppAssets.Scripts.Unity.UI_New.InGame.InGame.instance.bridge.SetAutoPlay(true); 
        }

        __result = true;
        return false;
    }
}