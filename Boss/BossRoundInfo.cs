using Il2CppAssets.Scripts.Models.Bloons.Behaviors;

namespace BossIntegration.Boss;

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
    /// If not specified, the API will use <see cref="ModBoss.SkullDescription"/>
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
    /// If not specified, the API will use <see cref="ModBoss.TimerDescription"/>
    /// </remarks>
    public string? timerDescription = null;

    /// <summary>
    /// Determines if the player will lose if, when this boss spawns, 
    /// another version of it is alive
    /// </summary>
    public bool? allowDuplicate = null;

    /// <summary>
    /// Creates an empty round info
    /// </summary>
    public BossRoundInfo() { }
}
