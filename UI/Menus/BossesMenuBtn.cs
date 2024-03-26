using BossIntegration.Boss;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity.UI_New;
using System;
using UnityEngine;
using UnityEngine.UI;
using Cache = BossIntegration.Boss.Cache;

namespace BossIntegration.UI.Menus;

internal class BossesMenuBtn
{
    private const string BUTTON_SPRITE_NAME = "BossButton";
    private const string PARENT_NAME = "Friends";
    private const string BUTTON_NAME = "BossIntegration-BossButton";

    private static ModHelperPanel? buttonPanel;

    internal static void OnMenuChanged()
    {
        if (!Cache.HasBosses)
            return;

        if (buttonPanel != null)
            return;

        Init();
    }

    #region UI

    private static void Init()
    {
        Transform foregroundScreen = CommonForegroundScreen.instance.transform;
        Transform roundSetChanger = foregroundScreen.FindChild(PARENT_NAME);

        if (roundSetChanger == null || !Cache.HasBosses)
            return;

        if (roundSetChanger.Find(BUTTON_NAME) == null)
            CreatePanel(roundSetChanger.gameObject);

        if (roundSetChanger.GetComponent<VerticalLayoutGroup>() == null)
        {
            VerticalLayoutGroup group = roundSetChanger.gameObject.AddComponent<VerticalLayoutGroup>();
            group.childControlHeight = false;
            group.childControlWidth = false;
            group.spacing = 100;
        }
    }

    private static void CreatePanel(GameObject screen)
    {
        buttonPanel = screen.AddModHelperPanel(new Info(BUTTON_NAME)
        {
            SizeDelta = new Vector2(220, 220),
            Pivot = new Vector2(1, 0),
            AnchorMax = new Vector2(0.5f, 0.5f),
            AnchorMin = Vector2.zero
        });

        Create(buttonPanel);
    }

    private static void Create(ModHelperPanel panel)
    {
        ModHelperButton bossesBtn = panel.AddButton(
            new Info("BossMenuBtn", 0, 0, 220, 220),
            ModContent.GetSpriteReference<BossIntegration>(BUTTON_SPRITE_NAME).GUID,
            new Action(() => ModGameMenu.Open<BossesMenu>()));

        var count = Cache.Count.ToString();
        var text = "Boss";

        // Make it plurial
        if (Cache.Count > 1)
            text += "es";

        //if (count.Length < 3)
        //    text += $" ({count})";

        _ = bossesBtn.AddText(new Info("Text", 0, -120, 500, 100), text, 60f);
    }

    #endregion
}
