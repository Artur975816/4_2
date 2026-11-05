using UnityEngine;

namespace Assignment6
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class A6Audio : MonoBehaviour
    {
        public static A6Audio Instance { get; private set; }

        public AudioSource audioSource;

        [Header("Six player sound effects")]
        public AudioClip shotClip;
        public AudioClip hurtClip;
        public AudioClip pickupClip;
        public AudioClip explosionClip;
        public AudioClip winClip;
        public AudioClip jumpClip;

        private void Awake()
        {
            Instance = this;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = SaveSystem.SfxVolume;
            LoadMissingClips();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LoadMissingClips()
        {
            if (shotClip == null) shotClip = Resources.Load<AudioClip>("Audio/SFX_Shot");
            if (hurtClip == null) hurtClip = Resources.Load<AudioClip>("Audio/SFX_Hurt");
            if (pickupClip == null) pickupClip = Resources.Load<AudioClip>("Audio/SFX_Pickup");
            if (explosionClip == null) explosionClip = Resources.Load<AudioClip>("Audio/SFX_Explosion");
            if (winClip == null) winClip = Resources.Load<AudioClip>("Audio/SFX_Victory");
            if (jumpClip == null) jumpClip = Resources.Load<AudioClip>("Audio/SFX_Jump");
        }

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clip);
            else if (audioSource != null) audioSource.PlayOneShot(clip);
        }

        public static void PlayShot() { if (Instance != null) Instance.Play(Instance.shotClip); }
        public static void PlayHurt() { if (Instance != null) Instance.Play(Instance.hurtClip); }
        public static void PlayHeal() { if (Instance != null) Instance.Play(Instance.pickupClip); }
        public static void PlayPickup() => PlayHeal();
        public static void PlayExplosion() { if (Instance != null) Instance.Play(Instance.explosionClip); }
        public static void PlayWin() { if (Instance != null) Instance.Play(Instance.winClip); }
        public static void PlayJump() { if (Instance != null) Instance.Play(Instance.jumpClip); }
    }
}
