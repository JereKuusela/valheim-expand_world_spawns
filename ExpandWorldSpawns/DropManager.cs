using System.Collections.Generic;
using HarmonyLib;

namespace ExpandWorld.Drops;

[HarmonyPatch]
public class Manager
{

  public static readonly int HashDrop = "ews_drops".GetStableHashCode();

  public static Dictionary<int, Data> DataByHash = [];
  public static Dictionary<string, Data> DataByName = [];


  public static void Add(Data data)
  {
    var hash = data.name.GetStableHashCode();
    DataByHash[hash] = data;
    DataByName[data.name] = data;
  }

  public static bool TryGetData(ZNetView view, out Data data)
  {
    if (!view)
    {
      data = null!;
      return false;
    }
    var zDO = view.GetZDO();
    if (zDO == null)
    {
      data = null!;
      return false;
    }
    return TryGetData(zDO, out data);
  }
  public static bool TryGetData(ZDO zDO, out Data data)
  {
    data = null!;
    if (zDO == null) return false;
    var hash = zDO.GetInt(HashDrop, 0);
    if (hash != 0)
      return DataByHash.TryGetValue(hash, out data);
    var name = zDO.GetString(HashDrop, "");
    if (name != "")
      return DataByName.TryGetValue(name, out data);
    return false;
  }


}