# Description
Randomizes artifacts on every stage

# Console commands

* `ar_status` - Shows is artifacts randomization enabled or disabled

* `ar_disable` - Disable artifacts randomization. Rememebers status even if game is restarted.

* `ar_enable` - Enable artifacts randomization.

# Blacklist
You can add artifacts that will not be randomized. If you enable artifacts that are blacklisted, they will be enabled for the whole run.
To blacklist artifacts you should edit `blacklist` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
It should be equal to comma separated artifact names.

Example:
```
blacklist = FriendlyFire, TeamDeath, WeakAssKnees, WispOnDeath
```

#### List of artifact names
Must be used exact names from that list (for vanilla, modded artifacts could also be added)

* `Chaos` - FriendlyFire

* `Command` - Command

* `Death` - TeamDeath

* `Dissonance` - MixEnemy

* `Enigma` - Enigma

* `Evolution` - MonsterTeamGainsItems

* `Frailty` - WeakAssKnees

* `Glass` - Glass

* `Honor` - EliteOnly

* `Kin` - SingleMonsterType

* `Metamorphosis` - RandomSurvivorOnRespawn

* `Sacrifice` - Sacrifice

* `Soul` - WispOnDeath

* `Spite` - Bomb

* `Swarms` - Swarms

* `Vengeance` - ShadowClone

***
# Changelog

**1.1.0**

* Added blacklist for artifacts.

**1.0.0**

* Mod release.