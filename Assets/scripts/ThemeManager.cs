using UnityEngine;

public static class ThemeManager
{
    public static BoardTheme Current
    {
        get
        {
            var board = StoreInventory.GetEquippedBoard();
            return board != null ? board.theme : null;
        }
    }

    public static Sprite Sprite(ThemeSlot slot)
    {
        var t = Current;
        return t != null ? t.Get(slot) : null;
    }
}
