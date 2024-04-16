// Adapted from WarperSan's BossPack 

using BossIntegration.Boss;
using BossIntegration.UI;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Bloons;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity.Achievements.List;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Generic;
using System.Linq;
using Image = UnityEngine.UI.Image;

namespace BossIntegration;

/// <summary>
/// Class for adding a new boss to the game
/// </summary>
public abstract class ModBoss : ModBloon
{
    #region ModBloon

    /// <inheritdoc/>
    public sealed override void ModifyBaseBloonModel(BloonModel bloonModel)
    {
        bloonModel.RemoveAllChildren();
        bloonModel.danger = 16;
        bloonModel.overlayClass = BloonOverlayClass.Dreadbloon;
        bloonModel.bloonProperties = BloonProperties.None;
        bloonModel.tags = new Il2CppStringArray(new[] { "Bad", "Moabs", "Boss" });
        bloonModel.maxHealth = this.Health;
        bloonModel.speed = this.Speed;
        bloonModel.isBoss = true;

        if (!bloonModel.HasBehavior<HealthPercentTriggerModel>())
        {
            bloonModel.AddBehavior(new HealthPercentTriggerModel(this.Name + "-SkullEffect", false, System.Array.Empty<float>(), new string[] { this.Name + "SkullEffect" }, false));
        }

        this.ModifyBloonModel(bloonModel);

        _ = this.RegisterBoss(bloonModel);
    }

    /// <inheritdoc />
    public sealed override void Register()
    {
        if (this.RoundsInfo.Count == 0)
        {
            ModHelper.Warning<BossIntegration>($"'{this}' has no round to spawn on. It will not be registered");
            return;
        }

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

    #region Stats

    // --- ROUNDS ---
    /// <summary>
    /// Informations about the boss on the given round
    /// </summary>
    public abstract Dictionary<int, BossRoundInfo> RoundsInfo { get; }

    // --- MOD BLOON ---
    /// <summary>
    /// The base speed of the boss, 4.5 is the default for a BAD and 25 is the default for a red bloon
    /// </summary>
    public virtual float Speed => 4.5f;

    /// <summary>
    /// The base health of the boss
    /// </summary>
    public virtual int Health => 20_000;

    // --- MOD BOSS ---
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
    /// Interval between ticks
    /// </summary>
    /// <remarks>
    /// If <see cref="BossRoundInfo.interval"/> is not set, this value will be used instead
    /// </remarks>
    public virtual uint? Interval => null;

    /// <summary>
    /// Skulls per spawn
    /// </summary>
    /// <remarks>
    /// If <see cref="BossRoundInfo.skullCount"/> is not set, this value will be used instead
    /// </remarks>
    public virtual uint? Skulls => null;

    // --- UI ---
    /// <summary>
    /// Determines the quote of the boss in the UI
    /// </summary>
    public virtual string Quote => "";

    /// <summary>
    /// The people who helped to create the mod, but aren't the authors
    /// </summary>
    public virtual string ExtraCredits => string.Empty;

    /// <summary>
    /// The description of the timer effect
    /// </summary>
    public virtual string TimerDescription => "???";

    /// <summary>
    /// The description of the skull effect
    /// </summary>
    public virtual string SkullDescription => "???";

    /// <summary>
    /// Defines if the boss is using the default waiting UI
    /// </summary>
    public virtual bool UsingDefaultWaitingUi => true;

    #endregion

    #region Virtual

    // --- MODIFY ---
    /// <summary>
    /// Modifies the base model of the boss. This is called when <see cref="ModBloon.ModifyBaseBloonModel(BloonModel)"/> is called
    /// </summary>
    protected virtual void ModifyBloonModel(BloonModel bloonModel) { }

    /// <summary>
    /// Modifies the boss based on the round
    /// </summary>
    public virtual void ModifyForRound(BloonModel bloon, int round) { }

    // --- TIMER ---
    /// <summary>
    /// Called when the boss timer ticks
    /// </summary>
    public virtual void TimerTick(Bloon boss) { }

    // --- SKULL ---
    /// <summary>
    /// Called when the boss hits a skull
    /// </summary>
    public virtual void SkullEffect(Bloon boss) { }

    /// <summary>
    /// Called when the boss should get a skull remove
    /// </summary>
    public virtual void SkullEffectUI(Bloon boss)
    {
        if (!boss.GetBossUI(out BossUI ui))
            return;

        if (ui.Skulls.Count == 0)
            return;

        ui.Skulls.Last(img => img != null).DeleteObject();
    }

    /// <summary>
    /// Called when a skull is selected in the boss menu. This is useful if skulls do different things on the same boss
    /// </summary>
    /// <param name="round">Round on which the description is needed</param>
    /// <param name="index">Index of the skull called. 0 is equal to the last skull</param>
    /// <returns>Description to show</returns>
    public virtual string GetSkullDescription(int round, int index)
    {
        return this.RoundsInfo[round].skullDescription ?? this.SkullDescription;
    }

    // --- EVENTS ---
    /// <summary>
    /// Called when the boss is spawned
    /// </summary>
    public virtual void OnSpawn(Bloon bloon) { }

    /// <summary>
    /// Called when the boss is leaked
    /// </summary>
    public virtual void OnLeak(Bloon bloon) { }

    /// <summary>
    /// Called when the boss is popped
    /// </summary>
    public virtual void OnPop(Bloon bloon) { }

    /// <summary>
    /// Called when the boss takes a damage
    /// </summary>
    public virtual void OnDamage(Bloon bloon, float totalAmount) { }

    // --- UI ---
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
    /// <param name="round"></param>
    public virtual ModHelperPanel? AddBossPanel(ModHelperPanel holderPanel, Bloon bloon, ref BossUI ui, int round)
        => DefaultHealthUI.Create(this, holderPanel, bloon.bloonModel, ref ui, round);

    /// <summary>
    /// Creates the panel that shows "Boss appears in X rounds".
    /// </summary>
    /// <remarks>
    /// You must set <see cref="UsingDefaultWaitingUi"/> to true if you want to use this.
    /// </remarks>
    public virtual void AddWaitPanel(ModHelperPanel waitingHolderPanel) => ModHelper.Error<BossIntegration>($"'{this}' must override the method '{nameof(AddWaitPanel)}' since '{nameof(this.UsingDefaultWaitingUi)}' is set to true.");

    #endregion

    #region Object

    /// <inheritdoc/>
    public sealed override string? ToString() => this.GetType().FullName ?? base.ToString();

    #endregion
}