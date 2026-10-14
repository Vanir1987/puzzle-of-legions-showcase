using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StoreBalance
{
    private const int BoosterSlots = 3;

    public static int UpgradeTotal(StoreItemData i) => i.upgradeCosts.Sum();

    public static int Total(StoreItemData i) => i.cost + UpgradeTotal(i);

    public static float GamesToAfford(StoreItemData i, float goldPerGame) => Total(i) / Mathf.Max(1f, goldPerGame);

    public static float CostPerPoint(StoreItemData i, int step)
    {
        var delta = Mathf.Abs(i.statValues[step + 1] - i.statValues[step]);
        return delta > 0f ? i.upgradeCosts[step] / delta : float.PositiveInfinity;
    }

    public static IEnumerable<string> Validate(StoreItemData i)
    {
        if (i.icon == null) yield return "нет иконки";
        if (string.IsNullOrWhiteSpace(i.displayName)) yield return "пустое имя";
        if (i.cost < 0) yield return "отрицательная цена";
        if (i.upgradeCosts.Any(c => c <= 0)) yield return "цена апгрейда <= 0";
        if (i.statValues.Length > 1 && !IsMonotonic(i.statValues)) yield return "statValues не монотонны";

        switch (i.category)
        {
            case StoreCategory.Booster:
                if (i.effect == BoosterEffect.None) yield return "бустер без эффекта";
                if (i.effect != BoosterEffect.None && i.statValues.Length == 0) yield return "нет statValues";
                if (i.isDefault && (i.defaultSlot < 0 || i.defaultSlot >= BoosterSlots)) yield return "defaultSlot вне 0..2";
                break;
            case StoreCategory.Tile:
                if (i.gameItem == null) yield return "не задан Game Item";
                else if (i.gameItem.tileType != i.tileTier) yield return "тип Game Item не совпадает с Tile Tier";
                if (i.tileTier == TileType.Normal) yield return "Tile Tier = Normal";
                break;
            case StoreCategory.Board:
                if (i.theme == null) yield return "не задана тема";
                break;
        }

        if (i.category != StoreCategory.Booster && i.statValues.Length > 0) yield return "statValues у не-бустера";
    }

    public static IEnumerable<string> ValidateAll(IEnumerable<StoreItemData> all) =>
        all.Where(i => i.isDefault && i.category != StoreCategory.Tile)
           .GroupBy(i => (i.category, i.defaultSlot))
           .Where(g => g.Count() > 1)
           .Select(g => $"{g.Key.category}, слот {g.Key.defaultSlot}: несколько предметов по умолчанию ({string.Join(", ", g.Select(i => i.name))})");

    private static bool IsMonotonic(float[] v)
    {
        bool up = true, down = true;
        for (var k = 1; k < v.Length; k++)
        {
            up   &= v[k] > v[k - 1];
            down &= v[k] < v[k - 1];
        }
        return up || down;
    }

    public static float ExtraGoldPerGame(StoreItemData i, int level, float goldPerGame) =>
    i.effect == BoosterEffect.GoldBonus ? goldPerGame * i.statValues[level - 1] / 100f : 0f;

    public static float PurchasePayback(StoreItemData i, float goldPerGame)
    {
        var extra = ExtraGoldPerGame(i, 1, goldPerGame);
        return extra > 0f ? i.cost / extra : float.PositiveInfinity;
    }

    public static float PaybackGames(StoreItemData i, int step, float goldPerGame)
    {
        var gain = ExtraGoldPerGame(i, step + 2, goldPerGame) - ExtraGoldPerGame(i, step + 1, goldPerGame);
        return gain > 0f ? i.upgradeCosts[step] / gain : float.PositiveInfinity;
    }

    public static IEnumerable<string> ValidatePricing(IEnumerable<StoreItemData> all)
    {
        var paid = all.Where(i => i.cost > 0).ToList();
        var cheapestCosmetic = paid.Where(i => i.category != StoreCategory.Booster)
            .Select(i => i.cost).DefaultIfEmpty(int.MaxValue).Min();
        return paid.Where(i => i.category == StoreCategory.Booster && i.cost >= cheapestCosmetic)
            .Select(i => $"{i.name}: бустер не дешевле самой дешёвой косметики ({cheapestCosmetic})");
    }
}
