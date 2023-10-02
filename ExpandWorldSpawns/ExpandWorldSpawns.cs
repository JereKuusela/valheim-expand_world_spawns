﻿using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", BepInDependency.DependencyFlags.HardDependency)]
public class EWS : BaseUnityPlugin
{
  public const string GUID = "expand_world_spawns";
  public const string NAME = "Expand World Spawns";
  public const string VERSION = "1.1";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public static void LogWarning(string message) => Log.LogWarning(message);
  public static void LogError(string message) => Log.LogError(message);
  public static void LogInfo(string message) => Log.LogInfo(message);
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public static bool NeedsMigration = File.Exists(Path.Combine(Paths.ConfigPath, "expand_world.cfg")) && !File.Exists(Path.Combine(Paths.ConfigPath, "expand_world_spawns.cfg"));
  public void Awake()
  {
    Log = Logger;
    ConfigWrapper wrapper = new("expand_spawns_config", Config, ConfigSync, () => { });
    Configuration.Init(wrapper);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    try
    {
      if (ExpandWorldData.Configuration.DataReload)
      {
        Spawn.Manager.SetupWatcher();
      }
    }
    catch (Exception e)
    {
      Log.LogError(e);
    }
  }
}
