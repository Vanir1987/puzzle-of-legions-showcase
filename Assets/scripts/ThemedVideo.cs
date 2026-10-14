using UnityEngine;
using UnityEngine.Video;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(VideoPlayer))]
public sealed class ThemedVideo : MonoBehaviour
{
    private void Awake()
    {
        var t = ThemeManager.Current;
        if (t == null || t.gameVideo == null) return;

        var vp = GetComponent<VideoPlayer>();

        if (vp.source == VideoSource.VideoClip && vp.clip == t.gameVideo) return;

        vp.source = VideoSource.VideoClip;
        vp.clip   = t.gameVideo;
    }
}
