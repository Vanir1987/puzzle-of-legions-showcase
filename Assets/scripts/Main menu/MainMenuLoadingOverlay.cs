using System.Collections;
using UnityEngine;

public sealed class MainMenuLoadingOverlay : MonoBehaviour
{
    [SerializeField] private GameObject overlayPanel;
    [SerializeField] private BackgroundVideoPlayer backgroundVideo;
    [SerializeField] private BackgroundMusicPlayer backgroundMusic;
    [SerializeField] private float minDisplayTime = 1f;

    private bool _videoReady;
    private bool _musicReady;

    private void Awake()
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);

        if (backgroundVideo != null) backgroundVideo.Prepared += OnVideoReady;
        else _videoReady = true;

        if (backgroundMusic != null) backgroundMusic.Ready += OnMusicReady;
        else _musicReady = true;

        StartCoroutine(WaitAndReveal());
    }

    private void OnDestroy()
    {
        if (backgroundVideo != null) backgroundVideo.Prepared -= OnVideoReady;
        if (backgroundMusic != null) backgroundMusic.Ready -= OnMusicReady;
    }

    private void OnVideoReady() => _videoReady = true;
    private void OnMusicReady() => _musicReady = true;

    private IEnumerator WaitAndReveal()
    {
        var startTime = Time.time;

        while (!_videoReady || !_musicReady || Time.time - startTime < minDisplayTime)
            yield return null;

        if (backgroundVideo != null) backgroundVideo.StartPlayback();
        if (backgroundMusic != null) backgroundMusic.StartPlayback();

        if (overlayPanel != null) overlayPanel.SetActive(false);
    }
}
