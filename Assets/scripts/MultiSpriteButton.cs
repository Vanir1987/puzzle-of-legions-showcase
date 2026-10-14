using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MultiSpriteButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [System.Serializable]
    private struct SpritePiece
    {
        public Image  image;
        public Sprite normalSprite;
        public Sprite pressedSprite;
    }

    [SerializeField] private SpritePiece[] pieces;

    [Tooltip("Один оверлей подсветки на всю кнопку целиком. " +
             "Изначально выключен в иерархии, включается при наведении/касании.")]
    [SerializeField] private GameObject highlightOverlay;

    [Header("Текст кнопки")]
    [Tooltip("При нажатии становится белым и смещается на pressedLabelOffsetX/Y, при отпускании возвращается")]
    [SerializeField] private Text label;
    [SerializeField] private Color pressedLabelColor = Color.white;
    [SerializeField] private float pressedLabelOffsetX = 0f;
    [SerializeField] private float pressedLabelOffsetY = -2f;

    public int PieceCount => pieces?.Length ?? 0;
    private bool  _isPressed;
    private Color _labelNormalColor;
    private Vector2 _labelNormalPos;

    private void Awake()
    {
        if (label == null) return;
        _labelNormalColor = label.color;
        _labelNormalPos   = label.rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData) => SetPressed(true);

    public void OnPointerUp(PointerEventData eventData) => SetPressed(false);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIClickSoundPlayer.Instance != null)
            UIClickSoundPlayer.Instance.PlayClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHighlighted(true);

        if (eventData.pointerPress == gameObject)
            SetPressed(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlighted(false);
        SetPressed(false);
    }

    private void OnDisable()
    {
        SetHighlighted(false);
        SetPressed(false);
    }

    private void SetPressed(bool pressed)
    {
        if (_isPressed == pressed) return;
        _isPressed = pressed;

        foreach (var piece in pieces)
        {
            if (piece.image == null) continue;
            piece.image.sprite = pressed ? piece.pressedSprite : piece.normalSprite;
        }

        if (label != null)
        {
            label.color = pressed ? pressedLabelColor : _labelNormalColor;
            label.rectTransform.anchoredPosition = pressed
                ? _labelNormalPos + new Vector2(pressedLabelOffsetX, pressedLabelOffsetY)
                : _labelNormalPos;
        }
    }

    private void SetHighlighted(bool highlighted)
    {
        if (highlightOverlay != null) highlightOverlay.SetActive(highlighted);
    }

    public void SetPieceSprites(int i, Sprite normal, Sprite pressed)
    {
        if (pieces == null || i >= pieces.Length) return;
        if (normal  != null) pieces[i].normalSprite  = normal;
        if (pressed != null) pieces[i].pressedSprite = pressed;
        if (pieces[i].image != null)
            pieces[i].image.sprite = _isPressed ? pieces[i].pressedSprite : pieces[i].normalSprite;
    }
}
