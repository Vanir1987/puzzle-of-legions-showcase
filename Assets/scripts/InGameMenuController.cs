using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class InGameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;

    [Tooltip("Имя сцены главного меню")]
    [SerializeField] private string mainMenuSceneName = "Main menu";

    [Header("Фон и музыка")]
    [SerializeField] private BackgroundVideoPlayer backgroundVideo;
    [SerializeField] private BackgroundMusicPlayer backgroundMusic;

    private void Awake()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && Time.timeScale != 0f) OnMenuButtonClicked();
    }

    public void OnMenuButtonClicked()
    {
        Time.timeScale = 0f;
        if (menuPanel != null) menuPanel.SetActive(true);

        if (backgroundVideo != null) backgroundVideo.PauseVideo();
        if (backgroundMusic != null) backgroundMusic.PauseMusic();

        if (Board.Instance != null) Board.Instance.PauseSfx();
        if (UIClickSoundPlayer.Instance != null) UIClickSoundPlayer.Instance.StopClick();
    }

    public void OnResumeClicked()
    {
        Time.timeScale = 1f;
        if (menuPanel != null) menuPanel.SetActive(false);

        if (backgroundVideo != null) backgroundVideo.ResumeVideo();
        if (backgroundMusic != null) backgroundMusic.ResumeMusic();

        if (Board.Instance != null) Board.Instance.ResumeSfx();
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
