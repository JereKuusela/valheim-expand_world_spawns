using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using ExpandWorldData;
using HarmonyLib;
using Service;
using UnityEngine;

namespace ExpandWorld.Spawn;

public class Manager
{
  public static string FileName = "expand_spawns.yaml";
  public static string FilePath = Path.Combine(Yaml.Directory, FileName);
  public static string Pattern = "expand_spawns*.yaml";

  public static bool IsValid(SpawnSystem.SpawnData spawn) => spawn.m_prefab;
  public static string Save()
  {
    var spawnSystem = SpawnSystem.m_instances.FirstOrDefault();
    if (spawnSystem == null) return "";
    var spawns = spawnSystem.m_spawnLists.SelectMany(s => s.m_spawners);
    var yaml = Yaml.Serializer().Serialize(spawns.Select(Loader.ToData).ToList());
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
    var yaml = DataManager.Read<Data, SpawnSystem.SpawnData>(Pattern, Loader.FromData);
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
    if (yaml == "") return;
    try
    {
      var data = Yaml.Deserialize<Data>(yaml, "Spawns")
        .Select(d => Loader.FromData(d, "Spawns")).Where(IsValid).ToList();
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
    Yaml.SetupWatcher(Pattern, FromFile);
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

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RPC_SetGlobalKey))]
public class RPC_SetGlobalKey
{
  // This is called on creature death.
  // Adds support for incrementing the key value when "key value" is used.
  static void Prefix(ZoneSystem __instance, ref string name)
  {
    var key = ZoneSystem.GetKeyValue(name.ToLower(), out var value, out _);
    if (value.StartsWith("--", StringComparison.OrdinalIgnoreCase))
    {
      if (int.TryParse(value.Substring(2), out var amount))
      {
        if (__instance.GetGlobalKey(key, out var prev) && int.TryParse(prev, out var prevValue))
          name = $"{key} {prevValue - amount}";
        else
          name = $"{key} -{amount}";
      }
    }
    else if (value.StartsWith("++", StringComparison.OrdinalIgnoreCase))
    {
      if (int.TryParse(value.Substring(2), out var amount))
      {
        if (__instance.GetGlobalKey(key, out var prev) && int.TryParse(prev, out var prevValue))
          name = $"{key} {prevValue + amount}";
        else
          name = $"{key} {amount}";
      }
    }
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey), typeof(string))]
public class GetGlobalKey
{
  // This is called by the spawn check.
  // Adds support for checking that the key exceeds the required value.
  static bool Prefix(ZoneSystem __instance, string name, ref bool __result)
  {
    var split = name.Trim().Split(' ');
    if (split.Length < 2) return true;
    if (!int.TryParse(split[1], out var value)) return true;
    __result = HasKey(__instance, split[0].ToLower(), value);
    return false;
  }

  static bool HasKey(ZoneSystem zs, string requiredKey, int requiredValue)
  {
    if (!zs.m_globalKeysValues.TryGetValue(requiredKey, out var strValue)) return false;
    if (!int.TryParse(strValue, out var value)) return false;
    return value >= requiredValue;
  }
}

[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
public class Spawn
{
  // After spawn, consume the required amount of global key.
  static void Postfix(SpawnSystem.SpawnData critter)
  {
    var split = critter.m_requiredGlobalKey.Trim().Split(' ');
    if (split.Length < 2) return;
    if (!int.TryParse(split[1], out var amount)) return;
    ZoneSystem.instance.SetGlobalKey($"{split[0]} --{amount}");
  }
}

[HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
public class HandleDeath
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => new CodeMatcher(instructions)
      .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Character), nameof(Character.m_defeatSetGlobalKey))), new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey), [typeof(string)])))
      .Advance(1)
      .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HandleDeath), nameof(ConvertKey))))
      .InstructionEnumeration();


  static string ConvertKey(string key) => $"{key} ++1";
}