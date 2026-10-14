using NUnit.Framework;
using UnityEngine;

public sealed class SpecialAreasTests
{
    private Tile[,] _grid;

    [SetUp]
    public void SetUp()
    {
        _grid = new Tile[5, 5];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 5; y++)
        {
            var tile = new GameObject().AddComponent<Tile>();
            tile.x = x;
            tile.y = y;
            _grid[x, y] = tile;
        }
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var t in _grid) Object.DestroyImmediate(t.gameObject);
    }

    [Test] public void Square_InCenter_Has9Tiles() =>
        Assert.AreEqual(9, SpecialAreas.Square(_grid, _grid[2, 2]).Count);

    [Test] public void Square_InCorner_Has4Tiles() =>
        Assert.AreEqual(4, SpecialAreas.Square(_grid, _grid[0, 0]).Count);

    [Test] public void Line_Horizontal_CoversWholeRow() =>
        Assert.AreEqual(5, SpecialAreas.Line(_grid, _grid[1, 3], true).Count);

    [Test] public void All_ReturnsEveryTile() =>
        Assert.AreEqual(25, SpecialAreas.All(_grid).Count);

    [Test] public void RandomTarget_NeverOnBorder()
    {
        for (var i = 0; i < 100; i++)
        {
            var t = SpecialAreas.RandomTarget(_grid, _grid[2, 2]);
            Assert.That(t.x, Is.InRange(1, 3));
            Assert.That(t.y, Is.InRange(1, 3));
        }
    }
}
