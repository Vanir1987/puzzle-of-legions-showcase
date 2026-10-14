using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public enum ThemeSlot
{
    BoardBorder1 = 0, BoardBorder2 = 1, BoardBorder3 = 2,
    Utile1 = 3, Utile2 = 4, Utile3 = 5,
    BoosterBg1 = 6, BoosterBg2 = 7, BoosterBg3 = 8,
    Menu1 = 9, Menu2 = 10, Menu3 = 11, Menu4 = 12,
    Menu5 = 13, Menu6 = 14, Menu7 = 15, Menu8 = 16,
    MenuBackground = 30,
    ButtonBackground = 31
}

[CreateAssetMenu(menuName = "Match-3/Board Theme")]
public sealed class BoardTheme : ScriptableObject
{
    [Serializable] public class Entry { public ThemeSlot slot; public Sprite sprite; }

    public List<Entry> sprites = new();

    [Header("Аудио/видео матча")]
    public AudioClip gameMusic;
    public AudioClip endGameMusic;
    public VideoClip gameVideo;

    [Header("Кнопки")]
    [Tooltip("Материал для button-highlight")]
    public Material buttonHighlightMaterial;

    public Sprite Get(ThemeSlot slot) => sprites.Find(e => e.slot == slot)?.sprite;

    private void OnValidate()
    {
        sprites.RemoveAll(e => !Enum.IsDefined(typeof(ThemeSlot), e.slot));

        foreach (ThemeSlot s in Enum.GetValues(typeof(ThemeSlot)))
            if (!sprites.Exists(e => e.slot == s)) sprites.Add(new Entry { slot = s });
    }

    [ContextMenu("Очистить лишние слоты")]
    private void Cleanup()
    {
        OnValidate();
    #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
    #endif
    }
}
