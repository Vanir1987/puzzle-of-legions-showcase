using UnityEngine;

public sealed class StoreCategoryFilter : MonoBehaviour
{
    [Tooltip("Объект 'items' — карточки ищутся во всех вложенных")]
    [SerializeField] private Transform itemsRoot;

    private StoreItemView[] _views;

    private Vector3[] _slots;

    private void Awake()
    {
        _views = itemsRoot.GetComponentsInChildren<StoreItemView>(true);
        _slots = System.Array.ConvertAll(_views, v => v.transform.position);
    }

    public void OnTabClicked(int index)
    {
        var slot = 0;
        foreach (var v in _views)
        {
            var visible = index == 0 || (int)v.Data.category == index - 1;
            v.gameObject.SetActive(visible);
            if (visible) v.transform.position = _slots[slot++];
        }
    }
}
