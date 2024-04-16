using BossIntegration.Boss;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.ChallengeEditor;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppNinjaKiwi.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Il2CppTMPro.TMP_Dropdown;

namespace BossIntegration.UI.Menus;

internal class BossesMenu : ModGameMenu<ExtraSettingsScreen>
{
    private static Dictionary<float, string>? bloonsPerSpeed = null;

    private ModBoss? currentBoss;
    private int? currentRound;

    /// <inheritdoc/>
    public override bool OnMenuOpened(Il2CppSystem.Object data)
    {
        this.GameMenu.anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        this.currentBoss = null;
        this.currentRound = null;

        // Get bloons per speed
        if (bloonsPerSpeed == null)
        {
            var bloons = Game.instance.model.bloons
                    .Where(b => b.GetModBloon() == null)
                    .Where(b => b.IsBase)
                    .Where(b => !b.dontShowInSandbox && !b.dontShowInSandboxOnRelease)
                    .Where(b => !b.isBoss)
                    .ToList();

            bloons.Sort((b1, b2) => b1.speed.CompareTo(b2.speed));

            bloonsPerSpeed = new Dictionary<float, string>();

            foreach (BloonModel? item in bloons)
            {
                if (bloonsPerSpeed.ContainsKey(item.speed))
                    continue;

                BTD_Mod_Helper.ModHelper.Msg<BossIntegration>(item.name);
                bloonsPerSpeed.Add(item.speed, item.name);
            }
        }

        this.CommonForegroundHeader.SetText("Bosses");

        this.GameMenu.gameObject.GetComponent<Animator>().enabled = false;

        this.Init(this.GameMenu.gameObject, InGame.instance?.IsInGame() ?? false);

        return false;
    }

    public override void OnMenuClosed() => this.GameMenu.gameObject.GetComponent<Animator>().enabled = true;

    private void Init(GameObject parent, bool isInGame)
    {
        RectTransform panelTransform = parent.GetComponentInChildrenByName<RectTransform>("Panel");
        GameObject panel = panelTransform.gameObject;
        panel.DestroyAllChildren();

        var bosses = Boss.Cache.GetBosses().ToList();
        bosses.Sort((b1, b2) => b1.DisplayName.CompareTo(b2.DisplayName));

        this.UI(panel, 50, bosses, isInGame);

        this.SelectBoss(bosses.ElementAt(0));

        if (isInGame)
        {
            ModHelperPanel bg = this.GameMenu.gameObject.AddModHelperPanel(new Info("ClickBG", InfoPreset.FillParent), "");
            panel.transform.MoveAfterSibling(bg.transform, false);
            bg.GetComponent<Image>().color = new Color(0.0688f, 0.0802f, 0.0882f, 0.498f);
        }
    }

    #region Fields

    private ModHelperImage? bossInfo_Icon;
    private ModHelperText? bossInfo_Name;
    private ModHelperDropdown? bossInfo_Rounds;

    private ModHelperText? bossInfo_Description;

    private (ModHelperPanel panel, ModHelperText text)? bossInfo_ExtraCredits;

    private ModHelperText? bossInfo_IsActive;
    private ModHelperText? bossInfo_Speed;
    private ModHelperText? bossInfo_Health;
    private ModHelperText? bossInfo_Timer;
    private ModHelperPanel? bossInfo_SkullGraph;
    private ModHelperPanel? bossInfo_SkullHolder;

    private ModHelperPanel? bossInfo_Properties;

    #endregion

    #region Components

    private void UI(GameObject parent, float fontSize, IEnumerable<ModBoss> bosses, bool isInGame)
    {
        ModHelperPanel panel = parent.AddModHelperPanel(new Info("BossPanel")
        {
            AnchorMinX = 0.1f,
            AnchorMaxX = 0.9f,
            AnchorMinY = 0.05f,
            AnchorMaxY = 1f
        });

        _ = panel.AddPanel(new Info("BG")
        {
            AnchorMin = Vector2.zero,
            AnchorMax = Vector2.one
        }, VanillaSprites.MainBGPanelBlue);

        this.Bosses(panel, bosses);

        ModHelperPanel bossInfo = panel.AddPanel(new Info("BossInfo")
        {
            AnchorMinX = 0.03f,
            AnchorMaxX = 0.97f,
            AnchorMinY = 0.03f,
            AnchorMaxY = 0.97f,
        });

        this.Header(bossInfo, isInGame);
        this.Content(bossInfo, fontSize);

        this.SelectBoss(bosses.ElementAt(0));
    }

    private void Bosses(ModHelperPanel parent, IEnumerable<ModBoss> bosses)
    {
        const float bossSpacing = 50;
        const float bossSize = 200;

        ModHelperScrollPanel bossListSP = parent.AddScrollPanel(new Info("List")
        {
            AnchorMinX = 0.98f,
            AnchorMaxX = 1.1f,
            AnchorMinY = 0.05f,
            AnchorMaxY = 0.95f,
        }, RectTransform.Axis.Vertical, VanillaSprites.MainBGPanelGrey, 0, Mathf.FloorToInt(bossSize / 4));

        bossListSP.transform.SetAsFirstSibling();

        ModHelperPanel bossPanel = bossListSP.ScrollContent.AddPanel(new Info("Panel")
        {
            Height = (bosses.Count() * bossSize) + ((bosses.Count() - 1) * bossSpacing),
            Width = bossSize,
        });
        VerticalLayoutGroup bossVLG = bossPanel.AddComponent<VerticalLayoutGroup>();
        bossVLG.childAlignment = TextAnchor.MiddleCenter;
        bossVLG.spacing = bossSpacing;

        foreach (ModBoss boss in bosses)
        {
            if (!boss.GetSpawnRounds().Any())
                continue;

            ModBoss b = boss;

            _ = bossPanel.AddButton(new Info(boss.Name)
            {
                Size = bossSize
            }, boss.IconReference.GUID, new System.Action(() => this.SelectBoss(b)));
        }
    }

    private void Header(ModHelperPanel parent, bool isInGame)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("BossHeader")
        {
            AnchorMinX = 0.0f,
            AnchorMaxX = 1.0f,
            AnchorMinY = 0.8f,
            AnchorMaxY = 1.0f,
        });
        HorizontalLayoutGroup bossHeaderHLG = panel.AddComponent<HorizontalLayoutGroup>();
        bossHeaderHLG.childAlignment = TextAnchor.MiddleLeft;

        this.Icon(panel);
        this.Labels(panel);
        this.Rounds(panel, isInGame);
    }
    private void Icon(ModHelperPanel parent) => this.bossInfo_Icon = parent
            .AddPanel(new Info("Icon") { Flex = 1 })
            .AddImage(new Info("Image")
            {
                Size = 300,
            }, default(string));
    private void Labels(ModHelperPanel parent)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("Labels") { Flex = 6 });
        VerticalLayoutGroup bossLabelsVLG = panel.AddComponent<VerticalLayoutGroup>();
        bossLabelsVLG.childAlignment = TextAnchor.MiddleLeft;

        // Name
        this.bossInfo_Name = panel.AddText(new Info("Name", InfoPreset.Flex), "NUH UH", 90, Il2CppTMPro.TextAlignmentOptions.MidlineLeft);
        this.bossInfo_Name.Text.fontStyle = Il2CppTMPro.FontStyles.Bold;

        this.Properties(panel);
    }
    private void Properties(ModHelperPanel parent)
    {
        // List
        this.bossInfo_Properties = parent.AddPanel(new Info("List", InfoPreset.Flex)
        {
            Height = 200,
            AnchorMinX = 0,
            AnchorMaxX = 1,
        });
        HorizontalLayoutGroup pLPHLG = this.bossInfo_Properties.AddComponent<HorizontalLayoutGroup>();
        pLPHLG.childForceExpandWidth = false;
        pLPHLG.childForceExpandHeight = false;
        pLPHLG.childControlHeight = true;
    }

    private void Rounds(ModHelperPanel parent, bool isInGame)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("Rounds") { Flex = 1 });
        VerticalLayoutGroup roundsVLG = panel.AddComponent<VerticalLayoutGroup>();
        roundsVLG.childForceExpandHeight = false;

        // Label
        ModHelperPanel roundLabelPanel = panel.AddPanel(new Info("Label")
        {
            AnchorMinX = 0f,
            AnchorMaxX = 1f,
            Height = 150,
        });

        if (!isInGame)
        {
            var onRoundIconClicked = new System.Action(() =>
            {
                Open<BossesSettings>();
            });

            _ = roundLabelPanel.AddButton(new Info("Icon")
            {
                Size = 100,
                Anchor = new Vector2(0.92f, 0.50f),
            }, VanillaSprites.InfoBtn2, onRoundIconClicked);
        }

        _ = roundLabelPanel.AddText(new Info("Text")
        {
            AnchorMinX = 0f,
            AnchorMinY = 0f,
            AnchorMaxX = isInGame ? 1f : 0.9f,
            AnchorMaxY = 1f,
        }, "Rounds", 69);

        // Dropdown
        var rounds = new Il2CppSystem.Collections.Generic.List<string>();
        rounds.Add("TEMP1");
        rounds.Add("TEMP1");
        rounds.Add("TEMP1");

        var onValueChanged = new System.Action<int>(r =>
        {
            if (this.bossInfo_Rounds == null)
                return;

            if (!int.TryParse(this.bossInfo_Rounds.Dropdown.options[r].text, out var index))
                return;

            this.SelectRound(index);
        });

        this.bossInfo_Rounds = panel.AddDropdown(new Info("Rounds")
        {
            Width = 400,
            Height = 150
        }, rounds, 150 * 4, onValueChanged, VanillaSprites.BlueInsertPanelRound);
    }

    private void Content(ModHelperPanel parent, float fontSize)
    {
        ModHelperPanel bossContent = parent.AddPanel(new Info("BossContent")
        {
            AnchorMinX = 0.0f,
            AnchorMaxX = 1.0f,
            AnchorMinY = 0.0f,
            AnchorMaxY = 0.77f,
        });

        this.Content_One(bossContent, fontSize);
        this.Content_Two(bossContent);
    }

    private void Content_One(ModHelperPanel parent, float fontSize)
    {
        ModHelperPanel content = parent.AddPanel(new Info("Content1")
        {
            AnchorMinX = 0.0f,
            AnchorMaxX = 0.35f,
            AnchorMinY = 0.0f,
            AnchorMaxY = 1.0f,
        });
        VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.childAlignment = TextAnchor.UpperLeft;
        contentVLG.childForceExpandHeight = false;
        contentVLG.spacing = 10;

        this.Description(content, fontSize);
        this.ExtraCredits(content, fontSize);
    }
    private void Description(ModHelperPanel parent, float fontSize)
    {
        // Label "Description"
        _ = parent.AddText(new Info("Description-Label")
        {
            Height = fontSize
        }, "Description:", fontSize, Il2CppTMPro.TextAlignmentOptions.Left);

        // Description
        ModHelperScrollPanel scrollPanel = parent.AddScrollPanel(new Info("DescriptionPanel", InfoPreset.FillParent)
        {
            FlexWidth = 1,
            FlexHeight = 1
        }, RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanelRound, 50, 25);

        this.bossInfo_Description = scrollPanel.AddText(new Info("Description")
        {
            Width = 1000,
            Height = 1000,
            Flex = 1
        }, "No description provided", fontSize * 1.5f, Il2CppTMPro.TextAlignmentOptions.TopLeft);
        // this.bossInfo_Description.Text.overflowMode = Il2CppTMPro.TextOverflowModes.Overflow;

        scrollPanel.AddScrollContent(this.bossInfo_Description);

    }
    private void ExtraCredits(ModHelperPanel parent, float fontSize)
    {
        ModHelperPanel panel = parent.AddPanel(new Info("ExtraCredits")
        {
            Height = (fontSize * 2) + (50 * 2) + fontSize,
            AnchorMinX = 0.1f,
            AnchorMaxX = 0.9f,
        });
        _ = panel.AddComponent<VerticalLayoutGroup>();

        _ = panel.AddText(new Info("ExtraCredits-Label")
        {
            Height = fontSize,
            AnchorMinX = 0,
            AnchorMaxX = 1,
        }, "Extra Credits:", fontSize, Il2CppTMPro.TextAlignmentOptions.Left);

        // Extra credits
        ModHelperScrollPanel scrollPanel = panel.AddScrollPanel(new Info("ExtraCreditsPanel")
        {
            AnchorMinX = 0,
            AnchorMaxX = 1,
            Height = (fontSize * 2) + (50 * 2),
        }, RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanelRound, 50, 25);

        this.bossInfo_ExtraCredits = (panel, scrollPanel.ScrollContent.AddText(new Info("ExtraCredits")
        {
            Width = 1000,
            Height = 150,
            Flex = 1,
        }, "No extra credit provided", fontSize, Il2CppTMPro.TextAlignmentOptions.MidlineLeft));
    }

    private void Content_Two(ModHelperPanel parent)
    {
        ModHelperPanel content = parent.AddPanel(new Info("Content2")
        {
            AnchorMinX = 0.4f,
            AnchorMaxX = 1.0f,
            AnchorMinY = 0.0f,
            AnchorMaxY = 1.0f,
        });
        VerticalLayoutGroup contentVLG = content.AddComponent<VerticalLayoutGroup>();
        contentVLG.childAlignment = TextAnchor.UpperLeft;
        contentVLG.childForceExpandHeight = false;

        this.SkullsGraph(content);
        this.Stats(content, 69);
    }
    private void Stats(ModHelperPanel parent, float fontSize)
    {
        // Label "Stats"
        _ = parent.AddText(new Info("Stats-Label")
        {
            Height = 50
        }, "Stats:", 50, Il2CppTMPro.TextAlignmentOptions.Left);

        _ = parent.AddPanel(new Info("Spacing", 10));

        ModHelperScrollPanel statsSP = parent.AddScrollPanel(new Info("Stats")
        {
            FlexWidth = 1,
            FlexHeight = 1
        }, RectTransform.Axis.Vertical, VanillaSprites.BlueInsertPanelRound, 0, 30);
        statsSP.ScrollContent.LayoutGroup.spacing = 30;

        this.IsActive(statsSP.ScrollContent, fontSize);
        this.Speed(statsSP.ScrollContent, fontSize);
        this.Health(statsSP.ScrollContent, fontSize);
        this.Timer(statsSP.ScrollContent, fontSize);
    }
    private void IsActive(ModHelperPanel parent, float fontSize)
    {
        this.bossInfo_IsActive = parent.AddText(new Info("Active")
        {
            Height = fontSize,
            FlexWidth = 1,
        }, "Is Active: NUH UH", fontSize, Il2CppTMPro.TextAlignmentOptions.TopLeft);

        this.bossInfo_IsActive.SetActive(false);
    }
    private void Speed(ModHelperPanel parent, float fontSize) => this.bossInfo_Speed = parent.AddText(new Info("Speed")
    {
        Height = fontSize,
        FlexWidth = 1,
    }, "speed: NaN", fontSize, Il2CppTMPro.TextAlignmentOptions.TopLeft);
    private void Health(ModHelperPanel parent, float fontSize) => this.bossInfo_Health = parent.AddText(new Info("Health")
    {
        Height = fontSize,
        FlexWidth = 1,
    }, "Health: NaN", fontSize, Il2CppTMPro.TextAlignmentOptions.TopLeft);
    private void Timer(ModHelperPanel parent, float fontSize) => this.bossInfo_Timer = parent.AddText(new Info("Timer")
    {
        Height = fontSize,
        FlexWidth = 1,
    }, "Timer (NaN): NUH UH", fontSize, Il2CppTMPro.TextAlignmentOptions.TopLeft);

    private void SkullsGraph(ModHelperPanel parent)
    {
        this.bossInfo_SkullGraph = parent.AddPanel(new Info("SkullGraph")
        {
            Height = 200 + 50 + 75,
            AnchorMinX = 0,
            AnchorMaxX = 1
        });

        _ = this.bossInfo_SkullGraph.AddComponent<VerticalLayoutGroup>();

        _ = this.bossInfo_SkullGraph.AddText(new Info("Label")
        {
            Height = 50,
            AnchorMinX = 0,
            AnchorMaxX = 1
        }, "Skulls:", 50, Il2CppTMPro.TextAlignmentOptions.Left);

        ModHelperPanel panel = this.bossInfo_SkullGraph.AddPanel(new Info("Panel")
        {
            Height = 200,
            AnchorMinX = 0,
            AnchorMaxX = 1
        });

        _ = DefaultHealthUI.HealthBar(panel);
        DefaultHealthUI.Frame(panel);

        this.bossInfo_SkullHolder = panel.AddPanel(new Info("SkullHolder")
        {
            AnchorMinX = 0,
            AnchorMaxX = 1,
            AnchorMinY = 0,
            AnchorMaxY = 0,
        });

        _ = this.bossInfo_SkullGraph.AddPanel(new Info("Spacing", 50));
    }

    #endregion

    #region Update UI

    private void SelectBoss(ModBoss boss)
    {
        this.currentBoss = boss;

        this.bossInfo_Icon?.Image.SetSprite(boss.IconReference);
        this.CommonForegroundHeader.SetText(boss.DisplayName);

        this.bossInfo_Name?.SetText(boss.DisplayName);

        this.UpdateRound(boss);
        this.UpdateExtraCredits(boss);

        this.bossInfo_Description?.SetText(boss.Description);
    }

    private void SelectRound(int round)
    {
        this.currentRound = round;

        if (this.currentBoss == null)
            return;

        BloonModel model = Game.instance.model.GetBloon(this.currentBoss.Id).Duplicate();
        this.currentBoss.ModifyForRound(model, this.currentRound.Value);

        this.bossInfo_IsActive?.SetText("Is Active: " + this.currentBoss.GetPermission(round));
        this.UpdateSpeed(model);
        this.bossInfo_Health?.SetText("Health: " + ((float)model.maxHealth).Format().ToString());

        // Properties
        if (this.bossInfo_Properties != null)
        {
            var propertiesCount = this.UpdateProperties(this.bossInfo_Properties, model);
            this.bossInfo_Properties.SetActive(propertiesCount > 0);

            if (this.bossInfo_Name != null)
            {
                this.bossInfo_Name.Text.alignment = this.bossInfo_Properties.isActiveAndEnabled 
                    ? Il2CppTMPro.TextAlignmentOptions.BottomLeft
                    : Il2CppTMPro.TextAlignmentOptions.MidlineLeft;
            }
        }

        // BossRoundInfo
        BossRoundInfo infos = this.currentBoss.RoundsInfo[round];

        this.bossInfo_Timer?.SetText($"Timer ({infos.interval}s): {infos.timerDescription ?? this.currentBoss.TimerDescription}");
        this.bossInfo_Timer?.SetActive(infos.interval.HasValue);

        if (this.bossInfo_SkullHolder != null)
        {
            this.bossInfo_SkullHolder.transform.DestroyAllChildren();

            var values = this.currentBoss.RoundsInfo[this.currentRound.Value].GetSkullsPosition(this.currentBoss);

            for (var i = 0; i < values.Length; i++)
            {
                var index = i;
                var item = values[i];

                _ = this.bossInfo_SkullHolder.AddButton(
                    new Info("BTN", InfoPreset.FillParent)
                    {
                        AnchorMinX = item,
                        AnchorMaxX = item,
                        Y = 0,
                        Size = 150,
                    },
                    ModContent.GetTextureGUID<BossIntegration>("BossSkullPipOff"),
                    new System.Action(() =>
                    {
                        PopupScreen.instance.SafelyQueue(screen =>
                        {
                            _ = screen.ShowOkPopup(this.currentBoss.GetSkullDescription(this.currentRound.Value, index));
                        });
                        BTD_Mod_Helper.ModHelper.Msg<BossIntegration>("A");
                    }));
            }

            this.bossInfo_SkullGraph?.SetActive(values.Length > 0);
        }
    }

    private void UpdateRound(ModBoss boss)
    {
        if (this.bossInfo_Rounds == null)
            return;

        var index = -1;

        var rounds = new Il2CppSystem.Collections.Generic.List<OptionData>();
        var spawnRounds = boss.GetSpawnRounds().ToList();
        spawnRounds.Sort();

        // Keep the same round
        if (this.currentRound.HasValue)
            index = spawnRounds.IndexOf(this.currentRound.Value);

        this.bossInfo_Rounds.Dropdown.ClearOptions();

        foreach (var item in spawnRounds)
            rounds.Add(new OptionData(item.ToString()));

        this.bossInfo_Rounds.Dropdown.AddOptions(rounds);
        this.bossInfo_Rounds.Dropdown.Select();

        if (index <= 0)
            index = 0;

        this.bossInfo_Rounds.Dropdown.value = index;
        this.bossInfo_Rounds.GetComponent<Il2CppTMPro.TMP_Dropdown>().interactable = spawnRounds.Count > 1;
        this.SelectRound(spawnRounds[index]);
    }
    private void UpdateSpeed(BloonModel model)
    {
        if (this.bossInfo_Speed == null || bloonsPerSpeed == null || !this.currentRound.HasValue)
            return;

        var lastSpeed = bloonsPerSpeed.Keys.ElementAt(0);

        // Find the closest speed
        for (var i = 1; i < bloonsPerSpeed.Count; i++)
        {
            var speed = bloonsPerSpeed.Keys.ElementAt(i);

            if (speed > model.speed)
                break;

            lastSpeed = speed;
        }

        // Show the speed
        this.bossInfo_Speed.SetText($"Speed: {model.speed / lastSpeed * 100:F2}% of a {bloonsPerSpeed[lastSpeed]}");
    }
    private int UpdateProperties(ModHelperPanel parent, BloonModel model)
    {
        parent.transform.DestroyAllChildren();

        // "This boss has Lead properties."
        var buttons = new Dictionary<BloonProperties, string>()
        {
            [BloonProperties.Lead] = VanillaSprites.LeadBloonIcon,
            [BloonProperties.Black] = VanillaSprites.BlackBloonIcon,
            [BloonProperties.White] = VanillaSprites.WhiteBloonIcon,
            [BloonProperties.Purple] = VanillaSprites.PurpleBloonIcon,
            [BloonProperties.Frozen] = VanillaSprites.AbsoluteZeroUpgradeIcon,
        };

        var counter = 0;

        foreach (BloonProperties property in System.Enum.GetValues(typeof(BloonProperties)))
        {
            if (property == BloonProperties.None)
                continue;

            if (!model.bloonProperties.HasFlag(property))
                continue;

            var text = $"{this.currentBoss?.DisplayName ?? "This boss"} has {property} properties";

            _ = parent.AddButton(new Info("Btn")
            {
                Size = 150
            }, buttons[property], new System.Action(() => ShowPropertyPopup(text)));
            counter++;
        }

        return counter;
    }
    private void UpdateExtraCredits(ModBoss boss)
    {
        if (!this.bossInfo_ExtraCredits.HasValue)
            return;

        this.bossInfo_ExtraCredits.Value.text.SetText(boss.ExtraCredits);
        this.bossInfo_ExtraCredits.Value.panel.SetActive(boss.ExtraCredits.Trim().Length > 0);
    }

    #endregion

    #region Other

    private static void ShowPropertyPopup(string message) => PopupScreen.instance.SafelyQueue(screen => screen.ShowOkPopup(message));

    #endregion
}