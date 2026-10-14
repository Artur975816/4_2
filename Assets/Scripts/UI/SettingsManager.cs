using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown languageDropdown;

    private void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(SaveSystem.MusicVolume);
            musicSlider.onValueChanged.AddListener(SetMusic);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(SaveSystem.SfxVolume);
            sfxSlider.onValueChanged.AddListener(SetSfx);
        }
        if (qualityDropdown != null)
        {
            qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
            qualityDropdown.onValueChanged.AddListener(QualitySettings.SetQualityLevel);
        }
    }

    private void OnDestroy()
    {
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(SetMusic);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(SetSfx);
    }

    private void SetMusic(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
        else SaveSystem.SetMusicVolume(value);
    }

    private void SetSfx(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(value);
        else SaveSystem.SetSfxVolume(value);
    }
}
