using UnityEngine;
using System;

public static class CurrencyManager
{
    private const string GoldKey = "Gold";

    public static int Gold { get; private set; }

    public static event Action OnGoldChanged;

    static CurrencyManager() => Load();

    public static void Load() => Gold = PlayerPrefs.GetInt(GoldKey, 0);

    public static void AddGold(int amount)
    {
        Gold += amount;
        PlayerPrefs.SetInt(GoldKey, Gold);
        PlayerPrefs.Save();
        OnGoldChanged?.Invoke();
    }

    public static bool TrySpend(int amount)
    {
        if (amount > Gold) return false;
        AddGold(-amount);
        return true;
    }
}
