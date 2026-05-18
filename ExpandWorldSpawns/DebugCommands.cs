using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  static void Postfix()
  {
    new Terminal.ConsoleCommand("ew_spawns", "Forces spawn file creation.", (args) =>
    {
      Spawn.Manager.Save();
    }, true);
    new Terminal.ConsoleCommand("ew_drops", "Forces drop file creation.", (args) =>
    {
      Drops.ReferenceFileGenerator.Save();
    }, true);
    new Terminal.ConsoleCommand("ew_test_spawn", "Spawns a creature from spawn system by entry name.", (args) =>
    {
      var ss = GetClosestSpawnSystem(args.Context);
      if (!ss) return;
      ss.m_nview.ClaimOwnership();
      var spawn = FindSpawn(args.Context, ss, args.ArgsAll);
      if (spawn == null) return;
      ss.Spawn(spawn, Player.m_localPlayer.transform.position, false);
    }, true, optionsFetcher: GetSpawnNames);
    new Terminal.ConsoleCommand("ew_try_spawn", "Attempts to spawn a creature from spawn system by entry name.", (args) =>
    {
      var ss = GetClosestSpawnSystem(args.Context);
      if (!ss) return;
      ss.m_nview.ClaimOwnership();
      var spawn = FindSpawn(args.Context, ss, args.ArgsAll);
      if (spawn == null) return;
      originalSpawnData.canSpawnCloseToPlayer = spawn.m_canSpawnCloseToPlayer;
      originalSpawnData.spawnRadiusMin = spawn.m_spawnRadiusMin;
      originalSpawnData.spawnRadiusMax = spawn.m_spawnRadiusMax;
      originalSpawnData.spawnInterval = spawn.m_spawnInterval;
      spawn.m_canSpawnCloseToPlayer = true;
      spawn.m_spawnRadiusMin = 0.01f;
      spawn.m_spawnRadiusMax = 0.01f;
      spawn.m_spawnInterval = 0.01f;
      ss.UpdateSpawning();
      spawn.m_canSpawnCloseToPlayer = originalSpawnData.canSpawnCloseToPlayer;
      spawn.m_spawnRadiusMin = originalSpawnData.spawnRadiusMin;
      spawn.m_spawnRadiusMax = originalSpawnData.spawnRadiusMax;
      spawn.m_spawnInterval = originalSpawnData.spawnInterval;
    }, true, optionsFetcher: GetSpawnNames);
  }

  private static OriginalSpawnData originalSpawnData;

  private static List<string> GetSpawnNames()
  {
    var ss = GetClosestSpawnSystem();
    if (!ss) return [];

    var list = new List<string>();
    foreach (var spawnList in ss.m_spawnLists)
    {
      foreach (var spawn in spawnList.m_spawners)
      {
        list.Add(spawn.m_name);
      }
    }
    return list;
  }

  private static SpawnSystem? GetClosestSpawnSystem(Terminal? context = null)
  {
    if (!Player.m_localPlayer)
    {
      context?.AddString("Player not found.");
      return null;
    }
    var pos = Player.m_localPlayer.transform.position;
    var ss = SpawnSystem.m_instances.OrderBy(s => Vector3.Distance(s.transform.position, pos)).FirstOrDefault();
    if (!ss)
    {
      context?.AddString("No spawn system found nearby.");
      return null;
    }
    return ss;
  }
  private static SpawnSystem.SpawnData? FindSpawn(Terminal context, SpawnSystem ss, string name)
  {
    foreach (var list in ss.m_spawnLists)
    {
      foreach (var spawn in list.m_spawners)
      {
        if (spawn.m_name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return spawn;
      }
    }
    context.AddString($"Spawn '{name}' not found in closest spawn system.");
    return null;
  }
}

public struct OriginalSpawnData
{
  public bool canSpawnCloseToPlayer;
  public float spawnRadiusMin;
  public float spawnRadiusMax;
  public float spawnInterval;
}