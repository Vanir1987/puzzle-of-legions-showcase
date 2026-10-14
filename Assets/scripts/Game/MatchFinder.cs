using System.Collections.Generic;
using System.Linq;

public static class MatchFinder
{
    public static List<HashSet<Tile>> FindComponents(Tile[,] grid)
    {
        var seeds = new HashSet<Tile>();
        ScanLines(grid, true,  seeds);
        ScanLines(grid, false, seeds);

        var visited    = new HashSet<Tile>();
        var components = new List<HashSet<Tile>>();

        foreach (var seed in seeds)
        {
            if (visited.Contains(seed)) continue;

            var component = new HashSet<Tile>();
            var queue     = new Queue<Tile>();
            queue.Enqueue(seed);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!visited.Add(cur)) continue;
                component.Add(cur);

                for (var i = 0; i < 4; i++)
                {
                    var nb = cur.GetNeighbour(i);
                    if (nb == null || visited.Contains(nb)) continue;
                    if (nb.Item != cur.Item || nb.Item.tileType != TileType.Normal) continue;
                    queue.Enqueue(nb);
                }
            }
            components.Add(component);
        }
        return components;
    }

    private static void ScanLines(Tile[,] g, bool horizontal, HashSet<Tile> seeds)
    {
        var lines = g.GetLength(horizontal ? 1 : 0);
        var len   = g.GetLength(horizontal ? 0 : 1);

        for (var l = 0; l < lines; l++)
        {
            var i = 0;
            while (i < len)
            {
                var first = At(g, horizontal, l, i);
                if (first.Item.tileType != TileType.Normal) { i++; continue; }

                var run = 1;
                while (i + run < len && At(g, horizontal, l, i + run).Item == first.Item) run++;
                if (run >= 3)
                    for (var k = 0; k < run; k++) seeds.Add(At(g, horizontal, l, i + k));
                i += run;
            }
        }
    }

    private static Tile At(Tile[,] g, bool horizontal, int line, int i) =>
        horizontal ? g[i, line] : g[line, i];

    public static (TileType tier, List<Tile> used) Analyze(HashSet<Tile> component)
    {
        var bestRun = FindLongestRun(component);
        var extra   = component.FirstOrDefault(t => !bestRun.Contains(t));
        var runs    = RunTiles(component);

        var (tier, core) = bestRun.Count switch
        {
            >= 6 => (TileType.Timmy, bestRun.Take(6).ToList()),
            5    => extra != null ? (TileType.Timmy, bestRun.Append(extra).ToList()) : (TileType.Tier3, bestRun),
            4    => extra != null ? (TileType.Tier3, bestRun.Append(extra).ToList()) : (TileType.Tier2, bestRun),
            _    => (TileType.Normal, bestRun)
        };
        
        var leftover = bestRun.Count > 6 ? bestRun.Skip(6) : Enumerable.Empty<Tile>();
        return (tier, core.Union(runs).Except(leftover).ToList());
    }

    public static Tile GetCenterTile(HashSet<Tile> component)
    {
        var cx = component.Average(t => t.x);
        var cy = component.Average(t => t.y);
        return component.OrderBy(t => (t.x - cx) * (t.x - cx) + (t.y - cy) * (t.y - cy)).First();
    }

    private static HashSet<Tile> RunTiles(HashSet<Tile> c)
    {
        int Len(Tile t, System.Func<Tile, Tile> next)
        {
            var n = 0;
            for (var k = next(t); k != null && c.Contains(k); k = next(k)) n++;
            return n;
        }

        return c.Where(t => 1 + Len(t, k => k.Left) + Len(t, k => k.Right) >= 3
                         || 1 + Len(t, k => k.Top)  + Len(t, k => k.Bottom) >= 3).ToHashSet();
    }

    private static List<Tile> FindLongestRun(HashSet<Tile> component)
    {
        var best = new List<Tile>();

        foreach (var row in component.GroupBy(t => t.y))
            FindRunsInLine(row.OrderBy(t => t.x).ToList(), (a, b) => b.x == a.x + 1, ref best);

        foreach (var col in component.GroupBy(t => t.x))
            FindRunsInLine(col.OrderBy(t => t.y).ToList(), (a, b) => b.y == a.y + 1, ref best);

        return best;
    }

    private static void FindRunsInLine(List<Tile> sorted, System.Func<Tile, Tile, bool> isNext, ref List<Tile> best)
    {
        var run = new List<Tile> { sorted[0] };
        for (var i = 1; i < sorted.Count; i++)
        {
            if (isNext(sorted[i - 1], sorted[i])) run.Add(sorted[i]);
            else
            {
                if (run.Count > best.Count) best = run;
                run = new List<Tile> { sorted[i] };
            }
        }
        if (run.Count > best.Count) best = run;
    }
}
