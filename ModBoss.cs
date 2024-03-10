// Adapted from WarperSan's BossPack 

using BossIntegration.UI;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Bloons;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace BossIntegration;

/// <summary>
/// Class for adding a new boss to the game
/// </summary>
public abstract class ModBoss : ModBloon
{
    #region Virtuals

    /// <summary>
    /// Called when the boss is spawned
    /// </summary>
    /// <param name="bloon"></param>
    public virtual void OnSpawn(Bloon bloon) { }

    /// <summary>
    /// Called when the boss is leaked
    /// </summary>
    /// <param name="bloon"></param>
    public virtual void OnLeak(Bloon bloon) { }

    /// <summary>
    /// Called when the boss is popped
    /// </summary>
    /// <param name="bloon"></param>
    public virtual void OnPop(Bloon bloon) { }

    /// <summary>
    /// Called when the boss takes a damage
    /// </summary>
    /// <param name="bloon"></param>
    /// <param name="totalAmount"></param>
    public virtual void OnDamage(Bloon bloon, float totalAmount) { }

    #endregion

    // --- MANAGEMENT ---

    #region Cache

    private static readonly Dictionary<string, ModBoss> Cache = new();

    public static int Count => Cache.Count;
    public static bool HasBosses => Count > 0;

    private bool RegisterBoss(BloonModel model)
    {
        var key = model.name;

        if (TryGetBoss(key, out _))
            return false;

        Cache.Add(key, this);

        return true;
    }

    #endregion Cache

    #region Bosses Alive

    public static Dictionary<ObjectId, BossUI> BossesAlive = new();

    public static bool AnyBossAlive => BossesAlive.Count != 0;

    private static void AddBossBloon(Bloon bloon, BossUI ui) => BossesAlive.Add(bloon.Id, ui);

    internal static void ClearBosses() => BossesAlive.Clear();

    internal static void RemoveUI(ObjectId id)
    {
        if (!BossesAlive.ContainsKey(id))
            return;

        BossUI ui = BossesAlive[id];

        ui.Panel?.DeleteObject();

        _ = BossesAlive.Remove(id);
    }

    #endregion Bosses Alive

    #region Permissions

    private static string Permission_Path => ModHelper.ModHelperDirectory + "\\Mod Settings\\BossesSetting.json";

    public static Dictionary<string, Dictionary<int, bool>> Permissions = InitPermissions();

    public static bool GetPermission(ModBoss boss, int round)
    {
        var key = boss.ToString();

        return key != null &&
            Permissions.TryGetValue(key, out Dictionary<int, bool>? permissions) &&
            permissions.TryGetValue(round, out var isAllowed) && isAllowed;
    }

    public static bool SetPermission(ModBoss boss, int round, bool value)
    {
        var key = boss.ToString();

        if (key == null)
            return false;

        Dictionary<int, bool> values = Permissions.ContainsKey(key) ? Permissions[key] : new();
        values[round] = value;
        Permissions[key] = values;
        return true;
    }

    private static Dictionary<string, Dictionary<int, bool>> InitPermissions()
    {
        var permissions = new Dictionary<string, Dictionary<int, bool>>();

        var path = Permission_Path;

        if (!File.Exists(path))
            return permissions;

        var json = JObject.Parse(File.ReadAllText(path));

        foreach ((var name, JToken? token) in json)
        {
            if (token == null)
                continue;

            Dictionary<int, bool>? p = token.ToObject<Dictionary<int, bool>>();

            if (p != null)
                permissions.Add(name, p);
        }

        return permissions;
    }

    public static void SavePermissions()
    {
        // Clean the permissions
        for (var i = Permissions.Count - 1; i >= 0; i--)
        {
            var key = Permissions.Keys.ElementAt(i);

            for (var j = Permissions[key].Count - 1; j >= 0; j--)
            {
                var key2 = Permissions[key].Keys.ElementAt(j);

                if (!Permissions[key][key2])
                    _ = Permissions[key].Remove(key2);
            }

            // Remove key if empty
            if (Permissions[key].Count == 0)
                _ = Permissions.Remove(key);
        }

        // Save it
        File.WriteAllText(Permission_Path, JsonConvert.SerializeObject(Permissions));

        ModHelper.Msg<BossIntegration>("Boss Rounds Saved !");
    }

    #endregion Permissions

    #region Static

    /// <returns>Is a boss associated with the given key?</returns>
    private static bool TryGetBoss(string key, out ModBoss? boss)
        => Cache.TryGetValue(key, out boss);

    /// <returns>Is the given bloon a registered boss?</returns>
    public static bool TryGetBoss(Bloon bloon, out ModBoss? boss)
        => TryGetBoss(bloon.bloonModel.id, out boss);

    /// <returns>Is the given bloon a registered boss that is still alive?</returns>
    public static bool IsAliveBoss(Bloon bloon, out ModBoss? boss)
        => TryGetBoss(bloon, out boss) && BossesAlive.ContainsKey(bloon.Id);

    /// <returns>All the bosses that will spawn for the given round</returns>
    public static IEnumerable<ModBoss> GetBossesForRound(int round)
        => GetBosses(b => b.SpawnRounds.Contains(round));

    /// <returns>All the bosses that matches the given condition</returns>
    public static IEnumerable<ModBoss> GetBosses(Func<ModBoss, bool> condition)
    {
        List<ModBoss> bosses = new();

        foreach ((var _, ModBoss boss) in Cache)
        {
            if (condition.Invoke(boss))
                bosses.Add(boss);
        }

        return bosses;
    }

    /// <returns>All the bosses</returns>
    public static IEnumerable<ModBoss> GetBosses()
        => GetBosses(_ => true);

    #endregion Static

    // --- STATS ---

    #region ModBloon

    /// <summary>
    /// The base speed of the boss, 4.5 is the default for a BAD and 25 is the default for a red bloon
    /// </summary>
    public virtual float Speed => 4.5f;

    /// <summary>
    /// The base health of the boss
    /// </summary>
    public virtual float Health => 20_000f;

    /// <inheritdoc/>
    public override void ModifyBaseBloonModel(BloonModel bloonModel)
    {
        bloonModel.RemoveAllChildren();
        bloonModel.danger = 16;
        bloonModel.overlayClass = BloonOverlayClass.Dreadbloon;
        bloonModel.bloonProperties = BloonProperties.None;
        bloonModel.tags = new Il2CppStringArray(new[] { "Bad", "Moabs", "Boss" });
        bloonModel.maxHealth = (int)this.Health;
        bloonModel.speed = this.Speed;
        bloonModel.isBoss = true;

        if (!bloonModel.HasBehavior<HealthPercentTriggerModel>())
        {
            bloonModel.AddBehavior(new HealthPercentTriggerModel(this.Name + "-SkullEffect", false, System.Array.Empty<float>(), new string[] { this.Name + "SkullEffect" }, false));
        }

        _ = this.RegisterBoss(bloonModel);
    }

    /// <inheritdoc />
    public override void Register()
    {
        if (this.RoundsInfo.Count == 0)
            return;

        base.Register();
    }

    /// <inheritdoc />
    public sealed override string BaseBloon => BloonType.Bad;

    /// <inheritdoc />
    public sealed override bool KeepBaseId => false;

    /// <inheritdoc />
    public sealed override bool Regrow => false;

    /// <inheritdoc />
    public sealed override string RegrowsTo => "";

    /// <inheritdoc />
    public sealed override float RegrowRate => 3;

    #endregion ModBloon

    #region ModBoss

    /// <summary>
    /// Determines if the boss causes a defeat when it leaks
    /// </summary>
    public virtual bool DefeatOnLeak => true;

    /// <summary>
    /// Determines if the boss blocks the rounds from spawning. 
    /// If set to true, the boss will have to be defeated before the next round starts
    /// </summary>
    public virtual bool BlockRounds => false;

    /// <summary>
    /// Determines the quote of the boss in the UI
    /// </summary>
    // public virtual string Quote => "";

    /// <summary>
    /// Modifies the boss before it is spawned, based on the round
    /// </summary>
    public virtual BloonModel ModifyForRound(BloonModel bloon, int round) => bloon;

    #endregion

    #region BossRoundInfo

    /// <summary>
    /// Informations about the boss on the round
    /// </summary>
    public abstract Dictionary<int, BossRoundInfo> RoundsInfo { get; }

    /// <summary>
    /// The rounds the boss should spawn on
    /// </summary>
    public IEnumerable<int> SpawnRounds => this.RoundsInfo.Keys;

    /// <summary>
    /// All the informations a boss holds for a specific round
    /// </summary>
    public struct BossRoundInfo
    {
        /// <summary>
        /// Tier of the boss on this round
        /// </summary>
        public uint? tier = null;

        /// <summary>
        /// Amount of skulls the boss has
        /// </summary>
        public uint? skullCount = null;

        /// <summary>
        /// Positions of the skulls
        /// </summary>
        /// <remarks>
        /// If not specified, the skulls' position will be placed evenly (3 skulls => 0.75, 0.5, 0.25)
        /// </remarks>
        public float[]? percentageValues = null;

        /// <summary>
        /// The description of this particular skull effect
        /// </summary>
        /// <remarks>
        /// If not specified, the API will use <see cref="SkullDescription"/>
        /// </remarks>
        public string? skullDescription = null;

        /// <summary>
        /// Determines if the boss's health should go down while it's skull effect is on 
        /// </summary>
        /// <remarks>
        /// Sets this value: <see cref="HealthPercentTriggerModel.preventFallthrough"/>
        /// </remarks>
        public bool? preventFallThrough = null;

        /// <summary>
        /// Determines if the timer starts immediately
        /// </summary>
        /// <remarks>
        /// Sets this value: <see cref="TimeTriggerModel.triggerImmediately"/>
        /// </remarks>
        public bool? triggerImmediately = null;

        /// <summary>
        /// Interval between ticks
        /// </summary>
        /// <remarks>
        /// Sets this value: <see cref="TimeTriggerModel.interval"/>
        /// </remarks>
        public float? interval = null;

        /// <summary>
        /// The description of this particualr timer effect
        /// </summary>
        /// <remarks>
        /// If not specified, the API will use <see cref="TimerDescription"/>
        /// </remarks>
        public string? timerDescription = null;

        /// <summary>
        /// Determines if the player will lose if, when this boss spawns, 
        /// another version of it is alive
        /// </summary>
        public bool? allowDuplicate = null;

        public BossRoundInfo() { }
    }

    public static uint? GetTier(Bloon bloon)
    {
        BossRoundInfo? info = GetRoundInfo(bloon);

        return info.HasValue ? info.Value.tier : null;
    }

    public static BossRoundInfo? GetRoundInfo(Bloon boss) => BossesAlive.ContainsKey(boss.Id) ? BossesAlive[boss.Id].RoundInfo : null;

    #endregion

    #region Timer

    /// <summary>
    /// The description of the timer effect
    /// </summary>
    public virtual string TimerDescription => "???";

    /// <summary>
    /// Interval between ticks
    /// </summary>
    /// <remarks>
    /// If <see cref="BossRoundInfo.interval"/> is not set, this value will be used instead
    /// </remarks>
    public virtual uint? Interval => null;

    /// <summary>
    /// Checks if the boss uses a timer
    /// </summary>
    public bool UsesTimer => this.Interval != null || this.RoundsInfo.Any(info => info.Value.interval != null);

    /// <summary>
    /// Called when the boss timer ticks
    /// </summary>
    public virtual void TimerTick(Bloon boss) { }

    #endregion Timer

    #region Skulls

    /// <summary>
    /// The description of the skull effect
    /// </summary>
    public virtual string SkullDescription => "???";

    /// <summary>
    /// Checks if the boss has any skull
    /// </summary>
    public bool UsesSkulls => this.RoundsInfo.Any(info => info.Value.skullCount > 0);

    /// <summary>
    /// Called when the boss hits a skull
    /// </summary>
    public virtual void SkullEffect(Bloon boss) { }

    /// <summary>
    /// Called when the boss should get a skull remove
    /// </summary>
    public virtual void SkullEffectUI(Bloon boss)
    {
        if (BossesAlive[boss.Id].Skulls.Count != 0)
            BossesAlive[boss.Id].Skulls.First(img => img != null).DeleteObject();
    }

    #endregion Skulls

    #region UI

    public struct BossUI
    {
        public BossRoundInfo RoundInfo;
        public ModHelperPanel Panel;
        public Image HpBar;
        public ModHelperText HpText;
        public List<ModHelperImage> Skulls;
    }

    private static void SetHP(float health, float maxHealth, ModHelperText hpText)
        => hpText.SetText((bool)BossIntegration.FormatBossHP.GetValue()
            ? $"{FormatNumber(health)} / {FormatNumber(maxHealth)}"
            : $"{health} / {maxHealth}");

    #endregion UI

    #region Health Bar UI

    /// <summary>
    /// Creates the panel for the boss health UI and registers the BossUI components
    /// </summary>
    /// <remarks>
    /// Here are the BossUI components you need to register:
    /// <list type="bullet">
    ///     <item>HP Text</item>
    ///     <item>HP Slider Bar</item>
    ///     <item>List of Skulls</item>
    ///     <item>The panel itself</item>
    /// </list>
    /// </remarks>
    /// <param name="holderPanel"></param>
    /// <param name="bloon"></param>
    /// <param name="ui"></param>
    public virtual ModHelperPanel? AddBossPanel(ModHelperPanel holderPanel, Bloon bloon, ref BossUI ui, int round)
        => DefaultHealthUI.Create(this, holderPanel, bloon.bloonModel, ref ui, round);

    private static readonly string[] NumSuffixs = new string[] { "K", "M", "B", "T", "q", "Q", "s", "S", "O", "N", "d", "U", "D", "!", "@", "#", "$", "%", "^", "&", "*", "[", "]", "{", "}", ";" };
    internal static string FormatNumber(double number)
    {
        var result = number.ToString().Split(',')[0];

        if (result.Length < 4)
            return result;

        var index = 0;
        while (result.Length - (3 * (index + 1)) > 3)
        {
            index++;
        }

        var commaPos = result.Length - (3 * (index + 1));
        var rest = result[commaPos..4];

        while (rest.Substring(rest.Length - 1, 1) == "0")
        {
            if (rest.Length == 1)
            {
                rest = "";
                break;
            }

            rest = rest[..^1];
        }

        return result[..commaPos] + (rest.Length == 0 ? "" : ",") + rest + (index >= NumSuffixs.Length ? "?" : NumSuffixs[index]);
    }

    #endregion Health Bar UI

    #region Wait UI

    /// <summary>
    /// Defines if the boss is using the default waiting UI
    /// </summary>
    public virtual bool UsingDefaultWaitingUi => true;

    /// <summary>
    /// Creates the panel that shows "Boss appears in X rounds".
    /// </summary>
    /// <remarks>
    /// You must set <see cref="UsingDefaultWaitingUi"/> to true if you want to use this.
    /// </remarks>
    public virtual void AddWaitPanel(ModHelperPanel waitingHolderPanel) => ModHelper.Error<BossIntegration>($"'{this}' must override the method '{nameof(AddWaitPanel)}' since '{nameof(UsingDefaultWaitingUi)}' is set to true.");

    #endregion

    #region Boss Info UI

    /// <summary>
    /// The people who worked/helped to create the mod, but aren't the authors
    /// </summary>
    public virtual string ExtraCredits => string.Empty;

    #endregion Boss Info UI

    #region Spawn

    internal void Spawn(int round)
    {
        BloonModel bossModel = this.ModifyForRound(Game.instance.model.GetBloon(this.Id), round);
        Bloon bloon = InGame.instance.GetMap().spawner.Emit(bossModel, 0, 0);
        bloon.bloonModel.speedFrames = bossModel.speed * 0.416667f / 25;

        BossRoundInfo info = this.RoundsInfo[round];

        // No duplicates
        this.Spawn_PreventDuplicates(info);

        // Skulls
        this.Spawn_Skulls(info, bloon.bloonModel);

        // Timer
        this.Spawn_Timer(info, bloon);

        // UI infos
        var ui = new BossUI()
        {
            RoundInfo = info,
        };

        ModBossUI.AddHealthPanel(this, bloon, ref ui, round);

        AddBossBloon(bloon, ui);
        
        this.Damage_UpdateHealthHP(bloon);
        this.OnSpawn(bloon);
    }

    private void Spawn_PreventDuplicates(BossRoundInfo info)
    {
        if (!info.allowDuplicate.HasValue || info.allowDuplicate.Value)
            return;

        if (!ModBossUI.HasHealthPanel(this))
            return;

        KillPlayer();
    }
    private void Spawn_Skulls(BossRoundInfo info, BloonModel model)
    {
        if (info.skullCount == null || !model.HasBehavior<HealthPercentTriggerModel>())
            return;

        // Puts default skulls placement
        if (info.percentageValues == null)
        {
            var skullsCount = (uint)info.skullCount;

            var pV = new List<float>();

            if (skullsCount > 0)
            {
                for (var i = 1; i <= skullsCount; i++)
                {
                    pV.Add(1f - (1f / (skullsCount + 1) * i));
                }
            }

            info.percentageValues = pV.ToArray();
        }

        HealthPercentTriggerModel bossSkulls = model.GetBehavior<HealthPercentTriggerModel>();
        bossSkulls.percentageValues = info.percentageValues;
        bossSkulls.preventFallthrough = info.preventFallThrough != null && (bool)info.preventFallThrough;
    }
    private void Spawn_Timer(BossRoundInfo info, Bloon bloon)
    {
        if (bloon.bloonModel.HasBehavior<TimeTriggerModel>())
            bloon.bloonModel.RemoveBehaviors<TimeTriggerModel>();

        if (info.interval != null || this.Interval != null)
        {
            var timer = new TimeTriggerModel(this.Name + "-TimerTick")
            {
                actionIds = new string[] { this.Name + "TimerTick" },
                interval = (float)(info.interval ?? this.Interval ?? default),
                triggerImmediately = info.triggerImmediately ?? default
            };

            bloon.bloonModel.AddBehavior(timer);
        }

        bloon.UpdatedModel(bloon.bloonModel);
    }

    #endregion Spawn

    #region Leak

    internal void Leak(Bloon bloon)
    {
        if (this.DefeatOnLeak)
            KillPlayer();

        RemoveUI(bloon.Id);
        this.OnLeak(bloon);
    }

    private static void KillPlayer()
    {
        if (!InGame.instance.GetSimulation().sandbox)
            InGame.instance.Lose();
        InGame.instance.SetHealth(0);
    }

    #endregion Leak

    #region Pop

    internal void Pop(Bloon bloon)
    {
        RemoveUI(bloon.Id);
        this.OnPop(bloon);
    }

    #endregion Pop

    #region Damage

    internal void Damage(Bloon bloon, float totalAmount)
    {
        this.Damage_UpdateHealthHP(bloon);
        this.OnDamage(bloon, totalAmount);
    }

    private void Damage_UpdateHealthHP(Bloon bloon)
    {
        if (!BossesAlive.TryGetValue(bloon.Id, out BossUI ui))
            return;

        if (ui.HpBar != default)
            ui.HpBar.fillAmount = (float)bloon.health / bloon.bloonModel.maxHealth;

        if (ui.HpText != default)
            SetHP(Mathf.FloorToInt(bloon.health), bloon.bloonModel.maxHealth, ui.HpText);
    }

    #endregion Damage

    public override sealed string? ToString() => this.GetType().FullName ?? base.ToString();
}