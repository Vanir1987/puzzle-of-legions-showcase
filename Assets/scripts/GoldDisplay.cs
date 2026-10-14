using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public sealed class GoldDisplay : MonoBehaviour
{
    private Text _text;

    private void Awake() => _text = GetComponent<Text>();

    private void OnEnable()
    {
        CurrencyManager.OnGoldChanged += Refresh;
        Refresh();
    }

    private void OnDisable() => CurrencyManager.OnGoldChanged -= Refresh;

    private void Refresh() => _text.text = CurrencyManager.Gold.ToString();
}
