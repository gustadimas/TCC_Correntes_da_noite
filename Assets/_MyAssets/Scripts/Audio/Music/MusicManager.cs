using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CorrentesDaNoite.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; protected set; }

        [Header("Mixer Routing")]
        [SerializeField] internal AudioMixerGroup musicGroup;

        [Header("Volume e Fade")]
        [SerializeField] internal float baseVolume = 1f;
        [SerializeField] internal float defaultFadeTime = 1.5f;

        [Header("Tabela de Músicas")]
        [SerializeField] internal List<MusicEntry> musicEntries = new List<MusicEntry>();

        protected AudioSource sourceA;
        protected AudioSource sourceB;
        protected AudioSource activeSource;
        protected Coroutine crossfadeRoutine;
        protected readonly Dictionary<string, AudioClip> musicMap = new Dictionary<string, AudioClip>();

        [System.Serializable]
        public class MusicEntry
        {
            public string key;
            public AudioClip clip;
        }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
                return;
            }

            if (Instance != this)
                Destroy(gameObject);
        }

        public static MusicManager GetOrCreate()
        {
            if (Instance != null)
                return Instance;

            MusicManager existing = FindFirstObjectByType<MusicManager>();
            if (existing != null)
                return existing;

            GameObject obj = new GameObject("MusicManager");
            MusicManager manager = obj.AddComponent<MusicManager>();
            DontDestroyOnLoad(obj);
            return manager;
        }

        protected void Initialize()
        {
            BuildMusicMap();
            sourceA = CreateSource("Music_Source_A");
            sourceB = CreateSource("Music_Source_B");
            activeSource = sourceA;
        }

        protected AudioSource CreateSource(string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            AudioSource src = obj.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.volume = 0f;
            src.outputAudioMixerGroup = musicGroup;
            return src;
        }

        protected void BuildMusicMap()
        {
            musicMap.Clear();
            for (int i = 0; i < musicEntries.Count; i++)
            {
                MusicEntry entry = musicEntries[i];
                if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                    musicMap[entry.key] = entry.clip;
            }
        }

        public void PlayMusic(AudioClip clip, float fadeTime = -1f)
        {
            if (clip == null)
            {
                StopMusic(fadeTime);
                return;
            }

            if (activeSource != null && activeSource.clip == clip && activeSource.isPlaying)
                return;

            float duration = fadeTime > 0f ? fadeTime : defaultFadeTime;
            StartCrossfade(clip, duration);
        }

        public void PlayMusic(string key, float fadeTime = -1f)
        {
            if (string.IsNullOrEmpty(key) || !musicMap.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning($"[MusicManager] Key '{key}' não encontrada na tabela de musicEntries.");
                return;
            }

            PlayMusic(clip, fadeTime);
        }

        public void StopMusic(float fadeTime = -1f)
        {
            float duration = fadeTime > 0f ? fadeTime : defaultFadeTime;
            StartCrossfade(null, duration);
        }

        protected void StartCrossfade(AudioClip targetClip, float fadeTime)
        {
            if (crossfadeRoutine != null)
                StopCoroutine(crossfadeRoutine);

            crossfadeRoutine = StartCoroutine(CrossfadeRoutine(targetClip, fadeTime));
        }

        protected IEnumerator CrossfadeRoutine(AudioClip targetClip, float fadeTime)
        {
            AudioSource from = activeSource;
            AudioSource to = activeSource == sourceA ? sourceB : sourceA;

            if (targetClip != null)
            {
                to.clip = targetClip;
                to.volume = 0f;
                to.Play();
            }

            if (fadeTime <= 0f)
            {
                if (from != null)
                {
                    from.Stop();
                    from.clip = null;
                }

                if (targetClip != null)
                    to.volume = baseVolume;

                activeSource = targetClip != null ? to : to;
                crossfadeRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                float t = elapsed / fadeTime;
                if (from != null)
                    from.volume = Mathf.Lerp(baseVolume, 0f, t);
                if (targetClip != null)
                    to.volume = Mathf.Lerp(0f, baseVolume, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (from != null)
            {
                from.Stop();
                from.clip = null;
                from.volume = 0f;
            }

            if (targetClip != null)
                to.volume = baseVolume;
            else
                to.volume = 0f;

            activeSource = targetClip != null ? to : to;
            crossfadeRoutine = null;
        }
    }
}