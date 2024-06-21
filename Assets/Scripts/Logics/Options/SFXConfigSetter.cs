using DeathMatch;
using UnityEngine;

public class SFXConfigSetter : MonoBehaviour
{
    public AudioSource[] sfxs;

    private void Start()
    {
        foreach (var sfx in sfxs)
        {
            sfx.volume = SaveManager.Get<float>(Options.SFX_VOLUME_PREFS);
            sfx.mute = SaveManager.Get<int>(Options.SFX_MUTE_PREFS) == 1 ? true : false;
        }
    }
}