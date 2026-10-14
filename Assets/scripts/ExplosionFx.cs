using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum ExplosionMode { Wave, Blast }

[CreateAssetMenu(menuName = "Match-3/Explosion Fx")]
public sealed class ExplosionFx : ScriptableObject
{
    public ExplosionMode mode;
    public Sprite[] frames;
    public float fps = 24f;
    [Tooltip("Размер относительно ячейки (Wave) или стороны области (Blast)")]
    public float scale = 1.3f;
    [Tooltip("Wave: задержка на каждую клетку расстояния от центра, сек")]
    public float waveStep = 0.06f;
    [Header("Процедурный взрыв (если Frames пуст)")]
    public int frameCount = 16;
    public int size = 96;
    public int seed;
    public Color hot  = new(1f, 0.95f, 0.7f);
    public Color mid  = new(1f, 0.55f, 0.1f);
    public Color cool = new(0.35f, 0.08f, 0.05f);

    [System.NonSerialized] private Sprite[] _generated;
    [System.NonSerialized] private Dictionary<Object, Sprite[]> _cache;

    private (Color hot, Color mid, Color cool) Palette(Material tint)
    {
        Color f = default, k = default;
        var custom = tint != null
            && TryGet(tint, "_FlameColor", "_Flame_Color", out f)
            && TryGet(tint, "_CoreColor",  "_Core_Color",  out k);

        var flame = custom ? Norm(f) : mid;
        return (custom ? Norm(k) : hot, flame, custom ? flame * 0.3f : cool);
    }

    public Sprite[] GetFrames(Material tint = null)
    {
        if (frames is { Length: > 0 }) return frames;

        _cache ??= new();
        Object key = tint != null ? tint : this;
        if (_cache.TryGetValue(key, out var cached)) return cached;

        var (h, m, c) = Palette(tint);
        return _cache[key] = ExplosionGenerator.Make(this, h, m, c);
    }

    public int PendingFrames(Material tint = null)
    {
        if (frames is { Length: > 0 }) return 0;
        Object key = tint != null ? tint : this;
        return _cache != null && _cache.ContainsKey(key) ? 0 : Mathf.Max(2, frameCount);
    }

    public async Task WarmAsync(Material tint = null, System.Action onFrame = null)
    {
        if (frames is { Length: > 0 }) return;

        _cache ??= new();
        Object key = tint != null ? tint : this;
        if (_cache.ContainsKey(key)) return;

        var (h, m, c) = Palette(tint);
        int n = Mathf.Max(2, frameCount), s = size, sd = seed;

        var px = await Task.Run(() => ExplosionGenerator.Render(n, s, sd, h, m, c, onFrame));
        if (!_cache.ContainsKey(key)) _cache[key] = ExplosionGenerator.ToSprites(px, s);
    }

    private static bool TryGet(Material m, string a, string b, out Color c)
    {
        var n = m.HasColor(a) ? a : m.HasColor(b) ? b : null;
        c = n != null ? m.GetColor(n) : default;
        return n != null;
    }

    private static Color Norm(Color c)
    {
        var m = Mathf.Max(c.r, c.g, c.b, 1f);
        return new Color(c.r / m, c.g / m, c.b / m, 1f);
    }
}
