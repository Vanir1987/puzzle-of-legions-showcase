using UnityEngine;

public enum TileType { Normal, Tier2, Tier3, Timmy }

[CreateAssetMenu(menuName = "Match-3/Item")]
public sealed class Item : ScriptableObject
{
    public int value;
    public Sprite sprite;

    [Header("Тип тайла")]
    public TileType tileType = TileType.Normal;

    [Header("Апгрейды (заполнять только у Tier1)")]
    [Tooltip("Какой Tier2-тайл появится при сборе 4 этого тайла в ряд")]
    public Item tier2Upgrade;

    [Tooltip("Какой Tier3-тайл появится при сборе 5 этого тайла")]
    public Item tier3Upgrade;

    [Header("Звуки (совпадение для Tier1 / активация для Tier2+)")]
    public AudioClip[] matchSounds;

    [Header("Эффекты")]
    [Tooltip("Материал свечения (Для Tier2+)")]
    public Material fxMaterial;
}
