using BTD_Mod_Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BossIntegration.Boss;
internal static class Permissions
{
    private static string Permission_Path => ModHelper.ModHelperDirectory + "\\Mod Settings\\BossesSetting.json";

    private static readonly Dictionary<string, Dictionary<int, bool>> Data = InitPermissions();

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

    /// <param name="boss">Boss to check the permission of</param>
    /// <param name="round">Round to check the permission at</param>
    /// <returns>Is the boss allowed?</returns>
    public static bool GetPermission(this ModBoss boss, int round)
    {
        var key = boss.ToString();

        return
            key != null && 
            Data.TryGetValue(key, out Dictionary<int, bool>? permissions) && 
            permissions != null && 
            permissions.TryGetValue(round, out var isAllowed) && 
            isAllowed;
    }

    /// <summary>
    /// Sets the permission for <paramref name="boss"/> on <paramref name="round"/> to <paramref name="value"/> 
    /// </summary>
    /// <param name="boss">Boss to change the permission to</param>
    /// <param name="round">Round of the permission</param>
    /// <param name="value">New value of the permission</param>
    /// <returns>Success of the operation</returns>
    public static bool SetPermission(this ModBoss boss, int round, bool value)
    {
        var key = boss.ToString();

        if (key == null)
            return false;

        Dictionary<int, bool> values = Data.ContainsKey(key) ? Data[key] : new();
        values[round] = value;
        Data[key] = values;
        return true;
    }

    /// <summary>
    /// Writes the current permissions into the file
    /// </summary>
    public static void SavePermissions()
    {
        // Clean the permissions
        for (var i = Data.Count - 1; i >= 0; i--)
        {
            var key = Data.Keys.ElementAt(i);

            for (var j = Data[key].Count - 1; j >= 0; j--)
            {
                var key2 = Data[key].Keys.ElementAt(j);

                if (!Data[key][key2])
                    _ = Data[key].Remove(key2);
            }

            // Remove key if empty
            if (Data[key].Count == 0)
                _ = Data.Remove(key);
        }

        // Save it
        File.WriteAllText(Permission_Path, JsonConvert.SerializeObject(Data));

        ModHelper.Msg<BossIntegration>("Boss Rounds Saved !");
    }
}
