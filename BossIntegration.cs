using BossIntegration;
using BossIntegration.Boss;
using BossIntegration.UI;
using BossIntegration.UI.Menus;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.Races;
using Il2CppAssets.Scripts.Unity.UI_New.Pause;
using Il2CppSystem.Linq;
using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.IO;
using UnityEngine.ResourceManagement.AsyncOperations;

[assembly: MelonInfo(typeof(BossIntegration.BossIntegration), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BossIntegration;

public class BossIntegration : BloonsTD6Mod
{
    private static readonly ModSettingCategory General = new("General")
    {
        collapsed = false,
        icon = VanillaSprites.SettingsIcon
    };

    public static readonly ModSettingBool ShowBossSpeedAsPercentage = new(true)
    {
        description = "Toggles showing the boss speed in the boss menu as a percentage or the literal value.",
        category = General,
        icon = VanillaSprites.FasterBossIcon,
    };

    public static readonly ModSettingBool FormatBossHP = new(false)
    {
        description = "If toggled, the bosses HP will be format to make it easier to read.\n\nWarning: this process will make the game slower because of the calculations.",
        category = General,
        icon = VanillaSprites.LivesIcon,
    };

    public override void OnInGameLoaded(InGame inGame)
    {
        if (!Boss.Cache.HasBosses)
            return;

        ModBossUI.Init();
    }

    public override void OnPauseScreenOpened(PauseScreen pauseScreen)
    {
        var size = 100;

        var panel = pauseScreen.transform.Find("Bg").gameObject.AddModHelperPanel(new Info("AC")
        {
            AnchorMinX = 0,
            AnchorMaxX = 0,
            AnchorMinY = 1,
            AnchorMaxY = 1,
            Size = size,
            X = size,
            Y = -size / 1.25f
        });

        var button = panel.AddButton(
            new Info("BossMenuBtn", 0, 0, size, size),
            ModContent.GetSpriteReference<BossIntegration>("BossButton").GUID,
            new System.Action(() => ModGameMenu.Open<BossesMenu>()));
    }
}