using UnityEngine;
using UnityEngine.UI;

public sealed class StoreItemPopup : MonoBehaviour
{
    [SerializeField] private Image     icon;
    [SerializeField] private Transform nameRoot;
    [SerializeField] private Transform descriptionRoot;
    [SerializeField] private Transform costRoot;
    [SerializeField] private Button    actionButton;
    [SerializeField] private Transform actionLabelRoot;
    [SerializeField] private Button    backButton;

    [Header("Выбор слота (бустеры и тайлы)")]
    [SerializeField] private GameObject equipOverlay;
    [SerializeField] private Button[]   slotButtons;
    [SerializeField] private Sprite     emptySlotSprite;

    [Header("Улучшение (только бустеры)")]
    [SerializeField] private GameObject upgradeButtonRoot;
    [SerializeField] private Transform  levelRoot;
    [SerializeField] private Transform  statRoot;
    [SerializeField] private Transform  upgradeValueRoot;
    [SerializeField] private Transform  upgradeCostRoot;

    private Button _upgradeButton;

    private StoreItemData _data;

    private void Awake()
    {
        for (var i = 0; i < slotButtons.Length; i++)
        {
            var slot = i;
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(slot));
        }
        if (equipOverlay != null && equipOverlay.TryGetComponent(out Button cancel))
            cancel.onClick.AddListener(() => equipOverlay.SetActive(false));
        
        if (upgradeButtonRoot != null) _upgradeButton = upgradeButtonRoot.GetComponentInChildren<Button>(true);
    }

    public void Show(StoreItemData data)
    {
        _data = data;
        gameObject.SetActive(true);
        if (equipOverlay != null) equipOverlay.SetActive(false);

        icon.sprite = data.icon;
        UiText.Set(nameRoot, data.displayName);
        UiText.Set(descriptionRoot, data.description);
        UiText.Set(costRoot, data.cost.ToString());
        Refresh();
    }

    private void OnEnable()
    {
        if (_upgradeButton != null) _upgradeButton.onClick.AddListener(OnUpgrade);
        StoreInventory.OnChanged += Refresh;
        actionButton.onClick.AddListener(OnAction);
        backButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (_upgradeButton != null) _upgradeButton.onClick.RemoveListener(OnUpgrade);
        StoreInventory.OnChanged -= Refresh;
        actionButton.onClick.RemoveListener(OnAction);
        backButton.onClick.RemoveListener(Close);
    }

    private void Refresh()
    {
        if (_data == null) return;
        var owned      = StoreInventory.IsOwned(_data);
        var equipped   = StoreInventory.IsEquipped(_data);
        var canUnequip = StoreInventory.CanUnequip(_data);

        costRoot.gameObject.SetActive(!owned);
        actionButton.interactable = !equipped || canUnequip;
        UiText.Set(actionLabelRoot, !owned ? "Купить"
                                  : !equipped ? "Экипировать"
                                  : canUnequip ? "Снять" : "Экипировано");
        
        RefreshUpgrade(owned);
    }

    private void RefreshUpgrade(bool owned)
    {
        if (upgradeButtonRoot == null) return;

        var show = owned && _data.Upgradable;
        var lvl  = show ? StoreInventory.GetLevel(_data) : 1;
        var max  = lvl >= _data.MaxLevel;

        levelRoot.gameObject.SetActive(show);
        statRoot.gameObject.SetActive(show);
        upgradeValueRoot.gameObject.SetActive(show);
        upgradeCostRoot.gameObject.SetActive(show && !max);
        upgradeButtonRoot.SetActive(show && !max);
        if (!show) return;

        var cur = Fmt(_data.statValues[lvl - 1]);
        UiText.Set(statRoot, _data.statName);
        UiText.Set(levelRoot, max ? $"LVL: {lvl} (MAX)" : $"LVL: {lvl} --> {lvl + 1}");
        UiText.Set(upgradeValueRoot, max ? cur : $"{cur} --> {Fmt(_data.statValues[lvl])}");
        if (!max) UiText.Set(upgradeCostRoot, StoreInventory.UpgradeCost(_data).ToString());
    }

    private string Fmt(float v) => v.ToString("0.##") + _data.statSuffix;

    private void OnUpgrade() => Feedback(StoreInventory.TryUpgrade(_data));

    private void OnAction()
    {
        if (!StoreInventory.IsOwned(_data))   { Feedback(StoreInventory.TryBuy(_data)); return; }
        if (StoreInventory.IsEquipped(_data)) { StoreInventory.Unequip(_data); return; }

        if (equipOverlay == null) StoreInventory.EquipToSlot(_data, 0);
        else OpenSlotPicker();
    }

    private static void Feedback(bool success)
    {
        if (!success && UIClickSoundPlayer.Instance != null)
            UIClickSoundPlayer.Instance.PlayError();
    }

    private void OpenSlotPicker()
    {
        var baseItems = _data.category == StoreCategory.Tile ? ItemDatabase.GetTier(_data.tileTier) : null;
        var count = StoreInventory.SlotCount(_data);

        for (var i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].gameObject.SetActive(i < count);
            if (i >= count) continue;

            var current = StoreInventory.GetSlotItem(_data, i);
            slotButtons[i].image.sprite = current != null ? current.icon
                                        : baseItems != null ? baseItems[i].sprite
                                        : emptySlotSprite;
        }
        equipOverlay.SetActive(true);
    }

    private void OnSlotClicked(int slot)
    {
        StoreInventory.EquipToSlot(_data, slot);
        equipOverlay.SetActive(false);
    }

    private void Close() => gameObject.SetActive(false);
}
