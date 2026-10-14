using UnityEngine;

public sealed class StoreEquipMenu : MonoBehaviour
{
    public static StoreEquipMenu Instance { get; private set; }

    [Tooltip("Порядок = enum StoreCategory: Board, Booster, Tile")]
    [SerializeField] private StoreItemPopup[] popups;

    private void Awake()
    {
        Instance = this;
        foreach (var p in popups) p.gameObject.SetActive(false);
    }

    public void Show(StoreItemData data) => popups[(int)data.category].Show(data);
}
