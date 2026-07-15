using UnityEngine;

namespace CuocDuaKyThu.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        public AudioClip backgroundMusicMenu;
        public AudioClip backgroundMusicGameplay;
        public AudioClip diceRollSfx;
        public AudioClip correctAnsSfx;
        public AudioClip wrongAnsSfx;
        public AudioClip trapSfx;
        public AudioClip rewardSfx;
        public AudioClip wheelSfx;
        public AudioClip winSfx;

        private void Start()
        {
            UpdateVolume();
        }

        public void UpdateVolume()
        {
            var settings = GameManager.Instance.saveManager.CurrentSettings;
            if (musicSource != null) musicSource.volume = settings.musicVolume / 100f;
            if (sfxSource != null) sfxSource.volume = settings.sfxVolume / 100f;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayDiceSfx() => PlaySfx(diceRollSfx);
        public void PlayCorrectSfx() => PlaySfx(correctAnsSfx);
        public void PlayWrongSfx() => PlaySfx(wrongAnsSfx);
        public void PlayTrapSfx() => PlaySfx(trapSfx);
        public void PlayRewardSfx() => PlaySfx(rewardSfx);
        public void PlayWheelSfx() => PlaySfx(wheelSfx);
        public void PlayWinSfx() => PlaySfx(winSfx);
    }
}
