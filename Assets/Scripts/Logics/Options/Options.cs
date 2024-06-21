using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DeathMatch;

public class Options : MonoBehaviour
{
    public const int RESOLUTION_HEIGHT = 1280;
    public const float MIN_RENDER_SCALE = 0.5f;
    public const float MAX_RENDER_SCALE = 2f;

    public const string SFX_VOLUME_PREFS = "CONFIG_SFX_VOLUME";
    public const string SFX_MUTE_PREFS = "CONFIG_SFX_MUTE";
    public const string MUSIC_VOLUME_PREFS = "CONFIG_MUSIC_VOLUME";
    public const string MUSIC_MUTE_PREFS = "CONFIG_MUSIC_MUTE";
    public const string RENDER_SCALE_PREFS = "CONFIG_RENDER_SCALE";
    public const string QUALITY_LEVEL_PREFS = "CONFIG_QUALITY_LEVEL";

    public event Action OnMusicChange;
    public event Action OnVideoChange;

    private OptionsPresentor _presentor;
    private UniversalRenderPipelineAsset urpAsset;

    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
    }

    private void OnApplicationQuit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    private void Start()
    {
        _presentor = FindObjectOfType<OptionsPresentor>();
        InitializeOption();
    }

    private void InitializeOption()
    {
        Screen.SetResolution(Convert.ToInt32(RESOLUTION_HEIGHT * ((float)Screen.width / Screen.height)), RESOLUTION_HEIGHT, true);

        if (!SaveManager.HasKey(SFX_VOLUME_PREFS))
            SetSFXVolume(1);
        else
            SetSFXVolume(SaveManager.Get<float>(SFX_VOLUME_PREFS));

        if (!SaveManager.HasKey(SFX_MUTE_PREFS))
            SetSFXMute(true);
        else
            SetSFXMute(SaveManager.Get<int>(SFX_MUTE_PREFS) == 1 ? false : true);

        if (!SaveManager.HasKey(MUSIC_VOLUME_PREFS))
            SetMusicVolume(0.8f);
        else
            SetMusicVolume(SaveManager.Get<float>(MUSIC_VOLUME_PREFS));

        if (!SaveManager.HasKey(MUSIC_MUTE_PREFS))
            SetMusicMute(true);
        else
            SetMusicMute(SaveManager.Get<int>(MUSIC_MUTE_PREFS) == 1 ? false : true);

        if (!SaveManager.HasKey(RENDER_SCALE_PREFS))
            SetRenderScale(1);
        else
            SetRenderScale(SaveManager.Get<float>(RENDER_SCALE_PREFS));

        if (!SaveManager.HasKey(QUALITY_LEVEL_PREFS))
            SetQualityLevel(1); // 0 == Low, 1 == Medium, 2 == High
        else
            SetQualityLevel(SaveManager.Get<int>(QUALITY_LEVEL_PREFS));

        _presentor.SetSFXVolumeSlider(SaveManager.Get<float>(SFX_VOLUME_PREFS));
        _presentor.SetSFXMuteToggle(SaveManager.Get<int>(SFX_MUTE_PREFS) == 1 ? true : false);
        _presentor.SetMusicVolumeSlider(SaveManager.Get<float>(MUSIC_VOLUME_PREFS));
        _presentor.SetMusicMuteToggle(SaveManager.Get<int>(MUSIC_MUTE_PREFS) == 1 ? true : false);
        _presentor.SetRenderScaleSlider(SaveManager.Get<float>(RENDER_SCALE_PREFS));
    }

    public void SetSFXVolume(float value)
    {
        SaveManager.Set(SFX_VOLUME_PREFS, value);
        OnMusicChange?.Invoke();
    }

    public void SetSFXMute(bool isOn)
    {
        SaveManager.Set(SFX_MUTE_PREFS, !isOn ? 1 : 0);
        OnMusicChange?.Invoke();
    }

    public void SetMusicVolume(float value)
    {
        SaveManager.Set(MUSIC_VOLUME_PREFS, value);
        OnMusicChange?.Invoke();
    }

    public void SetMusicMute(bool isOn)
    {
        SaveManager.Set(MUSIC_MUTE_PREFS, !isOn ? 1 : 0);
        OnMusicChange?.Invoke();
    }

    public void SetRenderScale(float value)
    {
        SaveManager.Set(RENDER_SCALE_PREFS, value);

        if (urpAsset)
            urpAsset.renderScale = value;
    }

    public void SetQualityLevel(int value)
    {
        SaveManager.Set(QUALITY_LEVEL_PREFS, value);
        QualitySettings.SetQualityLevel(value);
        urpAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
        SetRenderScale(SaveManager.Get<int>(RENDER_SCALE_PREFS));
        OnVideoChange?.Invoke();
        _presentor.SetQualityLevel(value);
    }
}