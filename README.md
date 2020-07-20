# Description
Randomizes artifacts on every stage

# Console commands

* `ar_status` - Shows is artifacts randomization enabled or disabled

* `ar_disable` - Disable artifacts randomization. Rememebers status even if game is restarted.

* `ar_enable` - Enable artifacts randomization.

# Blacklist
You can add artifacts that will not be randomized. If you enable artifacts that are blacklisted, they will be enabled for the whole run.
To blacklist artifacts you should edit `blacklist` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
It should be equal to comma-separated artifact names.

Example:
```
blacklist = FriendlyFire, TeamDeath, WeakAssKnees, WispOnDeath
```

# Artifact weights
You can add weight for specific artifacts it will affect artifact enabling chance.
To do that you should edit `artifactWeights` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
It should be equal to comma-separated entries that are like this pattern: `{Artifact name}: {weight}`.
Artifacts that are not included have a default weight of 50.

Example:
```
artifactWeights = Bomb: 20, Command: 75, Glass: 30
```

# Maximum artifacts count
You can set the maximum count of artifacts that will be enabled.
To do that you should edit `maxCount` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
If you set it to `-1` then all artifacts could be enabled at once

***
# List of artifact names
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

**1.2.0**

* Added maximum count of randomized artifacts

* Added weights config, so you can change artifact enabling chance

**1.1.0**

* Added blacklist for artifacts.

**1.0.0**

* Mod release.