using BossIntegration.UI;
using HarmonyLib;
using Il2CppAssets.Scripts.Unity.UI_New.GameOver;
using System;

namespace BossIntegration.Patches.UI.GameOver;

[HarmonyPatch(typeof(DefeatScreen), nameof(DefeatScreen.Awake))]
internal class DefeatScreen_Awake
{
    [HarmonyPostfix]
    internal static void Postfix(DefeatScreen __instance)
    {
        if (!ModBoss.HasBosses)
            return;

        __instance.retryLastRoundButton.onClick.AddListener(new Action(ModBossUI.Init));
    }
}