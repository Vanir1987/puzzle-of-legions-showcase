using UnityEngine;
using UnityEngine.UI;

public sealed class BoosterPanel : MonoBehaviour
{
    [Tooltip("Объекты icon: slot/icon, slot (1)/icon, slot (2)/icon")]
    [SerializeField] private Button[] iconButtons;
    [Tooltip("Затемнение поверх иконок: по одному на слот, в порядке слотов")]
    [SerializeField] private Image[] cooldownOverlays;
    [Tooltip("Направление, в котором затемнение исчезает")]
    [SerializeField] private bool clockwise = true;

    private void Start()
    {
        for (var i = 0; i < iconButtons.Length; i++)
        {
            var b = StoreInventory.GetBoosterSlot(i);
            iconButtons[i].transform.parent.gameObject.SetActive(b != null);
            if (b == null) continue;

            iconButtons[i].image.sprite = b.icon;
            if (b.effect != BoosterEffect.Bomb) continue;

            var overlay = i < cooldownOverlays.Length ? cooldownOverlays[i] : null;
            iconButtons[i].gameObject.AddComponent<BombBoosterSlot>()
                .Init(iconButtons[i].image, overlay, StoreInventory.GetStat(b), clockwise);
        }
    }
}
