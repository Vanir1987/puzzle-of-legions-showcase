using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class ThemedImage : MonoBehaviour
{
    [SerializeField] private ThemeSlot slot;

    private Image  _image;
    private Sprite _original;

    private void Awake()
    {
        _image    = GetComponent<Image>();
        _original = _image.sprite;
    }

    private void OnEnable()
    {
        StoreInventory.OnChanged += Apply;
        Apply();
    }

    private void OnDisable() => StoreInventory.OnChanged -= Apply;

    private void Apply()
    {
        var s = ThemeManager.Sprite(slot);
        _image.sprite = s != null ? s : _original;
    }
}
