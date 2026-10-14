using UnityEngine;
using UnityEngine.UI;

public sealed class StoreTabs : MonoBehaviour
{
    [SerializeField] private Button[]     buttons;
    [SerializeField] private GameObject[] tabBackgrounds;
    [SerializeField] private GameObject[] tabTexts;
    [SerializeField] private StoreCategoryFilter filter;

    private void Awake()
    {
        for (var i = 0; i < buttons.Length; i++)
        {
            var index = i;
            buttons[i].onClick.AddListener(() => Select(index));
        }
    }

    private void Start() => Select(0);

    public void Select(int index)
    {
        for (var i = 0; i < buttons.Length; i++)
        {
            tabBackgrounds[i].SetActive(i == index);
            tabTexts[i].SetActive(i == index);
        }
        filter.OnTabClicked(index);
    }
}
