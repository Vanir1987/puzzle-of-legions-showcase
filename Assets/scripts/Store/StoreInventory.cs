using System;
using System.Linq;
using UnityEngine;

public static class StoreInventory
{
    public static event Action OnChanged;

    private static StoreItemData[] _all;
    private static StoreItemData[] All => _all ??= Resources.LoadAll<StoreItemData>("Store items");

    private static string OwnedKey(StoreItemData i) => "Owned_" + i.Id;

    public static bool IsOwned(StoreItemData item) =>
        item.isDefault || item.cost == 0 || PlayerPrefs.GetInt(OwnedKey(item), 0) == 1;

    public static bool TryBuy(StoreItemData item)
    {
        if (IsOwned(item) || !CurrencyManager.TrySpend(item.cost)) return false;
        PlayerPrefs.SetInt(OwnedKey(item), 1);
        Commit();
        return true;
    }

    private static string LevelKey(StoreItemData i) => "Level_" + i.Id;

    public static int GetLevel(StoreItemData item) =>
        Mathf.Clamp(PlayerPrefs.GetInt(LevelKey(item), 1), 1, item.MaxLevel);

    public static float GetStat(StoreItemData item) =>
        item.statValues.Length > 0 ? item.statValues[GetLevel(item) - 1] : 0f;

    public static int UpgradeCost(StoreItemData item) => item.upgradeCosts[GetLevel(item) - 1];
    
    public static bool TryUpgrade(StoreItemData item)
    {
        var lvl = GetLevel(item);
        if (!IsOwned(item) || !item.Upgradable || lvl >= item.MaxLevel) return false;
        if (!CurrencyManager.TrySpend(item.upgradeCosts[lvl - 1])) return false;

        PlayerPrefs.SetInt(LevelKey(item), lvl + 1);
        Commit();
        return true;
    }

    public static int SlotCount(StoreItemData item) => item.category switch
    {
        StoreCategory.Board   => 1,
        StoreCategory.Booster => 3,
        _                     => ItemDatabase.GetTier(item.tileTier).Length
    };

    private static string TileKey(TileType tier, int slot) => $"Slot_Tile_{tier}_{slot}";

    private static string SlotKey(StoreItemData i, int slot) =>
        i.category == StoreCategory.Tile ? TileKey(i.tileTier, slot)
                                        : $"Slot_{i.category}__{slot}";

    private static StoreItemData DefaultFor(StoreItemData like, int slot) =>
        like.category == StoreCategory.Tile ? null
        : All.FirstOrDefault(x => x.category == like.category && x.DefaultSlot == slot);

    public static StoreItemData GetSlotItem(StoreItemData like, int slot)
    {
        var key = SlotKey(like, slot);
        if (!PlayerPrefs.HasKey(key)) return DefaultFor(like, slot);
        var id = PlayerPrefs.GetString(key);
        return All.FirstOrDefault(x => x.Id == id);
    }

    private static void Write(StoreItemData like, int slot, StoreItemData value) =>
        PlayerPrefs.SetString(SlotKey(like, slot), value != null ? value.Id : "");

    public static int SlotOf(StoreItemData item)
    {
        for (var s = 0; s < SlotCount(item); s++)
            if (GetSlotItem(item, s) == item) return s;
        return -1;
    }

    public static bool IsEquipped(StoreItemData item) => SlotOf(item) >= 0;

    public static bool CanUnequip(StoreItemData item) =>
        item.category != StoreCategory.Board && SlotOf(item) >= 0;

    public static void EquipToSlot(StoreItemData item, int slot)
    {
        var from      = SlotOf(item);
        var displaced = GetSlotItem(item, slot);
        if (from >= 0) Write(item, from, displaced);
        Write(item, slot, item);
        Commit();
    }

    public static void Unequip(StoreItemData item)
    {
        if (!CanUnequip(item)) return;
        Write(item, SlotOf(item), null);
        Commit();
    }

    private static void Commit()
    {
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }

    public static Item Resolve(Item baseItem)
    {
        if (baseItem == null || baseItem.tileType == TileType.Normal) return baseItem;

        var slot = System.Array.IndexOf(ItemDatabase.GetTier(baseItem.tileType), baseItem);
        if (slot < 0) return baseItem;

        var key = TileKey(baseItem.tileType, slot);
        if (!PlayerPrefs.HasKey(key)) return baseItem;

        var id          = PlayerPrefs.GetString(key);
        var replacement = All.FirstOrDefault(x => x.Id == id)?.gameItem;
        return replacement != null ? replacement : baseItem;
    }

    public static StoreItemData GetBoosterSlot(int slot)
    {
        var key = $"Slot_{StoreCategory.Booster}__{slot}";
        return PlayerPrefs.HasKey(key) ? All.FirstOrDefault(x => x.Id == PlayerPrefs.GetString(key)) : null;
    }

    public static float GetBoosterStat(BoosterEffect effect)
    {
        for (var s = 0; s < 3; s++)
        {
            var b = GetBoosterSlot(s);
            if (b != null && b.effect == effect) return GetStat(b);
        }
        return 0f;
    }

    public static StoreItemData GetEquippedBoard()
    {
        var any = All.FirstOrDefault(x => x.category == StoreCategory.Board);
        return any != null ? GetSlotItem(any, 0) : null;
    }
}
