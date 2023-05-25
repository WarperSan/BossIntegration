<a href="https://github.com/doombubbles/paths-plus-plus/releases/latest/download/PathsPlusPlus.dll">
    <img align="left" alt="Icon" height="90" src="Icon.png">
    <img align="right" alt="Download" height="75" src="https://raw.githubusercontent.com/gurrenm3/BTD-Mod-Helper/master/BloonsTD6%20Mod%20Helper/Resources/DownloadBtn.png">
</a>

<h1 align="center">Boss Integration</h1>
A mod allowing to create modded bosses in an easy yet complete way.

# Features
- Boss informations accessible within the game
- Toggle bosses to customize the challenge

![BossMenuShowCase](BossMenuShowcase.png)

# For modders: How to use it ?
## Rounds
In order to make the boss spawn, you have to add an item in RoundsInfo like this:
```cs
[/* Round */] = new BossRoundInfo()
{
    /* Stats */
}
```
You can specify things like the skull count and the interval of the timer, but will have to specify the health by overriding `ModifyForRound`.

## Behaviors
To use skulls, you need to override `SkullEffect`. 

Note that if you don't use a custom UI, you must call `base.SkullEffect()` in `SkullEffect`.
<br><br>
To use timer, you need to override `TimerTick`.

## Customizable UI
This mod allows you to use your own custom UI. For customizing the wait UI ("Boss appears in 1 round" UI), you must set to false `UsingDefaultWaitingUi` **AND** override `AddWaitPanel`. If you only do one of those steps, your mod will not work.
<br><br>
You can also use your own boss health bar UI. For that, use must override `AddBossPanel` **AND** add those references:
- Add the HP text to `BossHpTexts`
- Add the HP Image to `BossBars`
- If you use skulls, add the skulls to `BossSkulls`

## API
A good part of the mod is allocated to the boss API. Some properties like `ExtraCredits`, `SkullDescription` and `TimerDescription` are only used in it. Those allow you to show more informations about the boss, without needing the user to look on your wiki. 

Note: Properties like `ModBoss.Interval` allows you to set a value to all the tiers 

## Examples
### Bare minimum Version
This is what a boss needs in order to exist. The boss won't have any behaviors.
```cs
public class SimpleBoss : ModBoss
{
    public override Dictionary<int, BossRoundInfo> RoundsInfo => new Dictionary<int, BossRoundInfo>()
    {
        [/* Round */] = new BossRoundInfo()
        {
            /* Stats */
        }
    };

    public override string Icon => /* Icon */;

    public override void ModifyBaseBloonModel(BloonModel bloonModel)
    {
        /* Modify the stats */
    }
}
```
### Complete Version
This is the closest version to an actual boss.
```cs
public class CompleteBoss : ModBoss
{
    public override string DisplayName => /* Display name */;
    public override string Description => /* Boss description */;
    public override string Icon => /* Icon */;

    public override Dictionary<int, BossRoundInfo> RoundsInfo => new Dictionary<int, BossRoundInfo>()
    {
        [/* Round */] = new BossRoundInfo()
        {
            /* Stats */
        }
    };

    public override void ModifyBaseBloonModel(BloonModel bloonModel)
    {
        /* Set base stats */
    }

    public override BloonModel ModifyForRound(BloonModel bloon, int round)
    {
        /* Modify according to the round */

        return base.ModifyForRound(bloon, round);
    }

    public override string SkullDescription => /* Skull description */;
    public override void SkullEffect(Bloon boss)
    {
        /* Skull Effect */
        base.SkullEffect(boss);
    }

    public override string TimerDescription => /* Timer description */;
    public override void TimerTick(Bloon boss)
    {
        /* Do timer tick */
    }
}
```

### "Full" version
This is a version where it uses all the primary functions of ModBoss. This is for the people who wants custom UIs or a detailed boss in the API. 
```cs
public class FullBoss : ModBoss
{
    public override string DisplayName => /* Display name */;
    public override string Description => /* Boss description */;
    public override string ExtraCredits => /* Extra credits */;
    public override string Icon => /* Icon */;

    public override Dictionary<int, BossRoundInfo> RoundsInfo => new Dictionary<int, BossRoundInfo>()
    {
        [/* Round */] = new BossRoundInfo()
        {
            /* Stats */
        }
    };

    public override void ModifyBaseBloonModel(BloonModel bloonModel)
    {
        /* Set base stats */
    }

    public override ModHelperPanel AddWaitPanel(ModHelperPanel waitingHolderPanel)
    {
        /* Custom wait panel */
        return base.AddWaitPanel(waitingHolderPanel);
    }

    public override BloonModel ModifyForRound(BloonModel bloon, int round)
    {
        /* Modify according to the round */

        return base.ModifyForRound(bloon, round);
    }

    public override ModHelperPanel AddBossPanel(ModHelperPanel holderPanel)
    {
        /* Custom boss panel */
        return base.AddBossPanel(holderPanel);
    }

    public override string SkullDescription => /* Skull description */;
    public override void SkullEffect(Bloon boss)
    {
        /* Skull Effect */
        base.SkullEffect(boss);
    }

    public override string TimerDescription => /* Timer description */;
    public override void TimerTick(Bloon boss)
    {
        /* Do timer tick */
    }
}
```
