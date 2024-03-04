using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Spawn;

public class Manager
{
  public static string FileName = "expand_spawns.yaml";
  public static string FilePath = Path.Combine(EWD.YamlDirectory, FileName);
  public static string Pattern = "expand_spawns*.yaml";

  public static bool IsValid(SpawnSystem.SpawnData spawn) => spawn.m_prefab;
  public static string Save()
  {
    var spawnSystem = SpawnSystem.m_instances.FirstOrDefault();
    if (spawnSystem == null) return "";
    var spawns = spawnSystem.m_spawnLists.SelectMany(s => s.m_spawners);
    var yaml = DataManager.Serializer().Serialize(spawns.Select(Loader.ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    return yaml;
  }
  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(FilePath)) return;
    var yaml = Save();
    Configuration.valueSpawnData.Value = yaml;
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    var yaml = DataManager.Read(Pattern);
    Configuration.valueSpawnData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml)
  {
    // First load is delayed because RRR uses ServerSync to create monsters (race condition).
    if (HandleSpawnData.Override == null) return;
    if (Helper.IsClient()) Set(yaml);
  }
  public static void Set(string yaml)
  {
    HandleSpawnData.Override = null;

    Loader.Data.Clear();
    Loader.Objects.Clear();
    Loader.ExtraData.Clear();
    if (yaml == "") return;
    try
    {
      var data = DataManager.Deserialize<Data>(yaml, FileName)
        .Select(Loader.FromData).Where(IsValid).ToList();
      if (data.Count == 0)
      {
        EWS.LogWarning($"Failed to load any spawn data.");
        return;
      }
      EWS.LogInfo($"Reloading spawn data ({data.Count} entries).");
      HandleSpawnData.Override = data;
      SpawnSystem.m_instances.ForEach(HandleSpawnData.Set);
    }
    catch (Exception e)
    {
      EWS.LogError(e.Message);
      EWS.LogError(e.StackTrace);
    }
  }

  public static void SetupWatcher()
  {
    DataManager.SetupWatcher(Pattern, FromFile);
  }


}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.VeryLow)]
public class InitializeContent
{
  static void Postfix()
  {
    HandleSpawnData.Override = null;
    if (Helper.IsServer())
    {
      Manager.FromFile();
    }
  }
}

[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
public class HandleSpawnData
{
  public static List<SpawnSystem.SpawnData>? Override = null;
  static void Postfix(SpawnSystem __instance)
  {
    if (Override == null)
    {
      if (Helper.IsClient() && Configuration.valueSpawnData.Value != "")
        Manager.Set(Configuration.valueSpawnData.Value);
      if (Helper.IsServer())
        Manager.ToFile();
    }
    Set(__instance);
  }

  public static void Set(SpawnSystem system)
  {
    if (Override == null) return;

    while (system.m_spawnLists.Count > 1)
      system.m_spawnLists.RemoveAt(system.m_spawnLists.Count - 1);
    system.m_spawnLists[0].m_spawners = Override;

  }
}

[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.IsSpawnPointGood))]
public class IsSpawnPointGood
{

  static bool Postfix(bool result, SpawnSystem.SpawnData spawn, Vector3 spawnPoint)
  {
    if (!result) return false;
    if (Loader.ExtraData.TryGetValue(spawn, out var data))
    {
      var distance = Utils.LengthXZ(spawnPoint);
      if (distance < data.minDistance) return false;
      if (data.maxDistance > 0f && distance > data.maxDistance) return false;
    }
    return result;
  }
}