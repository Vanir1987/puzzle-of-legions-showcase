using UnityEngine;
using UnityEngine.UI;

public sealed class StoreItemView : MonoBehaviour
{
    [SerializeField] private StoreItemData data;
    [SerializeField] private Image icon;
    [SerializeField] private Transform nameRoot;
    [SerializeField] private Transform costRoot;
    [SerializeField] private GameObject activeFrame;

    public StoreItemData Data => data;

    private void Awake()
    {
        if (icon != null) icon.sprite = data.icon;
        UiText.Set(nameRoot, data.displayName);
        UiText.Set(costRoot, data.cost.ToString());
    }

    private void OnEnable()
    {
        StoreInventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => StoreInventory.OnChanged -= Refresh;

    private void Refresh()
    {
        activeFrame.SetActive(StoreInventory.IsEquipped(data));
        costRoot.gameObject.SetActive(!StoreInventory.IsOwned(data));
    }

    public void OnClicked()
    {
        if (UIClickSoundPlayer.Instance != null) UIClickSoundPlayer.Instance.PlayItemClick();
        StoreEquipMenu.Instance.Show(data);
    }
}
