using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplySavedVolumes();
    }

    public void ApplySavedVolumes()
    {
        if (musicSource != null) musicSource.volume = SaveSystem.MusicVolume;
        if (sfxSource != null) sfxSource.volume = SaveSystem.SfxVolume;
    }

    public void SetMusicVolume(float value)
    {
        SaveSystem.SetMusicVolume(value);
        if (musicSource != null) musicSource.volume = value;
    }

    public void SetSfxVolume(float value)
    {
        SaveSystem.SetSfxVolume(value);
        if (sfxSource != null) sfxSource.volume = value;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}
