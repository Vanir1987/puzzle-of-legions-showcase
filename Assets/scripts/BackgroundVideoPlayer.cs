using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public sealed class BackgroundVideoPlayer : MonoBehaviour
{
    [Tooltip("RawImage, на который выводится видео")]
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private bool autoPlayOnReady = true;

    public event Action Prepared;

    private VideoPlayer _videoPlayer;

    public bool IsPrepared { get; private set; }

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.playOnAwake = false;
        _videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        _videoPlayer.prepareCompleted += OnPrepared;

        if (videoDisplay != null) videoDisplay.enabled = false;

        ApplyAmbientVolume();
        GameAudioSettings.OnChanged += ApplyAmbientVolume;
    }

    private void OnEnable()
    {
        _videoPlayer.Prepare();
    }

    private void OnDisable()
    {
        IsPrepared = false;
        _videoPlayer.Stop();
        if (videoDisplay != null) videoDisplay.enabled = false;
    }

    private void OnDestroy()
    {
        _videoPlayer.prepareCompleted -= OnPrepared;
        GameAudioSettings.OnChanged -= ApplyAmbientVolume;
    }

    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log($"[Video] prepared {_videoPlayer.width}x{_videoPlayer.height}");
        IsPrepared = true;
        Prepared?.Invoke();
        if (autoPlayOnReady) StartPlayback();
    }

    private void ApplyAmbientVolume() =>
        _videoPlayer.SetDirectAudioVolume(0, GameAudioSettings.EffectiveAmbientVolume);

    public void StartPlayback()
    {
        _videoPlayer.Play();
        if (videoDisplay != null) videoDisplay.enabled = true;
    }

    public void PauseVideo() => _videoPlayer.Pause();

    public void ResumeVideo() => _videoPlayer.Play();
}
