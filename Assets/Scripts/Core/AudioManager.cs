using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (Instance != null) return;
        new GameObject("Music Controller").AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        musicSource.clip = Resources.Load<AudioClip>("Audio/BGM_Platformer");
        ApplySavedVolumes();
        if (musicSource.clip != null && !musicSource.isPlaying) musicSource.Play();
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
