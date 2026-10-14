using UnityEngine;
 
public static class ItemDatabase
{
    public static Item[] Items      { get; private set; }
    public static Item[] Tier2Items { get; private set; }
    public static Item[] Tier3Items { get; private set; }
    public static Item[] TimmyItems { get; private set; }
 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Items      = Resources.LoadAll<Item>("Items/Tier1/");
        Tier2Items = Resources.LoadAll<Item>("Items/Tier2/");
        Tier3Items = Resources.LoadAll<Item>("Items/Tier3/");
        TimmyItems = Resources.LoadAll<Item>("Items/Timmy/");

        foreach (var arr in new[] { Tier2Items, Tier3Items, TimmyItems })
            System.Array.Sort(arr, (a, b) => string.CompareOrdinal(a.name, b.name));
    }

    public static Item[] GetTier(TileType t) => t switch
    {
        TileType.Tier2 => Tier2Items,
        TileType.Tier3 => Tier3Items,
        TileType.Timmy => TimmyItems,
        _              => System.Array.Empty<Item>()
    };
}
