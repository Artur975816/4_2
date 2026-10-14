using UnityEngine;

public static class SaveSystem
{
    private const string CoinsKey = "TotalCoins";
    private const string UnlockedKey = "UnlockedLevel";
    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";

    public static int TotalCoins { get => PlayerPrefs.GetInt(CoinsKey, 0); }
    public static int UnlockedLevel { get => PlayerPrefs.GetInt(UnlockedKey, 1); }

    public static void AddTotalCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, TotalCoins + amount);
        PlayerPrefs.Save();
    }

    public static void UnlockLevel(int level)
    {
        if (level > UnlockedLevel) PlayerPrefs.SetInt(UnlockedKey, level);
        PlayerPrefs.Save();
    }

    public static float MusicVolume { get => PlayerPrefs.GetFloat(MusicKey, 0.7f); }
    public static float SfxVolume { get => PlayerPrefs.GetFloat(SfxKey, 0.8f); }

    public static void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey(UnlockedKey);
        PlayerPrefs.Save();
    }
}
