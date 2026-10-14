using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class EndGameMenuController : MonoBehaviour
{
    [Header("Сцены")]
    [SerializeField] private string storeSceneName = "Store";

    [Header("Тексты счёта")]
    [SerializeField] private Text lumberText;
    [SerializeField] private Text goldText;

    [Header("Аудио при окончании игры")]
    [SerializeField] private BackgroundMusicPlayer regularMusic;
    [Tooltip("Трек окончания игры")]
    [SerializeField] private BackgroundMusicPlayer endGameMusic;

    [Header("Конвертация")]
    [Tooltip("Сколько очков счёта даёт 1 золото")]
    [SerializeField] private int scorePerGold = 10;
    [Tooltip("Длительность анимации конвертации, сек")]
    [SerializeField] private float conversionDuration = 1.5f;

    private void OnEnable()
    {
        if (regularMusic    != null) regularMusic.PauseMusic();
        if (Board.Instance  != null) Board.Instance.PauseSfx();
        if (endGameMusic    != null) endGameMusic.StartPlayback();

        var finalScore = ScoreManager.Instance.Score;
        var goldBonus  = StoreInventory.GetBoosterStat(BoosterEffect.GoldBonus);
        var earnedGold = Mathf.RoundToInt((float)finalScore / scorePerGold * (1f + goldBonus / 100f));

        CurrencyManager.AddGold(earnedGold);

        StartCoroutine(AnimateConversion(finalScore, earnedGold));
    }

    public void OnStoreClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(storeSceneName);
    }

    private IEnumerator AnimateConversion(int finalScore, int earnedGold)
    {
        var elapsed = 0f;

        while (elapsed < conversionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / conversionDuration);

            lumberText.text = Mathf.RoundToInt(Mathf.Lerp(finalScore, 0, t)).ToString();
            goldText.text   = Mathf.RoundToInt(Mathf.Lerp(0, earnedGold, t)).ToString();

            yield return null;
        }

        lumberText.text = "0";
        goldText.text   = earnedGold.ToString();
    }
}
