using System;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using Service;

namespace ExpandWorld.Drops;

public class Loader
{
  public static string ReferenceFileName = "ref_expand_drops.yaml";
  public static string ReferenceFilePath = Path.Combine(Yaml.Directory, ReferenceFileName);
  public static string FileName = "expand_drops.yaml";
  public static string FilePath = Path.Combine(Yaml.Directory, FileName);
  public static string Pattern = "expand_drops*.yaml";

  public static void Initialize()
  {
    ToReferenceFile();
    ToFile();
    FromFile();
  }

  public static void ToReferenceFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(ReferenceFilePath)) return;
    ReferenceFileGenerator.Save();
  }

  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(FilePath)) return;
    var yaml = "# Drop data. See reference file for examples.";
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    var yaml = DataManager.Read<Data, Data>(Pattern, FromData);
    Configuration.valueDropData.Value = yaml;
  }
  // No mapping needed.
  public static Data FromData(Data data, string fileName)
  {
    foreach (var drop in data.drops)
    {
      drop.obj = DataManager.ToPrefab(drop.prefab, fileName);
      if (drop.obj)
        drop.item = drop.obj.GetComponent<ItemDrop>();
    }
    return data;
  }
  public static bool IsValid(Data data) => data.drops.All(d => d.obj != null);

  public static void Set(string yaml)
  {
    Manager.DataByHash.Clear();
    Manager.DataByName.Clear();
    if (yaml == "") return;
    try
    {
      var data = Yaml.Deserialize<Data>(yaml, "Drops").Select(d => FromData(d, "Drops")).Where(IsValid).ToList();
      if (data.Count == 0)
      {
        // No errors as emptyy is ok.
        return;
      }
      foreach (var entry in data)
      {
        Manager.Add(entry);
      }
      EWS.LogInfo($"Reloading drop data ({data.Count} entries).");
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
    Loader.Initialize();

  }
}