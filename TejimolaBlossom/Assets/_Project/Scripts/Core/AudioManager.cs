using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Tejimola.Utils;

namespace Tejimola.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Audio Mixer")]
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup sfxGroup;
        public AudioMixerGroup voiceGroup;
        public AudioMixerGroup ambientGroup;

        [Header("Music Sources")]
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private AudioSource _ambientSource;
        private bool _musicSourceAActive = true;

        [Header("SFX Pool")]
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        private const int SFX_POOL_SIZE = 10;

        [Header("Settings")]
        private float _masterVolume = 1f;
        private float _musicVolume = 0.7f;
        private float _sfxVolume = 0.8f;
        private float _voiceVolume = 1f;

        // Loaded audio clips cache
        private Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }

        void InitializeAudioSources()
        {
            _musicSourceA = gameObject.AddComponent<AudioSource>();
            _musicSourceA.loop = true;
            _musicSourceA.playOnAwake = false;
            _musicSourceA.volume = _musicVolume;

            _musicSourceB = gameObject.AddComponent<AudioSource>();
            _musicSourceB.loop = true;
            _musicSourceB.playOnAwake = false;
            _musicSourceB.volume = 0f;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.playOnAwake = false;
            _ambientSource.volume = _musicVolume * 0.5f;

            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                var sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = _sfxVolume;
                _sfxPool.Add(sfxSource);
            }
        }

        public AudioClip LoadClip(string path)
        {
            if (_clipCache.ContainsKey(path))
                return _clipCache[path];

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
                _clipCache[path] = clip;
            return clip;
        }

        // Music
        public void PlayMusic(AudioClip clip, float fadeDuration = 1.5f)
        {
            if (clip == null) return;
            StartCoroutine(CrossfadeMusic(clip, fadeDuration));
        }

        public void PlayMusic(string resourcePath, float fadeDuration = 1.5f)
        {
            AudioClip clip = LoadClip(resourcePath);
            PlayMusic(clip, fadeDuration);
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
        {
            AudioSource fadeOut = _musicSourceAActive ? _musicSourceA : _musicSourceB;
            AudioSource fadeIn = _musicSourceAActive ? _musicSourceB : _musicSourceA;

            fadeIn.clip = newClip;
            fadeIn.Play();

            float timer = 0f;
            float startVolumeOut = fadeOut.volume;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                fadeOut.volume = Mathf.Lerp(startVolumeOut, 0f, t);
                fadeIn.volume = Mathf.Lerp(0f, _musicVolume, t);
                yield return null;
            }

            fadeOut.Stop();
            fadeOut.volume = 0f;
            fadeIn.volume = _musicVolume;
            _musicSourceAActive = !_musicSourceAActive;
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            StartCoroutine(FadeOutMusic(fadeDuration));
        }

        private IEnumerator FadeOutMusic(float duration)
        {
            AudioSource active = _musicSourceAActive ? _musicSourceA : _musicSourceB;
            float startVolume = active.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                active.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            active.Stop();
            active.volume = 0f;
        }

        // Ambient
        public void PlayAmbient(AudioClip clip, float fadeDuration = 2f)
        {
            if (clip == null) return;
            StartCoroutine(FadeInAmbient(clip, fadeDuration));
        }

        private IEnumerator FadeInAmbient(AudioClip clip, float duration)
        {
            _ambientSource.clip = clip;
            _ambientSource.volume = 0f;
            _ambientSource.Play();

            float timer = 0f;
            float targetVol = _musicVolume * 0.5f;
            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                _ambientSource.volume = Mathf.Lerp(0f, targetVol, timer / duration);
                yield return null;
            }
        }

        // SFX
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            AudioSource source = GetAvailableSFXSource();
            source.clip = clip;
            source.volume = _sfxVolume * volumeScale;
            source.Play();
        }

        public void PlaySFX(string resourcePath, float volumeScale = 1f)
        {
            AudioClip clip = LoadClip(resourcePath);
            PlaySFX(clip, volumeScale);
        }

        public AudioSource PlaySFXLooping(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return null;
            AudioSource source = GetAvailableSFXSource();
            source.clip = clip;
            source.volume = _sfxVolume * volumeScale;
            source.loop = true;
            source.Play();
            return source;
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                    return source;
            }
            // All busy, reuse the first
            return _sfxPool[0];
        }

        // Rhythm-specific: precise timing playback
        public void PlaySFXScheduled(AudioClip clip, double dspTime)
        {
            if (clip == null) return;
            AudioSource source = GetAvailableSFXSource();
            source.clip = clip;
            source.volume = _sfxVolume;
            source.PlayScheduled(dspTime);
        }

        // Volume controls
        public void SetMasterVolume(float vol)
        {
            _masterVolume = Mathf.Clamp01(vol);
            AudioListener.volume = _masterVolume;
        }

        public void SetMusicVolume(float vol)
        {
            _musicVolume = Mathf.Clamp01(vol);
            if (_musicSourceAActive)
                _musicSourceA.volume = _musicVolume;
            else
                _musicSourceB.volume = _musicVolume;
            _ambientSource.volume = _musicVolume * 0.5f;
        }

        public void SetSFXVolume(float vol)
        {
            _sfxVolume = Mathf.Clamp01(vol);
        }

        public void SetVoiceVolume(float vol)
        {
            _voiceVolume = Mathf.Clamp01(vol);
        }

        // Act-specific music management
        public void PlayActMusic(GameAct act)
        {
            string musicPath = act switch
            {
                GameAct.Act1_HappyHome => "Audio/Music/act1_happy",
                GameAct.Act1_Funeral => "Audio/Music/act1_funeral",
                GameAct.Act2_Descent => "Audio/Music/act2_descent",
                GameAct.Act2_Dheki => "Audio/Music/act2_dheki",
                GameAct.Act2_Burial => "Audio/Music/act2_burial",
                GameAct.Act3_DomArrival => "Audio/Music/act3_arrival",
                GameAct.Act3_DualTimeline => "Audio/Music/act3_dual",
                GameAct.Act4_Confrontation => "Audio/Music/act4_boss",
                GameAct.Epilogue => "Audio/Music/epilogue",
                _ => "Audio/Music/menu"
            };
            PlayMusic(musicPath);
        }
    }
}
