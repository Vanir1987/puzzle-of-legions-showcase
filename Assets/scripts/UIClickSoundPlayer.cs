using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class UIClickSoundPlayer : MonoBehaviour
{
    public static UIClickSoundPlayer Instance { get; private set; }

    [SerializeField] private AudioClip[] clickSounds;

    private AudioSource _audioSource;

    [Tooltip("Звук нажатия на карточку товара")]
    [SerializeField] private AudioClip[] itemClickSounds;

    private void Awake()
    {
        Instance     = this;
        _audioSource = GetComponent<AudioSource>();

        ApplyVolume();
        GameAudioSettings.OnChanged += ApplyVolume;
    }

    private void OnDestroy() => GameAudioSettings.OnChanged -= ApplyVolume;

    private void ApplyVolume() => _audioSource.volume = GameAudioSettings.EffectiveSfxVolume;

    public void StopClick() => _audioSource.Stop();

    public void PlayClick()     => PlayRandom(clickSounds);
    public void PlayItemClick() => PlayRandom(itemClickSounds);

    [Tooltip("Звук отказа (например, при нехватке золота)")]
    [SerializeField] private AudioClip[] errorSounds;

    public void PlayError() => PlayRandom(errorSounds);

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips is { Length: > 0 })
            _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
