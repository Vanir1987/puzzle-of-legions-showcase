using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIClickSoundOnClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIClickSoundPlayer.Instance != null)
            UIClickSoundPlayer.Instance.PlayClick();
    }
}
