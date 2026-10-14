using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public static class BoardFiller
{
    public static (Dictionary<Tile, Tile> remap, Sequence sequence) Prepare(
        Tile[,] tiles, HashSet<Tile> cleared, float duration, HashSet<Tile> hidden = null)
    {
        var remap     = new Dictionary<Tile, Tile>();
        var sequence  = DOTween.Sequence();
        var height    = tiles.GetLength(1);

        for (var x = 0; x < tiles.GetLength(0); x++)
        {
            var free = height - 1;

            for (var y = height - 1; y >= 0; y--)
            {
                var source = tiles[x, y];
                if (cleared.Contains(source)) continue;

                var target = tiles[x, free--];
                remap[source] = target;
                Drop(target, source, hidden != null && hidden.Contains(source), sequence, duration);
            }

            var step = height > 1
                ? Mathf.Abs(tiles[x, 0].transform.position.y - tiles[x, 1].transform.position.y)
                : 100f;

            for (var i = 0; i <= free; i++)
                Spawn(tiles[x, i], tiles[x, 0].transform.position.y + step * (free + 1 - i), sequence, duration);
        }

        return (remap, sequence);
    }

    private static void Drop(Tile target, Tile source, bool hidden, Sequence sequence, float duration)
    {
        target.SetItem(source.Item);
        target.icon.transform.localScale = hidden ? Vector3.zero : Vector3.one;
        if (source == target) return;

        target.icon.transform.position = source.transform.position;
        sequence.Join(target.icon.transform.DOMove(target.transform.position, duration));
    }

    private static void Spawn(Tile tile, float startY, Sequence sequence, float duration)
    {
        tile.SetItem(ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)]);
        tile.icon.transform.localScale = Vector3.one;

        var pos = tile.transform.position;
        tile.icon.transform.position = new Vector3(pos.x, startY, pos.z);
        sequence.Join(tile.icon.transform.DOMove(pos, duration));
    }
}
