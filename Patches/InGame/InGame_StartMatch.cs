using BossIntegration.Boss;
using BossIntegration.UI;
using HarmonyLib;

namespace BossIntegration.Patches.InGame;

[HarmonyPatch(typeof(Il2CppAssets.Scripts.Unity.UI_New.InGame.InGame), nameof(Il2CppAssets.Scripts.Unity.UI_New.InGame.InGame.StartMatch))]
internal class InGame_StartMatch
{
    [HarmonyPostfix]
    internal static void Postfix()
    {
        if (!Cache.HasBosses)
            return;

        ModBossUI.Init();
    }
}