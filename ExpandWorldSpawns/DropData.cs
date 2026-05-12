using System.ComponentModel;
using UnityEngine;

namespace ExpandWorld.Drops;

public class Data
{
  public string name = "";

  [DefaultValue(1)]
  public int minAmount = 1;
  [DefaultValue(1)]
  public int maxAmount = 1;
  [DefaultValue(1f)]
  public float chance = 1f;
  [DefaultValue(false)]
  public bool oneOfEach = false;

  public DropEntry[] drops = [];
}

public class DropEntry
{
  public string prefab = "";

  public GameObject? obj = null;
  public ItemDrop? item = null;

  [DefaultValue(1)]
  public int minAmount = 1;
  [DefaultValue(1)]
  public int maxAmount = 1;
  [DefaultValue(1f)]
  public float chance = 1f;
  [DefaultValue(false)]
  public bool onePerPlayer = false;
  [DefaultValue(false)]
  public bool levelMultiplier = false;

  [DefaultValue(1)]
  public int minStack = 1;
  [DefaultValue(1)]
  public int maxStack = 1;
  [DefaultValue(1f)]
  public float weight = 1f;

  [DefaultValue(1)]
  public int amount = 1;
  [DefaultValue(true)]
  public bool recover = true;

  [DefaultValue(false)]
  public bool dontScale = false;
}
