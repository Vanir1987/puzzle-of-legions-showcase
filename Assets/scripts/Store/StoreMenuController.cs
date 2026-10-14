using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class StoreMenuController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Main menu";
    [SerializeField] private string gameSceneName     = "Game";

    public void OnBackClicked()     => SceneManager.LoadScene(mainMenuSceneName);
    public void OnNewGameClicked()  => SceneManager.LoadScene(gameSceneName);
}
