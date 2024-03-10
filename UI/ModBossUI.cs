using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppNinjaKiwi.Common;
using Il2CppSystem.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static BossIntegration.ModBoss;

namespace BossIntegration.UI;

internal class ModBossUI
{
    private const int MAX_LINES = 3;

    private static ModHelperPanel? MainPanel { get; set; }

    private static ModHelperPanel? WaitHolder { get; set; }

    private static Dictionary<int, List<ModBoss>>? Rounds = null;

    internal static void Init()
    {
        MainPanel_Init();
        Rounds_Init();
        WaitPanels_Init();
    }

    #region Initialization

    private static void MainPanel_Init()
    {
        // If the parent exists, reset everything
        if (MainPanel != null)
        {
            MainPanel.DeleteObject();
            WaitHolder = null;
            ModBoss.ClearBosses();
        }

        // Create parent
        MainPanel = InGame.instance.mapRect.gameObject.AddModHelperPanel(
            new Info("BossUIPanel",
                -400, -25,
                800, 600,
                new Vector2(0.65f, .85f),
                new Vector2(.5f, .5f)),
            null);
        _ = MainPanel.AddComponent<ContentSizeFitter>();

        VerticalLayoutGroup mainPanelVLG = MainPanel.AddComponent<VerticalLayoutGroup>();
        mainPanelVLG.spacing = 300;
        mainPanelVLG.childControlHeight = false;

        // Prevent the UIs to catch raycasts
        CanvasGroup cg = MainPanel.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Place the UIs under every other UIs
        MainPanel.transform.SetAsFirstSibling();
    }

    private static void Rounds_Init()
    {
        // Skip if already initialized
        if (Rounds != null)
            return;

        // Get every boss for every round
        Rounds = new Dictionary<int, List<ModBoss>>();

        foreach (ModBoss boss in ModBoss.GetBosses())
        {
            foreach (var round in boss.SpawnRounds)
            {
                if (!Rounds.ContainsKey(round))
                    Rounds[round] = new List<ModBoss>();
                Rounds[round].Add(boss);
            }
        }
    }

    private static void WaitPanels_Init()
    {
        if (MainPanel == null)
            return;

        // Create parent if doesn't exist
        if (WaitHolder == null)
        {
            WaitHolder = MainPanel.AddPanel(new Info("WaitHolder"));
            VerticalLayoutGroup vlg = WaitHolder.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 25;
        }

        // Update the waiting panels
        UpdateWaitPanels();
    }

    #endregion

    #region Update

    public static void Update()
    {
        UpdateWaitPanels();
        UpdateHealthPanels();
    }

    /// <summary>
    /// Updates the Wait Panel
    /// </summary>
    private static void UpdateWaitPanels(int? currentRound = null)
    {
        // If the parent is not set, skip
        if (WaitHolder == null)
            return;

        // Destroy all children
        WaitHolder.transform.DestroyAllChildren();

        // If the rounds are not set, skip
        if (Rounds == null)
            return;

        // If not round given, set to the current round
        if (!currentRound.HasValue)
            currentRound = InGame.Bridge.GetCurrentRound() + 1;

        // Find the closest round
        var closestRound = int.MaxValue;
        var hasChanged = false;

        foreach ((var round, List<ModBoss> b) in Rounds)
        {
            if (round <= currentRound || !b.Any(boss => ModBoss.GetPermission(boss, round)))
                continue;

            if (round - currentRound >= closestRound)
                continue;

            closestRound = round - currentRound.Value;
            hasChanged = true;
        }

        // If all rounds have passed, skip
        if (!hasChanged)
            return;

        // Get all bosses of round
        var key = closestRound + currentRound.Value;
        List<ModBoss> bosses = Rounds[key];

        // Remove all the bosses that can't spawn
        _ = bosses.RemoveAll(boss => !ModBoss.GetPermission(boss, key));

        var uiAmount = MAX_LINES;

        // If any boss uses the default waiting UI
        if (bosses.Any(boss => boss.UsingDefaultWaitingUi))
        {
            _ = DefaultWaitUI.Create(
                WaitHolder,
                bosses.Where(b => b.UsingDefaultWaitingUi).ToArray(),
                closestRound);
            uiAmount--;
        }

        // Find all the bosses that use a custom UI
        List<ModBoss> customUiBosses = bosses.FindAll(boss => !boss.UsingDefaultWaitingUi);

        // Take the lowest number of UI
        uiAmount = Mathf.Min(uiAmount, customUiBosses.Count);

        // Add every waiting panels
        for (var i = 0; i < uiAmount; i++)
            customUiBosses[i].AddWaitPanel(WaitHolder);
    }

    private static void UpdateHealthPanels()
    {
        var copy = ModBoss.BossesAlive.Keys.ToList();
        foreach (BloonToSimulation? item in InGame.instance.GetAllBloonToSim())
            _ = copy.Remove(item.GetBloon().Id);

        foreach (Il2CppAssets.Scripts.ObjectId item in copy)
            ModBoss.RemoveUI(item);
    }

    public static bool HasHealthPanel(ModBoss boss)
        => MainPanel != null && MainPanel.transform.Find(GetHealthKey(boss)) == null;

    #endregion

    public static void AddHealthPanel(ModBoss boss, Bloon bloon, ref BossUI ui, int round)
    {
        if (MainPanel == null)
            return;

        ModHelperPanel? panel = boss.AddBossPanel(MainPanel, bloon, ref ui, round);

        if (panel == null)
            return;

        ui.Panel = panel;

        panel.gameObject.name = GetHealthKey(boss);
    }

    private static string? GetHealthKey(ModBoss boss) => boss.GetType().FullName;
}