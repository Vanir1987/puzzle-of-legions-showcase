using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [Header("Сцены")]
    [Tooltip("Имя игровой сцены")]
    [SerializeField] private string gameSceneName = "Game";

    [Tooltip("Имя сцены магазина")]
    [SerializeField] private string storeSceneName = "Store";

    [Header("Панели")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;

    private void Start() => ShowMainPanel();

    public void OnPlayClicked() => SceneManager.LoadScene(gameSceneName);
    public void OnStoreClicked() => SceneManager.LoadScene(storeSceneName);

    public void OnOptionsClicked()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnBackClicked() => ShowMainPanel();

    private void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }
}
