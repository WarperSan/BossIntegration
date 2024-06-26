using BossIntegration.UI.Menus;
using HarmonyLib;
using Il2CppAssets.Scripts.Unity.Menu;

namespace BossIntegration.Patches.UI;

[HarmonyPatch(typeof(MenuManager), nameof(MenuManager.OpenMenu))]
internal static class MenuManager_OpenMenu
{
    [HarmonyPrefix]
    private static void Prefix() => BossesMenuBtn.OnMenuChanged();

    //[HarmonyPrefix]
    //private static void Prefix(MenuManager __instance, string menuName)
    //    => BossesMenuBtn.OnMenuChanged(
    //        __instance.GetCurrentMenu().Exists()?.name ?? "",
    //        menuName);
}