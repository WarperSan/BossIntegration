using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Simulation.Bloons;
using System.Collections.Generic;

namespace BossIntegration.Boss;

public static class AliveCache
{
    private static readonly Dictionary<ObjectId, BossUI> BossesAlive = new();

    /// <summary>
    /// At least one boss is alive
    /// </summary>
    public static bool AnyBossAlive => BossesAlive.Count != 0;

    public static void AddBossBloon(Bloon bloon, BossUI ui) => BossesAlive.Add(bloon.Id, ui);

    public static Dictionary<ObjectId, BossUI> GetBosses()
    {
        Dictionary<ObjectId, BossUI> copy = new();

        foreach ((ObjectId key, BossUI value) in BossesAlive)
            copy.Add(key, value);

        return copy;
    }

    internal static void ClearBosses() => BossesAlive.Clear();

    internal static void RemoveUI(ObjectId id)
    {
        if (!BossesAlive.ContainsKey(id))
            return;

        BossUI ui = BossesAlive[id];

        ui.Panel?.DeleteObject();

        _ = BossesAlive.Remove(id);
    }

    /// <returns>Is the given bloon a registered boss that is still alive?</returns>
    public static bool IsAliveBoss(this Bloon bloon, out ModBoss? boss)
        => Cache.TryGetBoss(bloon, out boss) && BossesAlive.ContainsKey(bloon.Id);

    public static bool GetBossUI(this Bloon bloon, out BossUI ui)
        => BossesAlive.TryGetValue(bloon.Id, out ui);
}