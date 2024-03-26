using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Simulation.Bloons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossIntegration.Boss;

internal static class Cache
{
    private static readonly Dictionary<string, ModBoss> Data = new();

    /// <summary>
    /// Amount of bosses loaded in the cache
    /// </summary>
    public static int Count => Data.Count;

    /// <summary>
    /// The mod has at least one boss loaded
    /// </summary>
    public static bool HasBosses => Count > 0;

    public static bool RegisterBoss(this ModBoss boss, BloonModel model)
    {
        var key = model.name;

        if (TryGetBoss(key, out _))
            return false;

        Data.Add(key, boss);

        return true;
    }

    #region Getter

    /// <returns>All the bosses that matches the given condition</returns>
    public static IEnumerable<ModBoss> GetBosses(Func<ModBoss, bool> condition)
    {
        List<ModBoss> bosses = new();

        foreach ((var _, ModBoss boss) in Data)
        {
            if (condition.Invoke(boss))
                bosses.Add(boss);
        }

        return bosses;
    }

    /// <returns>All the bosses that will spawn for the given round</returns>
    public static IEnumerable<ModBoss> GetBossesForRound(int round)
        => GetBosses(b => b.GetSpawnRounds().Contains(round));

    /// <returns>All the bosses</returns>
    public static IEnumerable<ModBoss> GetBosses()
        => GetBosses(_ => true);

    /// <returns>Is a boss associated with the given key?</returns>
    private static bool TryGetBoss(string key, out ModBoss? boss)
        => Data.TryGetValue(key, out boss);

    /// <returns>Is the given bloon a registered boss?</returns>
    public static bool TryGetBoss(this Bloon bloon, out ModBoss? boss)
        => TryGetBoss(bloon.bloonModel.name, out boss);

    #endregion
}
