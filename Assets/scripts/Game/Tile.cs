using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public int x;
    public int y;

    private Item _item;

    private void ApplyFx(Item item)
    {
        var mat = item != null ? item.fxMaterial : null;
        var fx  = icon.transform.Find("fx");

        if (mat == null) { if (fx != null) fx.gameObject.SetActive(false); return; }
        if (fx == null)  fx = CreateFx();

        fx.GetComponent<Image>().material = mat;
        fx.gameObject.SetActive(true);
    }

    private Transform CreateFx()
    {
        var go = new GameObject("fx", typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(icon.transform, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = ((RectTransform)transform).rect.size;
        go.GetComponent<Image>().raycastTarget = false;
        return rt;
    }

    public Item Item
    {
        get => _item;
        set
        {
            if (_item == value) return;
            _item = value;
            icon.sprite = _item != null ? _item.sprite : null;
            ApplyFx(_item);
        }
    }

    public Image icon;
    public Button button;

    public Tile[,] Grid { get; set; }

    public Tile Left   => x > 0 ? Grid[x - 1, y] : null;
    public Tile Top    => y > 0 ? Grid[x, y - 1] : null;
    public Tile Right  => x < Grid.GetLength(0) - 1 ? Grid[x + 1, y] : null;
    public Tile Bottom => y < Grid.GetLength(1) - 1 ? Grid[x, y + 1] : null;

    public Tile GetNeighbour(int i) => i switch { 0 => Left, 1 => Top, 2 => Right, _ => Bottom };

    private const float SwipeThreshold = 50f;

    private Vector2 _dragStartScreenPos;
    private bool    _dragHandled;

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragStartScreenPos = eventData.position;
        _dragHandled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragHandled) return;

        var delta = eventData.position - _dragStartScreenPos;
        if (delta.magnitude < SwipeThreshold) return;

        var target = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? (delta.x > 0 ? Right : Left)
            : (delta.y > 0 ? Top : Bottom);

        if (target == null) return;

        _dragHandled = true;
        Board.Instance.TrySwap(this, target);
    }

    public void OnEndDrag(PointerEventData eventData) => _dragHandled = false;

    public void SetItem(Item item)
    {
        _item = item;
        icon.sprite = item != null ? item.sprite : null;
        ApplyFx(item);
    }
}
