using System;
using UnityEngine;

public static class GameAudioSettings
{
    private const string SfxEnabledKey     = "SfxEnabled";
    private const string SfxVolumeKey      = "SfxVolume";
    private const string MusicEnabledKey   = "MusicEnabled";
    private const string MusicVolumeKey    = "MusicVolume";
    private const string AmbientEnabledKey = "AmbientEnabled";
    private const string AmbientVolumeKey  = "AmbientVolume";

    public static bool  SfxEnabled     { get; private set; }
    public static float SfxVolume      { get; private set; }
    public static bool  MusicEnabled   { get; private set; }
    public static float MusicVolume    { get; private set; }
    public static bool  AmbientEnabled { get; private set; }
    public static float AmbientVolume  { get; private set; }

    public static float EffectiveSfxVolume     => SfxEnabled     ? SfxVolume     : 0f;
    public static float EffectiveMusicVolume   => MusicEnabled   ? MusicVolume   : 0f;
    public static float EffectiveAmbientVolume => AmbientEnabled ? AmbientVolume : 0f;

    public static event Action OnChanged;

    static GameAudioSettings() => Load();

    public static void Load()
    {
        SfxEnabled     = PlayerPrefs.GetInt(SfxEnabledKey, 1) == 1;
        SfxVolume      = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        MusicEnabled   = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        MusicVolume    = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        AmbientEnabled = PlayerPrefs.GetInt(AmbientEnabledKey, 1) == 1;
        AmbientVolume  = PlayerPrefs.GetFloat(AmbientVolumeKey, 1f);
    }

    public static void Set(bool sfxEnabled, float sfxVolume, bool musicEnabled, float musicVolume)
    {
        SfxEnabled   = sfxEnabled;
        SfxVolume    = sfxVolume;
        MusicEnabled = musicEnabled;
        MusicVolume  = musicVolume;
        OnChanged?.Invoke();
    }

    public static void SetAmbient(bool enabled, float volume)
    {
        AmbientEnabled = enabled;
        AmbientVolume  = volume;
        OnChanged?.Invoke();
    }

    public static void Save()
    {
        PlayerPrefs.SetInt(SfxEnabledKey, SfxEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.SetInt(MusicEnabledKey, MusicEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetInt(AmbientEnabledKey, AmbientEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(AmbientVolumeKey, AmbientVolume);
        PlayerPrefs.Save();
    }
}
