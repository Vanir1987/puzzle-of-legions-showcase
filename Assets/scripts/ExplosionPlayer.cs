using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExplosionPlayer : MonoBehaviour
{
    public static ExplosionPlayer Instance { get; private set; }

    [SerializeField] private RectTransform layer;

    private readonly Stack<Image> _pool = new();

    private void Awake() => Instance = this;

    public void Play(ExplosionFx fx, Vector3 pos, float side, float delay = 0f, Material tint = null)
    {
        if (fx == null) return;
        var frames = fx.GetFrames(tint);
        if (frames.Length == 0) return;

        var img = _pool.Count > 0 ? _pool.Pop() : Create();
        var rt  = (RectTransform)img.transform;
        rt.position  = pos;
        rt.sizeDelta = Vector2.one * side / layer.lossyScale.x;

        var n = frames.Length;
        DOTween.To(() => 0f, v => img.sprite = frames[Mathf.Min((int)v, n - 1)], n, n / fx.fps)
            .SetEase(Ease.Linear).SetDelay(delay).SetLink(layer.gameObject)
            .OnStart(() => img.gameObject.SetActive(true))
            .OnComplete(() => { img.gameObject.SetActive(false); _pool.Push(img); });
    }

    private Image Create()
    {
        var go  = new GameObject("explosion", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(layer, false);
        var img = go.GetComponent<Image>();
        img.raycastTarget  = false;
        img.preserveAspect = true;
        go.SetActive(false);
        return img;
    }

    public Dictionary<Tile, float> PlayWave(ExplosionFx fx, Tile origin, IEnumerable<Tile> tiles, Material tint = null)
    {
        var o = origin ?? tiles.First();
        var delays = new Dictionary<Tile, float>();
        foreach (var t in tiles)
        {
            var d = (Mathf.Abs(t.x - o.x) + Mathf.Abs(t.y - o.y)) * fx.waveStep;
            delays[t] = d;
            Play(fx, t.transform.position, CellSize(t).x * fx.scale, d, tint);
        }
        return delays;
    }

    public void PlayBlast(ExplosionFx fx, IEnumerable<Tile> area, Material tint = null)
    {
        Bounds b = default;
        var first = true;
        foreach (var t in area)
        {
            var c = new Bounds(t.transform.position, CellSize(t));
            if (first) { b = c; first = false; } else b.Encapsulate(c);
        }
        if (first) return;
        Play(fx, b.center, Mathf.Max(b.size.x, b.size.y) * fx.scale, 0f, tint);
    }

    private static Vector3 CellSize(Tile t)
    {
        var rt = (RectTransform)t.transform;
        return Vector3.Scale(rt.rect.size, rt.lossyScale);
    }
}
