using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    [SerializeField] private bool autoPlayOnReady = true;

    public event Action Ready;

    private AudioSource _audioSource;

    public event Action<double> Started;
    public double StartDspTime { get; private set; }
    public bool HasStarted { get; private set; }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        ApplyVolume();
        GameAudioSettings.OnChanged += ApplyVolume;
        StartCoroutine(LoadClip());
    }

    private void OnDisable()
    {
        GameAudioSettings.OnChanged -= ApplyVolume;
        _audioSource.Stop();
        HasStarted = false;
    }

    private void ApplyVolume() => _audioSource.volume = GameAudioSettings.EffectiveMusicVolume;

    private IEnumerator LoadClip()
    {
        var clip = _audioSource.clip;
        if (clip == null)
        {
            Ready?.Invoke();
            yield break;
        }

        if (clip.loadState == AudioDataLoadState.Unloaded)
            clip.LoadAudioData();

        while (clip.loadState == AudioDataLoadState.Loading)
            yield return null;

        Ready?.Invoke();
        if (autoPlayOnReady) StartPlayback();
    }

    public void StartPlayback()
    {
        StartDspTime = UnityEngine.AudioSettings.dspTime + 0.1;
        _audioSource.PlayScheduled(StartDspTime);
        HasStarted = true;
        Started?.Invoke(StartDspTime);
    }

    public void PauseMusic() => _audioSource.Pause();

    public void ResumeMusic() => _audioSource.UnPause();
}
