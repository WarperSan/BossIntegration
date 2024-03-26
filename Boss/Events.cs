using BossIntegration.UI;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System.Collections.Generic;
using UnityEngine;

namespace BossIntegration.Boss;
internal static class Events
{
    public static void Pop(this ModBoss boss, Bloon bloon)
    {
        AliveCache.RemoveUI(bloon.Id);
        boss.OnPop(bloon);
    }

    public static void Leak(this ModBoss boss, Bloon bloon)
    {
        if (boss.DefeatOnLeak)
            KillPlayer(bloon);

        AliveCache.RemoveUI(bloon.Id);
        boss.OnLeak(bloon);
    }

    private static void KillPlayer(Bloon bloon)
    {
        InGame.instance.UpdateLeaks(bloon, (float)InGame.instance.GetHealth(), 0);
        //if (!InGame.instance.GetSimulation().sandbox)
        //    InGame.instance.Lose();
        //InGame.instance.SetHealth(0);
    }

    #region Damage

    public static void Damage(this ModBoss boss, Bloon bloon, float totalAmount)
    {
        bloon.UpdateHealthHP();
        boss.OnDamage(bloon, totalAmount);
    }

    private static void UpdateHealthHP(this Bloon bloon)
    {
        if (!bloon.GetBossUI(out BossUI ui))
            return;

        if (ui.HpBar != default)
            ui.HpBar.fillAmount = (float)bloon.health / bloon.bloonModel.maxHealth;

        if (ui.HpText != default)
            SetHP(Mathf.FloorToInt(bloon.health), bloon.bloonModel.maxHealth, ui.HpText);
    }

    private static void SetHP(float health, float maxHealth, ModHelperText hpText)
    {
        var formatHP = (bool)BossIntegration.FormatBossHP.GetValue();

        var current = formatHP ? health.Format() : health.ToString();
        var max = formatHP ? maxHealth.Format() : maxHealth.ToString();

        hpText.SetText($"{current} / {max}");
    }

    #endregion

    #region Spawn

    public static void Spawn(this ModBoss boss, int round)
    {
        BloonModel bossModel = Game.instance.model.GetBloon(boss.Id);
        boss.ModifyForRound(bossModel, round);
        
        Bloon bloon = InGame.instance.GetMap().spawner.Emit(bossModel, 0, 0);
        bloon.bloonModel.speedFrames = bossModel.speed * 0.416667f / 25;

        BossRoundInfo info = boss.RoundsInfo[round];

        // No duplicates
        Spawn_PreventDuplicates(boss, bloon, info);

        // Skulls
        Spawn_Skulls(boss, info, bloon.bloonModel);

        // Timer
        Spawn_Timer(boss, info, bloon);

        // UI infos
        var ui = new BossUI()
        {
            round = round,
        };

        ModBossUI.AddHealthPanel(boss, bloon, ref ui, round);

        AliveCache.AddBossBloon(bloon, ui);

        UpdateHealthHP(bloon);
        boss.OnSpawn(bloon);
    }

    private static void Spawn_PreventDuplicates(ModBoss boss, Bloon bloon, BossRoundInfo info)
    {
        if (!info.allowDuplicate.HasValue || info.allowDuplicate.Value)
            return;

        if (!ModBossUI.HasHealthPanel(boss))
            return;

        KillPlayer(bloon);
    }
    private static void Spawn_Skulls(ModBoss boss, BossRoundInfo info, BloonModel model)
    {
        if ((boss.Skulls == null && info.skullCount == null) || !model.HasBehavior<HealthPercentTriggerModel>())
            return;

        // Puts default skulls placement
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

        HealthPercentTriggerModel bossSkulls = model.GetBehavior<HealthPercentTriggerModel>();
        bossSkulls.percentageValues = info.percentageValues;
        bossSkulls.preventFallthrough = info.preventFallThrough != null && (bool)info.preventFallThrough;
    }
    private static void Spawn_Timer(ModBoss boss, BossRoundInfo info, Bloon bloon)
    {
        if (bloon.bloonModel.HasBehavior<TimeTriggerModel>())
            bloon.bloonModel.RemoveBehaviors<TimeTriggerModel>();

        if (info.interval != null || boss.Interval != null)
        {
            var timer = new TimeTriggerModel(boss.Name + "-TimerTick")
            {
                actionIds = new string[] { boss.Name + "TimerTick" },
                interval = (float)(info.interval ?? boss.Interval ?? default),
                triggerImmediately = info.triggerImmediately ?? default
            };

            bloon.bloonModel.AddBehavior(timer);
        }

        bloon.UpdatedModel(bloon.bloonModel);
    }

    #endregion
}
