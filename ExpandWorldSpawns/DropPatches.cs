using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Drops;


[HarmonyPatch]
public static class CharacterDropPatches
{
  // Character drops know their parent object so single patch can handle all cases.
  [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList)), HarmonyPrefix]
  static void CharacterDropGenerateDropList(CharacterDrop __instance)
  {
    if (!Manager.TryGetData(__instance.m_character.m_nview, out var data)) return;
    Apply(__instance, data);
  }

  private static void Apply(CharacterDrop drop, Data data)
  {
    drop.m_drops = [.. data.drops.Select(d => new CharacterDrop.Drop
    {
      m_prefab = d.obj,
      m_chance = d.chance,
      m_amountMax = d.maxAmount,
      m_amountMin = d.minAmount,
      m_dontScale = d.dontScale,
      m_onePerPlayer = d.onePerPlayer,
      m_levelMultiplier = d.levelMultiplier,
    })];
  }
}


[HarmonyPatch]
public static class PieceRequirementPatches
{
  // Character drops know their parent object so single patch can handle all cases.
  [HarmonyPatch(typeof(Piece), nameof(Piece.DropResources)), HarmonyPrefix]
  static void PieceDropResources(Piece __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance, data);
  }

  private static void Apply(Piece piece, Data data)
  {
    piece.m_resources = [.. data.drops.Where(d => d.item != null).Select(d => new Piece.Requirement
    {
      m_amount = Random.Range(d.minAmount, d.maxAmount + 1),
      m_resItem = d.item,
      m_amountPerLevel = 0,
      m_recover = true,
      m_extraAmountOnlyOneIngredient = 0,
    })];
  }
}

[HarmonyPatch]
public static class DropTablePatches
{
  // Drop table doesn't know its parent object so have to patch each case.
  [HarmonyPatch(typeof(Container), nameof(Container.AddDefaultItems)), HarmonyPrefix]
  static void ContainerAddDefaultItems(Container __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_defaultItems, data);
  }

  [HarmonyPatch(typeof(FishingFloat), nameof(FishingFloat.Catch)), HarmonyPrefix]
  static void FishingFloatCatch(Fish fish)
  {
    if (!Manager.TryGetData(fish.m_nview, out var data)) return;
    Apply(fish.m_extraDrops, data);
  }

  [HarmonyPatch(typeof(Pickable), nameof(Pickable.RPC_Pick)), HarmonyPrefix]
  static void PickableRPC_Pick(Pickable __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_extraDrops, data);
  }

  [HarmonyPatch(typeof(DropOnDestroyed), nameof(DropOnDestroyed.OnDestroyed)), HarmonyPrefix]
  static void DropOnDestroyedOnDestroyed(DropOnDestroyed __instance)
  {
    if (!Manager.TryGetData(__instance.GetComponent<ZNetView>(), out var data)) return;
    Apply(__instance.m_dropWhenDestroyed, data);
  }

  [HarmonyPatch(typeof(LootSpawner), nameof(LootSpawner.UpdateSpawner)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> LootSpawnerUpdateSpawner(IEnumerable<CodeInstruction> instructions) =>
    new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(LootSpawner), nameof(LootSpawner.m_items))))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DropTablePatches), nameof(ApplyDrops), [typeof(LootSpawner)])))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InstructionEnumeration();

  static void ApplyDrops(LootSpawner __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_items, data);
  }

  [HarmonyPatch(typeof(MineRock), nameof(MineRock.RPC_Hit)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> MineRockRPC_Hit(IEnumerable<CodeInstruction> instructions) =>
    new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(MineRock), nameof(MineRock.m_dropItems))))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DropTablePatches), nameof(ApplyDrops), [typeof(MineRock)])))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InstructionEnumeration();

  static void ApplyDrops(MineRock __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_dropItems, data);
  }

  [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.DamageArea)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> MineRock5DamageArea(IEnumerable<CodeInstruction> instructions) =>
    new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(MineRock5), nameof(MineRock5.m_dropItems))))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DropTablePatches), nameof(ApplyDrops), [typeof(MineRock5)])))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InstructionEnumeration();

  static void ApplyDrops(MineRock5 __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_dropItems, data);
  }

  [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.RPC_Damage)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> TreeBaseRPC_Damage(IEnumerable<CodeInstruction> instructions) =>
    new CodeMatcher(instructions).MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TreeBase), nameof(TreeBase.m_dropWhenDestroyed))))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DropTablePatches), nameof(ApplyDrops), [typeof(TreeBase)])))
    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)).InstructionEnumeration();

  static void ApplyDrops(TreeBase __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_dropWhenDestroyed, data);
  }

  [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Destroy)), HarmonyPrefix]
  static void TreeLogDestroy(TreeLog __instance)
  {
    if (!Manager.TryGetData(__instance.m_nview, out var data)) return;
    Apply(__instance.m_dropWhenDestroyed, data);
  }


  private static void Apply(DropTable dropTable, Data data)
  {
    dropTable.m_dropChance = data.chance;
    dropTable.m_oneOfEach = data.oneOfEach;
    dropTable.m_dropMax = data.maxAmount;
    dropTable.m_dropMin = data.minAmount;
    dropTable.m_drops = [.. data.drops.Select(d => new DropTable.DropData
    {
      m_dontScale = d.dontScale,
      m_item = d.obj,
      m_stackMax = d.maxStack,
      m_stackMin = d.minStack,
      m_weight = d.weight
    })];
  }
}