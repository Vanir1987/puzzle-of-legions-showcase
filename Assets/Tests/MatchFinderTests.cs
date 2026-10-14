using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class MatchFinderTests
{
    private readonly List<Object> _created = new();
    private readonly Dictionary<char, Item> _items = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var o in _created) Object.DestroyImmediate(o);
        _created.Clear();
        _items.Clear();
    }

    private Item ItemFor(char c)
    {
        if (_items.TryGetValue(c, out var item)) return item;
        item = ScriptableObject.CreateInstance<Item>();
        item.tileType = c == '#' ? TileType.Tier2 : TileType.Normal;
        _created.Add(item);
        return _items[c] = item;
    }

    private Tile[,] Build(params string[] rows)
    {
        var grid = new Tile[rows[0].Length, rows.Length];
        for (var y = 0; y < rows.Length; y++)
        for (var x = 0; x < rows[y].Length; x++)
        {
            var go   = new GameObject("tile", typeof(RectTransform), typeof(Image));
            var tile = go.AddComponent<Tile>();
            tile.icon = go.GetComponent<Image>();
            tile.x    = x;
            tile.y    = y;
            tile.Item = ItemFor(rows[y][x]);
            grid[x, y] = tile;
            _created.Add(go);
        }
        foreach (var t in grid) t.Grid = grid;
        return grid;
    }

    [Test] public void NoRun_NoComponents() =>
        Assert.AreEqual(0, MatchFinder.FindComponents(Build("AABB", "BBAA")).Count);

    [Test] public void Run3_OneComponentOf3()
    {
        var comps = MatchFinder.FindComponents(Build("AAAB", "BCBC"));
        Assert.AreEqual(1, comps.Count);
        Assert.AreEqual(3, comps[0].Count);
    }

    [Test] public void TwoSeparateRuns_TwoComponents() =>
        Assert.AreEqual(2, MatchFinder.FindComponents(Build("AAAB", "CCCB")).Count);

    [Test] public void SpecialTiles_AreNotMatched() =>
        Assert.AreEqual(0, MatchFinder.FindComponents(Build("###A", "ABAB")).Count);

    [Test] public void LShape_MergesIntoOneComponentOf5()
    {
        var comps = MatchFinder.FindComponents(Build("AAA", "BCA", "CBA"));
        Assert.AreEqual(1, comps.Count);
        Assert.AreEqual(5, comps[0].Count);
    }

    [TestCase("AAA",    "BCB",    TileType.Normal, 3)]
    [TestCase("AAAA",   "BCBC",   TileType.Tier2,  4)]
    [TestCase("AAAA",   "BABC",   TileType.Tier3,  5)]
    [TestCase("AAAAA",  "BCBCB",  TileType.Tier3,  5)]
    [TestCase("AAAAA",  "BABCB",  TileType.Timmy,  6)]
    [TestCase("AAAAAA", "BCBCBC", TileType.Timmy,  6)]
    [TestCase("AAAAAAA", "BCBCBCB", TileType.Timmy, 6)]
    public void Analyze_GivesExpectedTierAndUsedCount(string row0, string row1, TileType tier, int used)
    {
        var comp = MatchFinder.FindComponents(Build(row0, row1))[0];
        var (actualTier, actualUsed) = MatchFinder.Analyze(comp);
        Assert.AreEqual(tier, actualTier);
        Assert.AreEqual(used, actualUsed.Count);
    }

    [Test] public void GetCenterTile_ReturnsMiddleOfRun()
    {
        var comp = MatchFinder.FindComponents(Build("AAA", "BCB"))[0];
        Assert.AreEqual(1, MatchFinder.GetCenterTile(comp).x);
    }
}
