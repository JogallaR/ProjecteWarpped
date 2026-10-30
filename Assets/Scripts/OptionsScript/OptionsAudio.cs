using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsAudio : MonoBehaviour
{
[Header("Audio Mixer")]
[SerializeField] private AudioMixer audioMixer;

[Header("Controls de volum")]
[SerializeField] private Slider masterSlider;
[SerializeField] private Slider musicSlider;
[SerializeField] private Slider sfxSlider;

private const string MasterKey = "MasterVolume";
private const string MusicKey = "MusicVolume";
private const string SFXKey = "SFXVolume";

private void Start()
{
    ConfigureSlider(masterSlider, MasterKey, "MasterVolume", 1f);
    ConfigureSlider(musicSlider, MusicKey, "MusicVolume", 0.8f);
    ConfigureSlider(sfxSlider, SFXKey, "SFXVolume", 1f);

    if (masterSlider != null)
        masterSlider.onValueChanged.AddListener(ChangeMaster);

    if (musicSlider != null)
        musicSlider.onValueChanged.AddListener(ChangeMusic);

    if (sfxSlider != null)
        sfxSlider.onValueChanged.AddListener(ChangeSFX);
}

private void ConfigureSlider(
    Slider slider, string key, string parameter, float defaultValue)
{
    if (slider == null || audioMixer == null)
    {
        Debug.LogError(
            $"Falta assignar el Slider o l'AudioMixer: {parameter}", this);
        return;
    }

    slider.minValue = 0.0001f;
    slider.maxValue = 1f;

    float value = Mathf.Clamp(
        PlayerPrefs.GetFloat(key, defaultValue),
        slider.minValue,
        slider.maxValue);

    slider.SetValueWithoutNotify(value);
    SetVolume(parameter, value);
}

private void ChangeMaster(float value)
{
    ChangeVolume("MasterVolume", MasterKey, value);
}

private void ChangeMusic(float value)
{
    ChangeVolume("MusicVolume", MusicKey, value);
}

private void ChangeSFX(float value)
{
    ChangeVolume("SFXVolume", SFXKey, value);
}

private void ChangeVolume(string parameter, string key, float value)
{
    SetVolume(parameter, value);
    PlayerPrefs.SetFloat(key, value);
    PlayerPrefs.Save();
}

private void SetVolume(string parameter, float value)
{
    float decibels = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;

    if (!audioMixer.SetFloat(parameter, decibels))
    {
        Debug.LogError(
            $"No s'ha pogut modificar el paràmetre '{parameter}'.", this);
    }
}

private void OnDestroy()
{
    if (masterSlider != null)
        masterSlider.onValueChanged.RemoveListener(ChangeMaster);

    if (musicSlider != null)
        musicSlider.onValueChanged.RemoveListener(ChangeMusic);

    if (sfxSlider != null)
        sfxSlider.onValueChanged.RemoveListener(ChangeSFX);
}

}