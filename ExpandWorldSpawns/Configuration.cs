using ServerSync;
using Service;

namespace ExpandWorld;

public partial class Configuration
{
#nullable disable

  public static CustomSyncedValue<string> valueSpawnData;
  public static CustomSyncedValue<string> valueDropData;
#nullable enable
  public static void Init(ConfigWrapper wrapper)
  {
    valueSpawnData = wrapper.AddValue("spawn_data");
    valueSpawnData.ValueChanged += () => Spawn.Manager.FromSetting(valueSpawnData.Value);
    valueDropData = wrapper.AddValue("drop_data");
    valueDropData.ValueChanged += () => Drops.Loader.Set(valueDropData.Value);
  }
}
