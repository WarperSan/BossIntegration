using BossIntegration.Boss;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity.UI_New.Settings;
using Il2CppTMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BossIntegration.UI.Menus;

internal class BossesSettings : ModGameMenu<HotkeysScreen>
{
    private static bool hasSaved = false;

    private static GameObject? canvas;

    private static void Init(GameObject canvas)
    {
        ModHelperPanel mainPanel = canvas.AddModHelperPanel(new Info("MainPanel")
        {
            X = 0,
            Y = 0,
            AnchorMin = new Vector2(0.15f, 0.09f),
            AnchorMax = new Vector2(0.85f, 0.85f),

        }/*, VanillaSprites.MainBGPanelBlueNotchesShadow*/);

        var bosses = Boss.Cache.GetBosses().ToList();
        bosses.Sort((b1, b2) => b1.DisplayName.CompareTo(b2.DisplayName));

        var rounds = GetRounds(bosses);

        ModHelperScrollPanel buttonsSP = Buttons_Init(mainPanel, rounds, bosses);
        ModHelperScrollPanel roundsSP = Rounds_Init(mainPanel, rounds);
        ModHelperScrollPanel bossesSP = Bosses_Init(mainPanel, bosses);

        buttonsSP.ScrollRect.onValueChanged.AddListener(new Action<Vector2>(r =>
        {
            roundsSP.ScrollRect.content.position = FollowOtherScroll(roundsSP, buttonsSP, false, true);
            bossesSP.ScrollRect.content.position = FollowOtherScroll(bossesSP, buttonsSP, true, false);
        }));
    }

    private static Vector3 FollowOtherScroll(ModHelperScrollPanel target, ModHelperScrollPanel source, bool horizontal = false, bool vertical = false) => new(
                horizontal ? target.RectTransform.position.x + source.ScrollRect.content.position.x - source.ScrollRect.rectTransform.position.x : target.ScrollRect.content.position.x,
                vertical ? target.RectTransform.position.y + source.ScrollRect.content.position.y - source.ScrollRect.rectTransform.position.y : target.ScrollRect.content.position.y,
                0);

    #region ModGameMenu

    /// <inheritdoc/>
    public override bool OnMenuOpened(Il2CppSystem.Object data)
    {
        canvas = this.GameMenu.gameObject;
        canvas.DestroyAllChildren();

        this.GameMenu.saved = true;
        this.CommonForegroundHeader.SetText("BOSS ROUNDS");

        Init(canvas);

        hasSaved = false;

        return true;
    }

    /// <inheritdoc/>
    public override void OnMenuClosed()
    {
        if (hasSaved)
            return;

        Permissions.SavePermissions();
        hasSaved = true;
    }

    #endregion

    #region Rounds

    private const float ROUND_SIZE = 200;
    private const float ROUND_SPACING = 25;

    private static ModHelperScrollPanel Rounds_Init(ModHelperPanel parent, int[] rounds)
    {
        ModHelperScrollPanel roundSP = parent.AddScrollPanel(
            new Info("RoundsScrollPanel")
            {
                AnchorMinX = 0.03f,
                AnchorMaxX = 0.15f,
                AnchorMinY = 0.05f,
                AnchorMaxY = 0.80f,
            }, RectTransform.Axis.Vertical, VanillaSprites.MainBGPanelGrey);
        roundSP.ScrollRect.enabled = false;

        ModHelperPanel roundPanel = roundSP.ScrollContent.AddPanel(new Info("RoundPanel")
        {
            Height = GetRoundsHeight(rounds.Length),
            Width = 200,
        });

        GridLayoutGroup roundGLG = roundPanel.AddComponent<GridLayoutGroup>();
        roundGLG.spacing = new Vector2(0, ROUND_SPACING);
        roundGLG.cellSize = Vector2.one * ROUND_SIZE;
        roundGLG.childAlignment = TextAnchor.MiddleCenter;
        roundGLG.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        roundGLG.constraintCount = 1;

        for (var i = 0; i < rounds.Length; i++)
        {
            var r = rounds[i].ToString();

            var onRoundClicked = new Action(() =>
            {
                ToggleAll(c => c.name.EndsWith(r));
            });

            ModHelperText t = roundPanel
                .AddPanel(new Info("Round" + rounds[i]))
                .AddButton(new Info("RoundBtn"), null, onRoundClicked)
                .AddText(new Info("Label")
                {
                    Size = ROUND_SIZE
                }, rounds[i].ToString(), 69, TextAlignmentOptions.MidlineRight);
            t.Text.m_maxFontSize = t.Text.fontSize;
            t.Text.enableAutoSizing = true;
        }

        return roundSP;
    }

    private static float GetRoundsHeight(int count) => (ROUND_SIZE * count) + (ROUND_SPACING * (count - 1)) + BUTTONS_PADDING;

    private static int[] GetRounds(IEnumerable<ModBoss> bosses)
    {
        var rounds = new List<int>();

        foreach (ModBoss b in bosses)
        {
            foreach ((var round, _) in b.RoundsInfo)
            {
                if (!rounds.Contains(round))
                    rounds.Add(round);
            }
        }

        rounds.Sort();
        return rounds.ToArray();
    }

    #endregion

    #region Bosses

    private const float BOSS_SIZE = 200;
    private const float BOSS_SPACING = 25;

    private static ModHelperScrollPanel Bosses_Init(ModHelperPanel parent, IEnumerable<ModBoss> bosses)
    {
        ModHelperScrollPanel bossSP = parent.AddScrollPanel(
           new Info("BossesScrollPanel")
           {
               AnchorMinX = 0.17f,
               AnchorMaxX = 0.97f,
               AnchorMinY = 0.82f,
               AnchorMaxY = 0.97f,
           }, RectTransform.Axis.Horizontal, VanillaSprites.MainBGPanelGrey);
        bossSP.ScrollRect.enabled = false;

        ModHelperPanel bossPanel = bossSP.ScrollContent.AddPanel(new Info("BossPanel")
        {
            Height = 200,
            Width = GetBossesWidth(bosses.Count()),
        });

        GridLayoutGroup bossGLG = bossPanel.AddComponent<GridLayoutGroup>();
        bossGLG.spacing = new Vector2(BOSS_SPACING, 0);
        bossGLG.cellSize = new Vector2(BOSS_SIZE, BOSS_SIZE);
        bossGLG.childAlignment = TextAnchor.MiddleCenter;
        bossGLG.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        bossGLG.constraintCount = 1;

        foreach (ModBoss boss in bosses)
        {
            ModBoss b = boss;
            var onIconClicked = new Action(() =>
            {
                var name = b.ToString();

                if (string.IsNullOrEmpty(name))
                    return;

                ToggleAll(c => c.name.StartsWith(name));
            });

            _ = bossPanel
                .AddPanel(new Info(boss.Name))
                .AddButton(new Info("Icon")
                {
                    Size = BOSS_SIZE,
                }, boss.IconReference.GUID, onIconClicked);
        }

        return bossSP;
    }

    private static float GetBossesWidth(int count) => (BOSS_SIZE * count) + (BOSS_SPACING * (count - 1)) + BUTTONS_PADDING;

    #endregion

    #region Buttons

    private const float BUTTONS_PADDING = 100;

    private static readonly List<ModHelperCheckbox> checks = new();

    private static ModHelperScrollPanel Buttons_Init(ModHelperPanel parent, int[] rounds, IEnumerable<ModBoss> bosses)
    {
        ModHelperScrollPanel buttonSP = parent.AddScrollPanel(
            new Info("ButtonsScrollPanel")
            {
                AnchorMinX = 0.17f,
                AnchorMaxX = 0.97f,
                AnchorMinY = 0.05f,
                AnchorMaxY = 0.80f,
            }, null, VanillaSprites.BlueInsertPanelRound); // VanillaSprites.BlueInsertPanelRound
        buttonSP.ScrollRect.vertical = true;
        buttonSP.ScrollRect.horizontal = true;
        buttonSP.ScrollRect.enabled = true;
        buttonSP.ScrollContent.RectTransform.sizeDelta = new Vector2(GetBossesWidth(bosses.Count()), GetRoundsHeight(rounds.Length));

        var amount = 1f / 1.5f;
        buttonSP.Background.color = new UnityEngine.Color(amount, amount, amount);

        ModHelperPanel buttonPanel = buttonSP.ScrollContent.AddPanel(new Info("ButtonPanel")
        {
            SizeDelta = buttonSP.ScrollContent.RectTransform.sizeDelta
        });

        GridLayoutGroup buttonGLG = buttonPanel.AddComponent<GridLayoutGroup>();
        buttonGLG.spacing = new Vector2(BOSS_SPACING, ROUND_SPACING);
        buttonGLG.childAlignment = TextAnchor.LowerCenter;
        buttonGLG.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        buttonGLG.constraintCount = bosses.Count();
        buttonGLG.cellSize = new Vector2(BOSS_SIZE, ROUND_SIZE);
        buttonGLG.startAxis = GridLayoutGroup.Axis.Vertical;

        buttonPanel.transform.localPosition = new Vector3(0, -buttonPanel.RectTransform.sizeDelta.y / 2, 0);

        var counter = 0;

        checks.Clear();

        foreach (ModBoss boss in bosses)
        {
            foreach (var round in rounds)
            {
                if (!boss.GetSpawnRounds().Contains(round))
                {
                    _ = buttonPanel.AddPanel(new Info("Empty")
                    {
                        Size = 150
                    });
                    continue;
                }

                ModBoss b = boss;
                var r = round;
                var i = counter;

                var onValueChanged = new Action<bool>(value =>
                {
                    _ = b.SetPermission(r, value);
                });

                ModHelperCheckbox newCheck = buttonPanel
                    .AddPanel(new Info(boss.Name + round))
                    .AddCheckbox(new Info(boss.ToString() + round)
                    {
                        Size = 150,
                    }, boss.GetPermission(round), VanillaSprites.BlueInsertPanelRound, onValueChanged);

                checks.Add(newCheck);

                counter++;
            }
        }

        return buttonSP;
    }

    private static void ToggleAll(Predicate<ModHelperCheckbox> condition)
    {
        bool? state = null;

        foreach (ModHelperCheckbox item in checks)
        {
            if (!condition.Invoke(item))
                continue;

            if (!state.HasValue)
                state = !item.CurrentValue;

            item.SetChecked(state.Value);
        }
    }

    #endregion
}
