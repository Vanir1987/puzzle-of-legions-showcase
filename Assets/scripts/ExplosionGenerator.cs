using UnityEngine;

public static class ExplosionGenerator
{
    public static Color32[][] Render(int n, int s, int seed, Color hot, Color mid, Color cool,
                                    System.Action onFrame = null)
    {
        var result = new Color32[n][];
        var off = new Vector2(seed * 7.31f, seed * 3.17f);

        for (var f = 0; f < n; f++)
        {
            var px     = new Color32[s * s];
            var t      = f / (n - 1f);
            var radius = Mathf.Lerp(0.5f, 0.75f, 1f - (1f - t) * (1f - t));

            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var p = new Vector2(x, y) / (s - 1f) * 2f - Vector2.one;
                var r = p.magnitude;

                var d    = r + (Fbm(p * 2.5f + off + Vector2.one * t * 0.6f) - 0.5f) * 0.5f;
                var body = Mathf.Clamp01((radius - d) / 0.22f);

                var heat = Mathf.Clamp01(body * 1.3f - t * 0.8f);
                var col  = heat > 0.5f ? Color.Lerp(mid, hot, (heat - 0.5f) * 2f)
                                    : Color.Lerp(cool, mid, heat * 2f);

                var k    = (r - radius * 1.1f) / 0.05f;
                var ring = Mathf.Exp(-k * k) * (1f - t);
                col = Color.Lerp(col, hot, Mathf.Clamp01(ring));

                col.a = Mathf.Clamp01(body * 8f + ring * 0.6f);
                px[y * s + x] = col;
            }
            result[f] = px;
            onFrame?.Invoke();
        }
        return result;
    }

    public static Sprite[] ToSprites(Color32[][] frames, int s)
    {
        var sprites = new Sprite[frames.Length];
        for (var f = 0; f < frames.Length; f++)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            tex.SetPixels32(frames[f]);
            tex.Apply(false, true);
            sprites[f] = Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * 0.5f, s);
        }
        return sprites;
    }

    public static Sprite[] Make(ExplosionFx fx, Color hot, Color mid, Color cool) =>
        ToSprites(Render(Mathf.Max(2, fx.frameCount), fx.size, fx.seed, hot, mid, cool), fx.size);

    private static float Hash(float x, float y) =>
        Mathf.Repeat(Mathf.Sin(x * 127.1f + y * 311.7f) * 43758.5453f, 1f);

    private static float Noise(Vector2 p)
    {
        float ix = Mathf.Floor(p.x), iy = Mathf.Floor(p.y);
        float fx = p.x - ix, fy = p.y - iy;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);
        return Mathf.Lerp(Mathf.Lerp(Hash(ix, iy),     Hash(ix + 1, iy),     fx),
                          Mathf.Lerp(Hash(ix, iy + 1), Hash(ix + 1, iy + 1), fx), fy);
    }

    private static float Fbm(Vector2 p) =>
        0.55f * Noise(p) + 0.3f * Noise(p * 2f) + 0.15f * Noise(p * 4f);
}
