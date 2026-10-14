using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;
    public Tile[,] Tiles;

    [Header("Звуки")]
    [Tooltip("Проигрывается, если свайп привёл к совпадению или активировал спец-тайл")]
    public AudioClip[] swapSuccessSounds;
    [Tooltip("Проигрывается, если свайп откатился обратно (совпадений не найдено)")]
    public AudioClip[] swapFailSounds;
    [Tooltip("Звук взрыва бустера-робогоблина")]
    public AudioClip[] bombSounds;

    [Header("Бонусное время за активацию спец-тайлов (секунды)")]
    public float tier2BonusTime = 3f;
    public float tier3BonusTime = 5f;
    public float timmyBonusTime = 10f;

    public int Width  => Tiles.GetLength(0);
    public int Height => Tiles.GetLength(1);

    private bool _isProcessing;
    private AudioSource _audioSource;

    [Header("Эффекты взрывов")]
    public ExplosionFx tier2Fx, tier3Fx, timmyFx, bombFx;
    public bool IsReady { get; private set; }
    private int _warmedFrames, _totalFrames;
    public float WarmProgress => IsReady ? 1f
        : _totalFrames == 0 ? 0f
        : Mathf.Clamp01((float)System.Threading.Volatile.Read(ref _warmedFrames) / _totalFrames);


    private readonly HashSet<Tile> _protectedThisTurn      = new();
    private readonly HashSet<int>  _touchedColumnsThisTurn = new();

    private const float TweenDuration   = 0.25f;
    private const float FlyDuration     = 0.45f;

    private void Awake()
    {
        Instance     = this;
        _audioSource = GetComponent<AudioSource>();

        ApplySfxVolume();
        GameAudioSettings.OnChanged += ApplySfxVolume;
    }

    private void OnDestroy() => GameAudioSettings.OnChanged -= ApplySfxVolume;

    private void ApplySfxVolume() => _audioSource.volume = GameAudioSettings.EffectiveSfxVolume;

    public void PauseSfx() => _audioSource.Pause();

    public void ResumeSfx() => _audioSource.UnPause();

    private void Start()
    {
        Tiles = new Tile[rows.Max(r => r.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var tile = rows[y].tiles[x];
            tile.x    = x;
            tile.y    = y;
            tile.Grid = Tiles;
            tile.Item = GetRandomItemWithoutMatch(x, y);
            Tiles[x, y] = tile;
        }

        _ = WarmAllAsync();
    }

    private async Task WarmAllAsync()
    {
        var jobs = new List<(ExplosionFx fx, Material mat)>();
        var seen = new HashSet<(ExplosionFx, Material)>();

        void Add(ExplosionFx fx, Material mat)
        {
            if (fx != null && seen.Add((fx, mat))) jobs.Add((fx, mat));
        }
        void AddTier(ExplosionFx fx, Item[] items)
        {
            Add(fx, null);
            foreach (var i in items) Add(fx, StoreInventory.Resolve(i).fxMaterial);
        }

        AddTier(tier2Fx, ItemDatabase.Tier2Items);
        AddTier(tier3Fx, ItemDatabase.Tier3Items);
        AddTier(timmyFx, ItemDatabase.TimmyItems);
        AddTier(bombFx,  System.Array.Empty<Item>());

        foreach (var j in jobs) _totalFrames += j.fx.PendingFrames(j.mat);
        void OnFrame() => System.Threading.Interlocked.Increment(ref _warmedFrames);

        try { foreach (var j in jobs) await j.fx.WarmAsync(j.mat, OnFrame); }
        catch (System.Exception e) { Debug.LogException(e); }
        finally { IsReady = true; }
    }

    private Item GetRandomItemWithoutMatch(int x, int y)
    {
        var forbidden = new HashSet<Item>();

        if (x >= 2 && Tiles[x - 1, y].Item == Tiles[x - 2, y].Item)
            forbidden.Add(Tiles[x - 1, y].Item);

        if (y >= 2 && Tiles[x, y - 1].Item == Tiles[x, y - 2].Item)
            forbidden.Add(Tiles[x, y - 1].Item);

        if (forbidden.Count == 0)
            return ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

        var candidates = ItemDatabase.Items.Where(i => !forbidden.Contains(i)).ToList();
        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
    }

    public async Task ActivateRemainingSpecials()
    {
        while (_isProcessing) await Task.Yield();
        _isProcessing = true;

        while (true)
        {
            var comps = GetMatchComponents();
            if (comps.Count > 0) { await Pop(comps, null); continue; }

            var stray = FindAnySpecialOnBoard();
            if (stray == null) break;
            await ActivateSpecialTile(stray);
        }

        _isProcessing = false;
    }

    private Tile FindAnySpecialOnBoard()
    {
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var tile = Tiles[x, y];
            if (tile.Item != null && tile.Item.tileType != TileType.Normal)
                return tile;
        }
        return null;
    }

    public async void TrySwap(Tile tile1, Tile tile2)
    {
        if (_isProcessing || tile1 == null || tile2 == null) return;
        await RunTurn(() => SwapRoutine(tile1, tile2));
    }

    private async Task RunTurn(System.Func<Task> routine)
    {
        _isProcessing = true;
        _protectedThisTurn.Clear();
        _touchedColumnsThisTurn.Clear();
        try { await routine(); }
        catch (System.Exception e) { Debug.LogException(e); }
        finally { _isProcessing = false; }
    }

    private async Task SwapRoutine(Tile tile1, Tile tile2)
    {
        SwapData(tile1, tile2);
        var swapAnim = AnimateSwapVisual(tile1, tile2);

        var t1Special = tile1.Item.tileType != TileType.Normal;
        var t2Special = tile2.Item.tileType != TileType.Normal;

        if (t1Special || t2Special)
        {
            PlayRandomClip(swapSuccessSounds);
            await swapAnim;

            if (t1Special) await ActivateSpecialTile(tile1);
            if (t2Special && tile2.Item != null && tile2.Item.tileType != TileType.Normal)
                await ActivateSpecialTile(tile2);

            await ResolveBoard();
        }
        else if (CanPop())
        {
            PlayRandomClip(swapSuccessSounds);
            await swapAnim;
            await ResolveBoard(tile2);
        }
        else
        {
            PlayRandomClip(swapFailSounds);
            await swapAnim;
            SwapData(tile1, tile2);
            await AnimateSwapVisual(tile1, tile2);
        }
    }

    private async Task ResolveBoard(Tile activeTile = null)
    {
        while (true)
        {
            var comps = GetMatchComponents();
            if (comps.Count > 0)
            {
                await Pop(comps, activeTile);
                activeTile = null;
                continue;
            }

            var stray = FindUnprotectedSpecial();
            if (stray == null) break;
            await ActivateSpecialTile(stray);
        }
    }

    private Tile FindUnprotectedSpecial()
    {
        foreach (var x in _touchedColumnsThisTurn)
        for (var y = 0; y < Height; y++)
        {
            var tile = Tiles[x, y];
            if (tile.Item != null && tile.Item.tileType != TileType.Normal && !_protectedThisTurn.Contains(tile))
                return tile;
        }
        return null;
    }

    private void SwapData(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var t1 = icon1.transform;
        var t2 = icon2.transform;

        var worldPos1 = t1.position;
        var worldPos2 = t2.position;

        t1.SetParent(tile2.transform);
        t2.SetParent(tile1.transform);

        t1.position = worldPos1;
        t2.position = worldPos2;

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tmp = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tmp;
    }

    private async Task AnimateSwapVisual(Tile tile1, Tile tile2)
    {
        var seq = DOTween.Sequence();
        seq.Join(tile1.icon.transform.DOMove(tile1.transform.position, TweenDuration));
        seq.Join(tile2.icon.transform.DOMove(tile2.transform.position, TweenDuration));
        await seq.Play().AsyncWaitForCompletion();
    }

    private List<HashSet<Tile>> GetMatchComponents() => MatchFinder.FindComponents(Tiles);

    private bool CanPop() => GetMatchComponents().Count > 0;

    private async Task Pop(List<HashSet<Tile>> components, Tile activeTile)
    {
        foreach (var component in components)
            foreach (var t in component)
                _touchedColumnsThisTurn.Add(t.x);

        var allToClear  = new HashSet<Tile>();
        var spawnQueue  = new List<(Tile tile, TileType tier, Item sourceItem)>();

        foreach (var component in components)
        {
            var (tier, used) = MatchFinder.Analyze(component);
            var usedSet = new HashSet<Tile>(used);

            var bonus = (tier == TileType.Normal && used.Count > 3) ? 2 : 1;
            foreach (var t in used)
                ScoreManager.Instance.AddTileScore(t.Item.value * bonus);

            PlayMatchSounds(usedSet);

            if (tier == TileType.Normal)
            {
                foreach (var t in used) allToClear.Add(t);
            }
            else
            {
                var spawnTile = (activeTile != null && usedSet.Contains(activeTile))
                    ? activeTile
                    : MatchFinder.GetCenterTile(usedSet);

                var sourceItem = used[0].Item;
                spawnQueue.Add((spawnTile, tier, sourceItem));

                foreach (var t in used)
                    if (t != spawnTile) allToClear.Add(t);
            }
        }

        var deflate = DOTween.Sequence();
        foreach (var t in allToClear)
            deflate.Join(t.icon.transform.DOScale(Vector3.zero, TweenDuration));
        foreach (var (sp, _, _) in spawnQueue)
            deflate.Join(sp.icon.transform.DOScale(Vector3.zero, TweenDuration));

        await deflate.Play().AsyncWaitForCompletion();

        foreach (var (sp, tier, sourceItem) in spawnQueue)
        {
            var item = StoreInventory.Resolve(PickSpecialItem(tier, sourceItem));
            sp.SetItem(item);
        }

        var justSpawned = spawnQueue.Select(s => s.tile).ToHashSet();
        var (remap, fillSequence) = PrepareFillBoard(allToClear, justSpawned);

        foreach (var (sp, _, _) in spawnQueue)
        {
            var finalTile = remap[sp];
            _protectedThisTurn.Add(finalTile);
            fillSequence.Join(finalTile.icon.transform.DOScale(Vector3.one, TweenDuration).SetEase(Ease.OutBack));
        }

        await fillSequence.Play().AsyncWaitForCompletion();
    }

    private Item PickSpecialItem(TileType tier, Item source)
    {
        switch (tier)
        {
            case TileType.Tier2:
                if (source != null && source.tier2Upgrade != null)
                    return source.tier2Upgrade;
                if (ItemDatabase.Tier2Items is { Length: > 0 })
                    return ItemDatabase.Tier2Items[Random.Range(0, ItemDatabase.Tier2Items.Length)];
                break;

            case TileType.Tier3:
                if (source != null && source.tier3Upgrade != null)
                    return source.tier3Upgrade;
                if (ItemDatabase.Tier3Items is { Length: > 0 })
                    return ItemDatabase.Tier3Items[Random.Range(0, ItemDatabase.Tier3Items.Length)];
                break;

            case TileType.Timmy:
                if (ItemDatabase.TimmyItems is { Length: > 0 })
                    return ItemDatabase.TimmyItems[Random.Range(0, ItemDatabase.TimmyItems.Length)];
                break;
        }
        return ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
    }

    private async Task ActivateSpecialTile(Tile tile)
    {
        if (tile?.Item == null) return;

        PlaySound(tile.Item);

        switch (tile.Item.tileType)
        {
            case TileType.Tier2: await ActivateTier2(tile); break;
            case TileType.Tier3: await ActivateTier3(tile); break;
            case TileType.Timmy: await ActivateTimmy(tile);  break;
        }
    }

    private async Task ActivateTier2(Tile tile)
    {
        TimerManager.Instance.AddTime(tier2BonusTime);
        var area = SpecialAreas.Line(Tiles, tile, Random.value > 0.5f);
        await ExplodeTiles(area, tile, fx: tier2Fx);
    }

    private async Task ActivateTier3(Tile tile)
    {
        TimerManager.Instance.AddTime(tier3BonusTime);
        var target = SpecialAreas.RandomTarget(Tiles, tile);
        await FlyTo(tile, target);

        var area = SpecialAreas.Square(Tiles, target);
        await ExplodeTiles(new HashSet<Tile>(area) { tile }, tile, fx: tier3Fx, blastArea: area);
    }

    private async Task ActivateTimmy(Tile tile)
    {
        TimerManager.Instance.AddTime(timmyBonusTime);
        await ExplodeTiles(SpecialAreas.All(Tiles), tile, chainActivateSpecials: false, fx: timmyFx);
    }

    private async Task FlyTo(Tile tile, Tile target)
    {
        var icon = tile.icon.transform;
        await icon.DOMove(target.transform.position, FlyDuration).SetEase(Ease.InCubic).AsyncWaitForCompletion();
        await icon.DOScale(Vector3.zero, TweenDuration * 0.5f).AsyncWaitForCompletion();
        icon.SetParent(tile.transform);
        icon.localPosition = Vector3.zero;
    }

    private async Task ExplodeTiles(HashSet<Tile> tiles, Tile originTile = null,
        bool chainActivateSpecials = true, ExplosionFx fx = null, HashSet<Tile> blastArea = null)
    {
        foreach (var t in tiles)
            _touchedColumnsThisTurn.Add(t.x);

        var otherSpecials = chainActivateSpecials
            ? tiles.Where(t => t != originTile && t.Item != null && t.Item.tileType != TileType.Normal).ToHashSet()
            : new HashSet<Tile>();

        var toClear = new HashSet<Tile>(tiles);
        toClear.ExceptWith(otherSpecials);

        foreach (var t in toClear)
            ScoreManager.Instance.AddTileScore(t.Item.value);

        var tint = originTile != null && originTile.Item != null ? originTile.Item.fxMaterial : null;

        Dictionary<Tile, float> delays = null;
        if (fx != null && ExplosionPlayer.Instance != null)
        {
            if (fx.mode == ExplosionMode.Wave) delays = ExplosionPlayer.Instance.PlayWave(fx, originTile, toClear, tint);
            else ExplosionPlayer.Instance.PlayBlast(fx, blastArea ?? toClear, tint);
        }

        var deflate = DOTween.Sequence();
        foreach (var t in toClear)
            deflate.Insert(delays != null ? delays[t] : 0f, t.icon.transform.DOScale(Vector3.zero, TweenDuration));

        await deflate.Play().AsyncWaitForCompletion();

        await FillBoard(toClear);
    }

    private (Dictionary<Tile, Tile> remap, Sequence sequence) PrepareFillBoard(
    HashSet<Tile> clearedTiles, HashSet<Tile> hidden = null)
    {
        var result = BoardFiller.Prepare(Tiles, clearedTiles, TweenDuration, hidden);
        var moved  = _protectedThisTurn.Where(result.remap.ContainsKey).Select(p => result.remap[p]).ToList();
        _protectedThisTurn.Clear();
        _protectedThisTurn.UnionWith(moved);
        return result;
    }

    private async Task FillBoard(HashSet<Tile> clearedTiles)
    {
        var (_, sequence) = PrepareFillBoard(clearedTiles);
        await sequence.Play().AsyncWaitForCompletion();
    }

    private void PlayMatchSounds(HashSet<Tile> tiles)
    {
        var played = new HashSet<Item>();
        foreach (var tile in tiles)
        {
            if (!played.Add(tile.Item)) continue;
            PlaySound(tile.Item);
        }
    }

    private void PlaySound(Item item) => PlayRandomClip(item.matchSounds);

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips is { Length: > 0 })
            _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    public bool TryBomb(Tile center)
    {
        if (center == null || _isProcessing || Time.timeScale == 0f) return false;
        _ = RunTurn(() => BombRoutine(center));
        return true;
    }

    private async Task BombRoutine(Tile center)
    {
        var area = SpecialAreas.Square(Tiles, center);

        PlayRandomClip(bombSounds);
        await ExplodeTiles(area, fx: bombFx, blastArea: area);
        await ResolveBoard();
    }
}
