# Description
Randomizes artifacts on every stage

# Console commands

* `ar_status` - Shows is artifacts randomization enabled or disabled

* `ar_disable` - Disable artifacts randomization. Remembers status even if the game is restarted.

* `ar_enable` - Enable artifacts randomization.

# Blacklist
You can add artifacts that won't be affected by randomization. If you enable artifacts that are blacklisted, they will be enabled for the whole run.
To blacklist artifacts you should edit `blacklist` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
It should be equal to comma-separated artifact names.

Example:
```
blacklist = FriendlyFire, TeamDeath, WeakAssKnees, WispOnDeath
```

# Randomization modes
There are 2 randomization modes: `Weight`, `Chance`. 
You can switch mode by editing `randomizationMode` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.

## Weight

#### Artifact weights
You can add weight for specific artifacts it will affect artifact enabling chance.
To do that you should edit `artifactWeights` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
It should be equal to comma-separated entries that are like this pattern: `{Artifact name}: {weight}`.
Artifacts that are not included have a default weight of 50.

Example:
```
artifactWeights = Bomb: 20, Command: 75, Glass: 30
```

#### Maximum artifacts count
You can set the maximum count of artifacts that will be enabled with the `Weight` randomization mode.
To do that you should edit `maxCount` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
If you set it to `-1` then all artifacts could be enabled at once

#### Minimum artifacts count
You can set the minimum count of artifacts that will be enabled with the `Weight` randomization mode.
To do that you should edit `minCount` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.

## Chance

#### Artifact chances
You can add a chance for individual artifacts.
To do that you should edit `artifactChances` entry in config file `BepInEx/config/com.KingEnderBrine.ArtifactsRandomizer.cfg`.
It should be equal to comma-separated entries that are like this pattern: `{Artifact name}: {chance}`.
Chance should be in a range from 0 to 1 inclusive, where 0 - artifact always disabled, 1 - artifact always enabled.
Artifacts that are not included in that list have a default chance of 0.

Example:
```
artifactWeights = Bomb: 0.2, Command: 1, Glass: 0.4
```

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

**1.4.0**

* Added minimum count of randomized artifacts (Weight mode)

* Moved `blacklist` entry to `Main` section in config

**1.3.0**

* Added randomization modes

* Added artifact chances randomization mode

**1.2.0**

* Added maximum count of randomized artifacts

* Added weights config, so you can change artifact enabling chance

**1.1.0**

* Added blacklist for artifacts.

**1.0.0**

* Mod release.