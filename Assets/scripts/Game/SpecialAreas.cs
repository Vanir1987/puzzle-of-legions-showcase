using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpecialAreas
{
    public static HashSet<Tile> Line(Tile[,] g, Tile origin, bool horizontal)
    {
        var set = new HashSet<Tile>();
        var len = g.GetLength(horizontal ? 0 : 1);
        for (var i = 0; i < len; i++)
            set.Add(horizontal ? g[i, origin.y] : g[origin.x, i]);
        return set;
    }

    public static HashSet<Tile> Square(Tile[,] g, Tile center)
    {
        var set = new HashSet<Tile>();
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
        {
            var x = center.x + dx;
            var y = center.y + dy;
            if (x >= 0 && x < g.GetLength(0) && y >= 0 && y < g.GetLength(1)) set.Add(g[x, y]);
        }
        return set;
    }

    public static HashSet<Tile> All(Tile[,] g) => g.Cast<Tile>().ToHashSet();

    public static Tile RandomTarget(Tile[,] g, Tile origin)
    {
        var padX = g.GetLength(0) >= 3 ? 1 : 0;
        var padY = g.GetLength(1) >= 3 ? 1 : 0;

        int x, y, attempts = 0;
        do
        {
            x = Random.Range(padX, g.GetLength(0) - padX);
            y = Random.Range(padY, g.GetLength(1) - padY);
        }
        while (x == origin.x && y == origin.y && ++attempts < 20);

        return g[x, y];
    }
}
