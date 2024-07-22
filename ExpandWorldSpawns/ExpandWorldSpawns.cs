using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", "1.27")]
public class EWS : BaseUnityPlugin
{
  public const string GUID = "expand_world_spawns";
  public const string NAME = "Expand World Spawns";
  public const string VERSION = "1.9";
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
  public void Awake()
  {
    Log = Logger;
    ConfigWrapper wrapper = new("expand_spawns_config", Config, ConfigSync, () => { });
    Configuration.Init(wrapper);
    new Harmony(GUID).PatchAll();
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
