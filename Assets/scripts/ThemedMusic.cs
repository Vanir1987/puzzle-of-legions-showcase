using UnityEngine;

public enum ThemeTrack { GameMusic, EndGameMusic }
[RequireComponent(typeof(AudioSource))]
[DefaultExecutionOrder(-100)]
public sealed class ThemedMusic : MonoBehaviour
{
    [SerializeField] private ThemeTrack track;

    private void Awake()
    {
        var t = ThemeManager.Current;
        if (t == null) return;

        var clip = track == ThemeTrack.GameMusic ? t.gameMusic : t.endGameMusic;
        if (clip != null) GetComponent<AudioSource>().clip = clip;
    }
}
