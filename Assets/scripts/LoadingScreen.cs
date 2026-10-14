using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private float minDisplayTime = 1f;

    [Header("Фон")]
    [SerializeField] private BackgroundVideoPlayer backgroundVideo;
    [SerializeField] private BackgroundMusicPlayer backgroundMusic;
    [Tooltip("За сколько секунд до скрытия панели запускать видео/музыку")]
    [SerializeField] private float mediaPreRollTime = 0.1f;

    [Tooltip("Сколько ждать готовности видео сверх minDisplayTime, сек (страховка от зависания)")]
    [SerializeField] private float maxExtraWait = 5f;

    private bool MediaReady => backgroundVideo == null || backgroundVideo.IsPrepared;
    private bool BoardReady => Board.Instance == null || Board.Instance.IsReady;
    private float BoardProgress => Board.Instance == null ? 1f : Board.Instance.WarmProgress;

    private void Start() => StartCoroutine(ShowThenHide());

    private IEnumerator ShowThenHide()
    {
        if (progressPanel != null) progressPanel.SetActive(true);

        var startTime      = Time.time;
        var mediaStartTime = Mathf.Max(0f, minDisplayTime - mediaPreRollTime);
        var mediaStarted   = false;
        var shown          = 0f;
        var vel            = 0f;

        do
        {
            var elapsed = Time.time - startTime;

            var timedOut = elapsed >= minDisplayTime + maxExtraWait;
            if (!mediaStarted && elapsed >= mediaStartTime && (MediaReady || timedOut) && BoardReady)
            {
                mediaStarted = true;
                if (backgroundVideo != null) backgroundVideo.StartPlayback();
                if (backgroundMusic != null) backgroundMusic.StartPlayback();
            }

            var target = Mathf.Min(elapsed / minDisplayTime, BoardProgress);
            shown = Mathf.SmoothDamp(shown, target, ref vel, 0.25f);
            if (progressBarImage != null) progressBarImage.fillAmount = shown;
            yield return null;
        }
        while (shown < 0.99f || !mediaStarted || !BoardReady);

        if (progressBarImage != null) progressBarImage.fillAmount = 1f;
        if (progressPanel != null) progressPanel.SetActive(false);

        if (TimerManager.Instance != null) TimerManager.Instance.StartTimer();
    }
}
