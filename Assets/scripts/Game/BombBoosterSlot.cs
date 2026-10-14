using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class BombBoosterSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static readonly List<RaycastResult> Hits = new();

    private Image _icon, _overlay;
    private float _cooldown, _remaining;
    private RectTransform _ghost, _canvasRect;
    private bool _dragging;

    public void Init(Image icon, Image overlay, float cooldown, bool clockwise)
    {
        _icon = icon; _overlay = overlay; _cooldown = cooldown;
        if (_overlay == null) return;

        if (_overlay.sprite == null) _overlay.sprite = icon.sprite;
        _overlay.type          = Image.Type.Filled;
        _overlay.fillMethod    = Image.FillMethod.Radial360;
        _overlay.fillOrigin    = (int)Image.Origin360.Top;
        _overlay.fillClockwise = clockwise;
        _overlay.raycastTarget = false;
        _overlay.fillAmount    = 0f;
    }

    private void Update()
    {
        if (_remaining <= 0f) return;

        _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
        if (_overlay != null) _overlay.fillAmount = _remaining / _cooldown;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (_remaining > 0f || Time.timeScale == 0f) return;

        _dragging   = true;
        _canvasRect = (RectTransform)_icon.canvas.rootCanvas.transform;

        var go = new GameObject("goblin-ghost", typeof(RectTransform), typeof(Image));
        _ghost = (RectTransform)go.transform;
        _ghost.SetParent(_canvasRect, false);
        var img = go.GetComponent<Image>();
        img.sprite = _icon.sprite;
        img.preserveAspect = true;
        img.raycastTarget  = false;

        var cell = (RectTransform)Board.Instance.Tiles[0, 0].transform;
        _ghost.sizeDelta  = cell.rect.size;
        _ghost.localScale = Vector3.one * (cell.lossyScale.x / _canvasRect.lossyScale.x);
        MoveGhost(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (_dragging) MoveGhost(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!_dragging) return;
        _dragging = false;
        Destroy(_ghost.gameObject);

        Tile target = null;
        Hits.Clear();
        EventSystem.current.RaycastAll(e, Hits);
        foreach (var h in Hits)
            if ((target = h.gameObject.GetComponentInParent<Tile>()) != null) break;

        if (target == null || !Board.Instance.TryBomb(target)) return;

        _remaining = _cooldown;
        if (_overlay != null) _overlay.fillAmount = 1f;
    }

    private void MoveGhost(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, e.position, e.pressEventCamera, out var world);
        _ghost.position = world;
    }

    private void OnDisable()
    {
        _dragging = false;
        if (_ghost != null) Destroy(_ghost.gameObject);
    }
}
