using Il2CppAssets.Scripts.Simulation.Bloons;
using System.Collections.Generic;
using System.Linq;

namespace BossIntegration.Boss;

/// <summary>
/// Extensions for <see cref="ModBoss"/>
/// </summary>
public static class ModBossExtensions
{
    /// <param name="bloon">Bloon presenting the boss</param>
    /// <returns>Tier of the boss or null if not set or not found</returns>
    public static uint? GetTier(this Bloon bloon)
    {
        if (!bloon.TryGetBoss(out ModBoss? boss) || boss == null)
            return null;

        if (!bloon.GetBossUI(out BossUI ui) || !ui.round.HasValue)
            return null;

        // Fucking IDE
#pragma warning disable IDE0046 // Convert to conditional expression
        if (!boss.RoundsInfo.TryGetValue(ui.round.Value, out BossRoundInfo roundInfo))
            return null;
#pragma warning restore IDE0046 // Convert to conditional expression

        return roundInfo.tier;
    }

    /// <summary>
    /// The rounds the boss should spawn on
    /// </summary>
    public static IEnumerable<int> GetSpawnRounds(this ModBoss boss) => boss.RoundsInfo.Keys;

    /// <summary>
    /// Checks if the boss uses a timer
    /// </summary>
    public static bool UsesTimer(this ModBoss boss)
        => boss.Interval != null || boss.RoundsInfo.Any(info => info.Value.interval != null);

    /// <summary>
    /// Checks if the boss has any skull
    /// </summary>
    public static bool UsesSkull(this ModBoss boss)
        => boss.Skulls != null || boss.RoundsInfo.Any(info => info.Value.skullCount > 0);

    public static float[] GetSkullsPosition(this BossRoundInfo info, ModBoss boss)
    {
        if (info.percentageValues == null)
        {
            var skullsCount = info.skullCount ?? boss.Skulls ?? 0;

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

        return info.percentageValues;
    }

    private static readonly string[] NumSuffixs = new string[] {
        "K", "M", "B", "T",
        "q", "Q", "s", "S",
        "O", "N", "d", "U",
        "D", "!", "@", "#",
        "$", "%", "^", "&",
        "*", "[", "]", "{",
        "}", ";"
    };

    /// <returns>Formatted version of the given number</returns>
    public static string Format(this float number)
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
}