using UnityEngine;
using UnityEngine.UI;

public sealed class OptionsPanelController : MonoBehaviour
{
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle ambientToggle;
    [SerializeField] private Slider ambientSlider;

    private bool  _savedSfxEnabled;
    private float _savedSfxVolume;
    private bool  _savedMusicEnabled;
    private float _savedMusicVolume;
    private bool  _savedAmbientEnabled;
    private float _savedAmbientVolume;

    private void OnEnable()
    {
        _savedSfxEnabled   = GameAudioSettings.SfxEnabled;
        _savedSfxVolume    = GameAudioSettings.SfxVolume;
        _savedMusicEnabled = GameAudioSettings.MusicEnabled;
        _savedMusicVolume  = GameAudioSettings.MusicVolume;

        sfxToggle.isOn = _savedSfxEnabled;
        sfxSlider.SetValueWithoutNotify(_savedSfxVolume);
        musicToggle.isOn = _savedMusicEnabled;
        musicSlider.SetValueWithoutNotify(_savedMusicVolume);

        sfxToggle.onValueChanged.AddListener(OnToggleChanged);
        sfxSlider.onValueChanged.AddListener(OnSliderChanged);
        musicToggle.onValueChanged.AddListener(OnToggleChanged);
        musicSlider.onValueChanged.AddListener(OnSliderChanged);

        if (ambientToggle != null && ambientSlider != null)
        {
            _savedAmbientEnabled = GameAudioSettings.AmbientEnabled;
            _savedAmbientVolume  = GameAudioSettings.AmbientVolume;

            ambientToggle.isOn = _savedAmbientEnabled;
            ambientSlider.SetValueWithoutNotify(_savedAmbientVolume);

            ambientToggle.onValueChanged.AddListener(OnAmbientToggleChanged);
            ambientSlider.onValueChanged.AddListener(OnAmbientSliderChanged);
        }
    }

    private void OnDisable()
    {
        sfxToggle.onValueChanged.RemoveListener(OnToggleChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSliderChanged);
        musicToggle.onValueChanged.RemoveListener(OnToggleChanged);
        musicSlider.onValueChanged.RemoveListener(OnSliderChanged);

        if (ambientToggle != null && ambientSlider != null)
        {
            ambientToggle.onValueChanged.RemoveListener(OnAmbientToggleChanged);
            ambientSlider.onValueChanged.RemoveListener(OnAmbientSliderChanged);
        }
    }

    private void OnToggleChanged(bool _) => ApplyLivePreview();

    private void OnSliderChanged(float _) => ApplyLivePreview();

    private void OnAmbientToggleChanged(bool _) => ApplyAmbientLivePreview();

    private void OnAmbientSliderChanged(float _) => ApplyAmbientLivePreview();

    private void ApplyLivePreview()
    {
        GameAudioSettings.Set(sfxToggle.isOn, sfxSlider.value, musicToggle.isOn, musicSlider.value);
    }

    private void ApplyAmbientLivePreview()
    {
        if (ambientToggle == null || ambientSlider == null) return;
        GameAudioSettings.SetAmbient(ambientToggle.isOn, ambientSlider.value);
    }

    public void OnOkClicked() => GameAudioSettings.Save();

    public void OnCancelClicked()
    {
        GameAudioSettings.Set(_savedSfxEnabled, _savedSfxVolume, _savedMusicEnabled, _savedMusicVolume);

        if (ambientToggle != null && ambientSlider != null)
            GameAudioSettings.SetAmbient(_savedAmbientEnabled, _savedAmbientVolume);
    }
}
