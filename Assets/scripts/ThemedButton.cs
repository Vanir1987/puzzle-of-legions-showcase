using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MultiSpriteButton))]
public sealed class ThemedButton : MonoBehaviour
{
    [SerializeField] private Image   highlight;
    [SerializeField] private Image background;

    private MultiSpriteButton _button;

    private void Reset()
    {
        var root   = transform.parent != null ? transform.parent : transform;
        var images = root.GetComponentsInChildren<Image>(true);
        background = images.FirstOrDefault(i => i.name == "button-background-enable");
        highlight  = images.FirstOrDefault(i => i.name == "button-highlight");
    }

    private void Awake() => _button = GetComponent<MultiSpriteButton>();

    private void OnEnable()
    {
        StoreInventory.OnChanged += Apply;
        Apply();
    }

    private void OnDisable() => StoreInventory.OnChanged -= Apply;

    private void Apply()
    {
        var t = ThemeManager.Current;
        if (t == null) return;

        var bg = t.Get(ThemeSlot.ButtonBackground);
        if (background != null && bg != null) background.sprite = bg;

        if (highlight != null && t.buttonHighlightMaterial != null)
            highlight.material = t.buttonHighlightMaterial;
    }
}
