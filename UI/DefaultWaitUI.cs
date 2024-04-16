using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BossIntegration.UI;

internal class DefaultWaitUI
{
    public static ModHelperPanel? Create(ModHelperPanel parent, ModBoss[] bosses, int round)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("WaitPanel")
        {
            X = 50,
            Y = -25,
            Height = 100,
            AnchorMinX = 0,
            AnchorMaxX = 1,
            Scale = Vector3.one * 1.5f
        }, VanillaSprites.SmallSquareWhite);
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.34f);

        if (bosses.Length <= 3)
            Icons(panel, bosses, 175);

        Text(panel, bosses.Length, round);

        return panel;
    }

    #region Components

    private static void Text(ModHelperPanel parent, int amount, int rounds)
    {
        var text = amount switch
        {
            1 => "Boss appears",
            2 => "A Duo appears",
            _ => $"A float of {amount} appears"
        };

        text += rounds == 1
            ? " next round"
            : $" in {rounds} rounds";

        parent.AddText(new Info("Title", InfoPreset.FillParent)
        {
            FlexWidth = 4
        }, text, 40, Il2CppTMPro.TextAlignmentOptions.Center);
    }

    private static void Icons(ModHelperPanel parent, ModBoss[] bosses, float size)
    {
        ModHelperPanel iconsHolder = parent.AddPanel(new Info("IconsHolder")
        {
            Flex = 1,
            AnchorMinX = 0,
            AnchorMaxX = 0,
        }, null);

        var objs = new List<GameObject>();

        foreach (ModBoss boss in bosses)
        {
            ModHelperImage icon = iconsHolder.AddImage(new Info(boss.Name)
            {
                Size = size,
                Anchor = new Vector2(.1f, .5f)
            }, boss.IconReference.guidRef);

            objs.Add(icon.gameObject);
        }

        SetCircular(Vector2.zero, objs, 50);
    }

    #endregion Components

    /// <summary>
    /// Puts the given objects in a circular layout. If the list has 1 or 2 items, the layout is hardcoded;
    /// </summary>
    private static void SetCircular(Vector2 center, List<GameObject> objects, float radius)
    {
        var count = objects.Count;

        if (count == 1)
        {
            objects[0].transform.localPosition = center;
            return;
        }

        if (count == 2)
        {
            objects[0].transform.localPosition = (new Vector2(0.5f, 0.5f) * radius) + center;
            objects[1].transform.localPosition = (new Vector2(-0.5f, -0.5f) * radius) + center;
            return;
        }

        for (var pointNum = 0; pointNum < objects.Count; pointNum++)
        {
            var i = pointNum * 1f / objects.Count;
            var angle = i * Mathf.PI * 2;
            var x = Mathf.Sin(angle);
            var y = Mathf.Cos(angle);
            objects[pointNum].transform.localPosition = (new Vector2(x, y) * radius) + center;
        }
    }
}