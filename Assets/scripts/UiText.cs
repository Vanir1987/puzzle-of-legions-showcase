using UnityEngine;
using UnityEngine.UI;

public static class UiText
{
    public static void Set(Transform root, string value)
    {
        foreach (var t in root.GetComponentsInChildren<Text>(true)) t.text = value;
    }
}
