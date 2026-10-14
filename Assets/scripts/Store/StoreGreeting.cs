using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class StoreGreeting : MonoBehaviour
{
    [SerializeField] private BackgroundMusicPlayer music;
    [SerializeField] private AudioClip[] lines;

    private static int _last = -1;
    private AudioSource _source;
    private bool _played;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        foreach (var l in lines) if (l != null) l.LoadAudioData();
    }

    private void Start()
    {
        if (music.HasStarted) Play(music.StartDspTime);
        else music.Started += Play;
    }

    private void OnDestroy()
    {
        if (music != null) music.Started -= Play;
    }

    private void Play(double dspTime)
    {
        if (_played || lines is not { Length: > 0 }) return;
        _played = true;

        int i;
        do i = Random.Range(0, lines.Length); while (i == _last && lines.Length > 1);
        _last = i;

        _source.clip   = lines[i];
        _source.volume = GameAudioSettings.EffectiveSfxVolume;
        _source.PlayScheduled(dspTime);
    }
}
