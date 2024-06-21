using UnityEngine;
using UnityEngine.UI;

public class OptionsPresentor : MonoBehaviour
{
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxVolume;
    [Space(2)]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;
    [Space(2)]
    [SerializeField] private Slider renderScaleSlider;
    [Space(2)]
    [SerializeField] private Button LowQuality;
    [SerializeField] private Button MedQuality;
    [SerializeField] private Button HighQuality;
    [SerializeField] private Sprite QualitySelected;
    [SerializeField] private Sprite QualityNotSelected;

    private Options _options;

    private void Start()
    {
        _options = FindObjectOfType<Options>();

        renderScaleSlider.minValue = Options.MIN_RENDER_SCALE;
        renderScaleSlider.maxValue = Options.MAX_RENDER_SCALE;

        sfxSlider.onValueChanged.AddListener(_options.SetSFXVolume);
        sfxVolume.onValueChanged.AddListener(_options.SetSFXMute);
        sfxVolume.onValueChanged.AddListener((value) => AudioManager.Instance.ClickButtonSFX());
        musicSlider.onValueChanged.AddListener(_options.SetMusicVolume);
        musicToggle.onValueChanged.AddListener(_options.SetMusicMute);
        musicToggle.onValueChanged.AddListener((value) => AudioManager.Instance.ClickButtonSFX());
        renderScaleSlider.onValueChanged.AddListener(_options.SetRenderScale);

        LowQuality.onClick.AddListener(() => _options.SetQualityLevel(0));
        LowQuality.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        MedQuality.onClick.AddListener(() => _options.SetQualityLevel(1));
        MedQuality.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        HighQuality.onClick.AddListener(() => _options.SetQualityLevel(2));
        HighQuality.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    public void SetSFXVolumeSlider(float value) => sfxSlider.value = value;

    public void SetSFXMuteToggle(bool toggle) => sfxVolume.isOn = !toggle;

    public void SetMusicVolumeSlider(float value) => musicSlider.value = value;

    public void SetMusicMuteToggle(bool toggle) => musicToggle.isOn = !toggle;

    public void SetRenderScaleSlider(float value) => renderScaleSlider.value = value;

    public void SetQualityLevel(int value)
    {
        LowQuality.image.sprite = QualityNotSelected;
        MedQuality.image.sprite = QualityNotSelected;
        HighQuality.image.sprite = QualityNotSelected;

        switch (value)
        {
            case 0:
                LowQuality.image.sprite = QualitySelected;
                break;
            case 1:
                MedQuality.image.sprite = QualitySelected;
                break;
            case 2:
                HighQuality.image.sprite = QualitySelected;
                break;
            default:
                break;
        }
    }
}