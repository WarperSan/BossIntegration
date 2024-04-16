using BossIntegration.Boss;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BossIntegration.UI;

internal class DefaultHealthUI
{
    public static ModHelperPanel? Create(
        ModBoss boss,
        ModHelperPanel parent,
        BloonModel model,
        ref BossUI ui,
        int round
        )
    {
        if (!boss.RoundsInfo.TryGetValue(round, out BossRoundInfo infos))
            return null;

        // Parent panel
        ModHelperPanel panel = parent.AddPanel(new Info("Panel")
        {
            Width = 1500,
            Height = 200
        });
        panel.transform.SetAsFirstSibling();

        ui.HpBar = HealthBar(panel);
        Frame(panel);
        ui.HpText = HealthText(panel, 69);

        ui.Skulls = boss.UsesSkull() ? Skulls(panel, model, 150) : new();

        if (infos.tier.HasValue)
            Stars(panel, infos.tier.Value, 100);

        Icon(panel, boss, 300);

        return panel;
    }

    #region Components

    private static void Icon(ModHelperPanel parent, ModBoss boss, float size) => parent.AddImage(new Info("Icon")
    {
        Pivot = new Vector2(0.5f, 0.5f),
        AnchorMinX = 0,
        AnchorMaxX = 0,
        AnchorMaxY = 0.5f,
        AnchorMinY = 0.5f,
        X = -size / 2,
        Size = size,
    }, boss.IconReference.guidRef);

    private static void Stars(ModHelperPanel parent, uint tier, float size)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("Stars")
        {
            AnchorMinX = 0.0f,
            AnchorMaxX = 0.1f,
            AnchorMinY = 1.0f,
            AnchorMaxY = 1.0f,
        }, null, RectTransform.Axis.Horizontal);

        var sprite = ModContent.GetTextureGUID<BossIntegration>("BossStar");

        for (var i = 0; i < tier; i++)
            _ = panel.AddImage(new Info("Star" + i, size), sprite);
    }

    private static ModHelperText HealthText(ModHelperPanel parent, float fontSize) => parent.AddText(
            new Info("HealthText")
            {
                AnchorMinX = 0.1f,
                AnchorMaxX = 1.0f,
                AnchorMinY = 1.0f,
                AnchorMaxY = 1.0f,
                Height = fontSize,
            },
            "888888888888 / 888888888888",
            fontSize,
            Il2CppTMPro.TextAlignmentOptions.BottomRight);

    public static Image HealthBar(ModHelperPanel parent)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("HealthBar")
        {
            AnchorMinX = 0.01f,
            AnchorMaxX = 0.99f,
            AnchorMinY = 0.0f,
            AnchorMaxY = 0.75f,
        });

        Image slider = panel.AddImage(
            new Info("Gradient", InfoPreset.FillParent),
            ModContent.GetTextureGUID<BossIntegration>("BossBarGradient")
            ).Image;
        slider.type = Image.Type.Filled;
        slider.fillMethod = Image.FillMethod.Horizontal;

        return slider;
    }

    public static void Frame(ModHelperPanel parent)
    {
        Image image = parent.AddImage(
            new Info("Frame")
            {
                AnchorMinX = 0.0f,
                AnchorMaxX = 1.0f,
                AnchorMinY = 0.0f,
                AnchorMaxY = 0.75f,
            },
            ModContent.GetTextureGUID<BossIntegration>("BossFrame")
            ).Image;

        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 0.1f;

        var rect = new Rect(0, 0, image.sprite.texture.width, image.sprite.texture.height);
        image.sprite = Sprite.Create(image.sprite.texture, rect, new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect, new Vector4(30, 30, 30, 30));
    }

    private static List<ModHelperImage> Skulls(ModHelperPanel parent, BloonModel model, float size)
    {
        ModHelperPanel holder = parent.AddPanel(new Info("SkullsHolder")
        {
            AnchorMinX = 0,
            AnchorMaxX = 1,
            AnchorMinY = 0,
            AnchorMaxY = 0,
        }, VanillaSprites.PanelFrame);

        var skulls = new List<ModHelperImage>();

        Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float> percentageValues =
            model.GetBehavior<HealthPercentTriggerModel>().percentageValues;

        for (var i = percentageValues.Count - 1; i >= 0; i--)
            skulls.Add(Skull(holder, Mathf.Clamp(percentageValues[i], 0, 1), size));

        return skulls;
    }

    public static ModHelperImage Skull(ModHelperPanel parent, float value, float size) => parent.AddImage(new Info("Skull")
    {
        AnchorMinX = value,
        AnchorMaxX = value,
        Y = 0,
        Size = size,
    }, ModContent.GetTextureGUID<BossIntegration>("BossSkullPipOff"));

    #endregion Components
}