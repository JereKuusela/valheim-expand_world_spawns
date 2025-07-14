# Expand World Spawns

Allows configuring spawns.

Install on all clients and the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install [Expand World Data](https://valheim.thunderstore.io/package/JereKuusela/Expand_World_Data/).

## Features

- Add new spawns and edit existing ones.

## Configuration

See the [wiki](https://valheim.fandom.com/wiki/Spawn_zones) for more info about events.

The file `expand_world/expand_spawns.yaml` is created when loading a world.

### expand_spawns.yaml

Note: All distances are in meters, and don't scale up with the world size. For bigger worlds you may need to increase some of the values.

- prefab: Name of the object to spawn.
  - Any [object](https://valheim.fandom.com/wiki/Item_IDs) is valid, not just creatures.
- name: Identifier for this entry, only needed for mod compatibility.
- enabled (default: `true`): Quick way to disable this entry if needede.
- biome: List of possible biomes.
- biomeArea: List of possible biome areas (edge = zones with multiple biomes, median = zones with only a single biome, 4 = unused, leftover from Valheim data).
- spawnChance (default: `100` %): Chance to spawn when attempted.
- maxSpawned: Limit for this entry. Also how many spawn attempts are stacked over time.
- spawnInterval: How often the spawning is attempted.
- minLevel (default: `1`): Minimum creature level.
- maxLevel (default: `1`): Maximum creature level.
- minAltitude (default: `-1000` meters): Minimum terrain altitude.
- maxAltitude (default: `1000` meters): Maximum terrain altitude.
- minDistance (default: `0` meters): Minimum distance from the world center (0 = disabled).
- maxDistance (default: `0` meters): Maximum distance from the world center (0 = disabled).
- spawnAtDay (default: `true`): Enabled during the day time.
- spawnAtNight (default: `true`): Enabled during the night time.
- requiredGlobalKey: Which [global keys](https://valheim.fandom.com/wiki/Global_Keys) must be set to enable this entry.
  - When using format `key value`, the key must have at least this amount of value.
  - After spawning, the key value is reduced by the required value.
  - This can be used to create limited spawns.
  - Creature deaths can be changed to increase the key value by using the `defeatSetGlobalKey` field.
  - See the [Fields](#fields) section for more info.
- requiredEnvironments: List of valid environments/weathers.
- spawnDistance (default: `10` meters): Distance to suppress similar spawns.
- spawnRadiusMin (default: `40` meters): Minimum distance from every player.
- spawnRadiusMax (default: `80` meters): Maximum distance from any player.
- groupSizeMin (default: `1`): Minimum amount spawned at the same time.
- groupSizeMax (default: `1`): Maximum amount spawned at the same time.
- groupRadius (default: `3` meters): Radius when spawning multiple objects.
- minTilt (default: `0` degrees): Minimum terrain angle.
- maxTilt (default: `35` degrees): Maximum terrain angle.
- inForest (default: `true`): Enabled in forests.
- outsideForest (default: `true`): Enabled outside forests.
- canSpawnCloseToPlayers (default: `false`): If set to true, spawnRadiusMin is ignored.
- insidePlayerBase (default: `false`): If set to true, player base protection is ignored.
- inLava (default: `false`): If set to true, can spawn in lava.
- outsideLava (default: `true`): If set to false, can only spawn in lava.
- minOceanDepth (default: `0` meters): Minimum ocean depth.
- maxOceanDepth (default: `0` meters): Maximum ocean depth.
- huntPlayer (default: `false`): Spawned creatures are more aggressive.
- groundOffset (default: `0.5` meters): Spawns above the ground.
- groundOffsetRandom (default: `0` meters): Maximum offset from the ground.
- levelUpMinCenterDistance (default: `0` meters): Distance from the world center to enable higher creature levels. This is not scaled with the world size.
- overrideLevelupChance (default: `-1` percent): Chance per level up (from the default 10%).
- faction: Name of the faction. Requires using Expand World Factions.
- data: ZDO data override.
- fields: Custom fields to override prefab properties.
  - See the [Fields](#fields) section for more info.
- objects: Extra objects to spawn. Spawned on top of any obstacles. The spawning is skipped if 10 meters above the original position. Format is `id,posX,posZ,posY,chance,data`.
  - id: Prefab name.
  - posX, posZ, posY: Offset from the location position. Defalt is 0.
  - chance: Chance to spawn (from 0 to 1). Default is 1.
  - data: ZDO data override.

## Fields

Fields can be used to override prefab properties.

You can hover a creature and use `data dump=check` from World Edit Commands mod to print available fields to `config/data/data.yaml` file.

```yaml
- prefab: Troll
  ...
  fields:
    # Adds kill count tracking for trolls.
    defeatSetGlobalKey: killedtroll ++1
- prefab: Troll
  ...
  # Spawning consumes 10 troll kills.
  requiredGlobalKey: killedtroll 10
  fields:
    boss: true
    name: Troll King
    bossEvent: foresttrolls
    health: 1000
    runSpeed: 8
    # "damage" is converted to "RandomSkillFactor", provided for convenience.
    damage: 2
```

## Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_spawns)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
