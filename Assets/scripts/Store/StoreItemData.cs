using UnityEngine;

public enum StoreCategory { Board, Booster, Tile }

[CreateAssetMenu(menuName = "Match-3/Store Item")]
public sealed class StoreItemData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;
    public int cost;
    public StoreCategory category;
    [Tooltip("Экипирован по умолчанию, пока игрок не выбрал другой предмет этой категории")]
    public bool isDefault;
    [Header("Слоты (бустеры и тайлы)")]
    [Tooltip("Слот по умолчанию, если включён Is Default")]
    public int defaultSlot;
    [Tooltip("Только тайлы: тир, чьи 6 слотов заменяет этот тайл")]
    public TileType tileTier;
    [Tooltip("Только тайлы: игровой предмет для доски")]
    public Item gameItem;
    [Header("Улучшения (только бустеры)")]
    public string statName;
    public string statSuffix;
    [Tooltip("Значение параметра по уровням: элемент 0 = уровень 1 (сразу после покупки)")]
    public float[] statValues;
    [Tooltip("Цена перехода на след. уровень: элемент 0 = 1→2. Размер подгоняется сам")]
    public int[] upgradeCosts;
    [Header("Бустеры")]
    public BoosterEffect effect;
    [Header("Поля")]
    public BoardTheme theme;

    public bool Upgradable => statValues != null && statValues.Length > 1;
    public int  MaxLevel   => Mathf.Max(1, statValues.Length);

    public int DefaultSlot => isDefault ? defaultSlot : -1;

    public string Id => name;

    private void OnValidate()
    {
        var need = Mathf.Max(0, statValues.Length - 1);
        if (upgradeCosts.Length != need) System.Array.Resize(ref upgradeCosts, need);

        if (category == StoreCategory.Tile && gameItem != null && gameItem.tileType != tileTier)
            Debug.LogWarning($"{name}: у Game Item тип {gameItem.tileType}, а Tile Tier = {tileTier}", this);
    }

}

public enum BoosterEffect { None, ScoreBonus, GoldBonus, ExtraTime, Bomb }
