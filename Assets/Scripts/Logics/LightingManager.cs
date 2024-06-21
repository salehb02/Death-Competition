using System;
using UnityEngine;
using DG.Tweening;

public class LightingManager : MonoBehaviour
{
    [SerializeField] private LightingPreset[] presets;
    [SerializeField] private int defaultPreset;
    [SerializeField] private float lerpDuration = 1f;
    [SerializeField] private Material waterMaterial;

    private Light sun;
    private Material sky;
    private int currentPreset;

    [Serializable]
    public class LightingPreset
    {
        public string title;

        [Header("Sun")]
        public Color sunColor;
        public float sunIntensity;

        [Header("Skybox")]
        public float skyIntensity;

        [Header("Fog")]
        public Color fogColor;

        [Header("Environment Gradient")]
        [ColorUsage(true, true)] public Color gradientSkyColor;
        [ColorUsage(true, true)] public Color gradientEquatorColor;
        [ColorUsage(true, true)] public Color gradientGroundColor;

        [Header("Water")]
        [ColorUsage(true, true)] public Color waterDeepColor;
        [ColorUsage(true, true)] public Color waterShallowColor;
        [ColorUsage(true, true)] public Color waterHorizonColor;
        [ColorUsage(true, true)] public Color waterFoamColor;
    }

    private void Start()
    {
        sky = RenderSettings.skybox;
        sun = RenderSettings.sun;

        LoadPreset(presets[defaultPreset], true);
        currentPreset = defaultPreset;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            currentPreset++;
            currentPreset %= presets.Length;

            LoadPreset(presets[currentPreset]);
        }
    }

    public void LoadPreset(int index)
    {
        LoadPreset(presets[index]); 
    }

    public void LoadPreset(LightingPreset preset, bool force = false)
    {
        var lerpDuration = force ? 0 : this.lerpDuration;

        // Sun
        sun.DOColor(preset.sunColor, lerpDuration);
        sun.DOIntensity(preset.sunIntensity, lerpDuration);

        // Sky
        sky.DOFloat(preset.skyIntensity, "_Intensity", lerpDuration);

        // Fog 
        var currentColor = RenderSettings.fogColor;

        DOVirtual.Color(currentColor, preset.fogColor, lerpDuration, (color) =>
        {
            RenderSettings.fogColor = color;
        });

        // Environment Gradient
        var currentSkyColor = RenderSettings.ambientSkyColor;
        var currentEquatorColor = RenderSettings.ambientEquatorColor;
        var currentGroundColor = RenderSettings.ambientGroundColor;

        DOVirtual.Color(currentSkyColor, preset.gradientSkyColor, lerpDuration, (color) =>
        {
            RenderSettings.ambientSkyColor = color;
        });

        DOVirtual.Color(currentEquatorColor, preset.gradientEquatorColor, lerpDuration, (color) =>
        {
            RenderSettings.ambientEquatorColor = color;
        });

        DOVirtual.Color(currentGroundColor, preset.gradientGroundColor, lerpDuration, (color) =>
        {
            RenderSettings.ambientGroundColor = color;
        });

        // Water
        waterMaterial.DOColor(preset.waterDeepColor, "_BaseColor", lerpDuration);
        waterMaterial.DOColor(preset.waterShallowColor, "_ShallowColor", lerpDuration);
        waterMaterial.DOColor(preset.waterHorizonColor, "_HorizonColor", lerpDuration);
        waterMaterial.DOColor(preset.waterFoamColor, "_IntersectionColor", lerpDuration);
    }
}