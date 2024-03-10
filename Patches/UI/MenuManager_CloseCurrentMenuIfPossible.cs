using BossIntegration.UI.Menus;
using HarmonyLib;
using Il2CppAssets.Scripts.Unity.Menu;

namespace BossIntegration.Patches.UI;

[HarmonyPatch(typeof(MenuManager), nameof(MenuManager.CloseCurrentMenuIfPossible))]
internal static class MenuManager_CloseCurrentMenu
{
    [HarmonyPostfix]
    private static void Postfix() => BossesMenuBtn.OnMenuChanged();

    //[HarmonyPostfix]
    //private static void Postfix(MenuManager __instance, ref GameMenu __state)
    //    => BossesMenuBtn.OnMenuChanged(
    //        __state.Exists()?.name ?? "",
    //        __instance.menuStack.ToList().SkipLast(1).LastOrDefault()?.Item1 ?? "");
}