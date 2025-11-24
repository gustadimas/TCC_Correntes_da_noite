using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CorrentesDaNoite.Audio
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        public static IAudioService Instance { get; protected set; }

        [Header("Mixer")]
        [SerializeField] internal AudioMixer audioMixer;
        [SerializeField] internal string masterVolumeParameter = "MasterVolume";
        [SerializeField] internal string musicVolumeParameter = "MusicVolume";
        [SerializeField] internal string sfxVolumeParameter = "SfxVolume";
        [SerializeField] internal string musicGroupPath = "Master/Music";
        [SerializeField] internal string sfxGroupPath = "Master/SFX";
        [SerializeField] internal string uiGroupPath = "Master/SFX";
        [SerializeField] internal string spatialGroupPath = "Master/SFX";

        [Header("Dedicated Sources")]
        [SerializeField] internal AudioSource musicSource;
        [SerializeField] internal AudioSource ambientSource;

        [Header("Pool Settings")]
        [SerializeField] internal int sfxPoolSize = 12;
        [SerializeField] internal int uiPoolSize = 6;
        [SerializeField] internal int spatialPoolSize = 8;

        [Header("Audio Events")]
        [SerializeField] internal List<AudioEventConfig> audioEvents = new List<AudioEventConfig>();

        protected readonly List<AudioSource> sfxSourcePool = new List<AudioSource>();
        protected readonly List<AudioSource> uiSourcePool = new List<AudioSource>();
        protected readonly List<AudioSource> spatialSourcePool = new List<AudioSource>();
        protected readonly Dictionary<AudioEvent, AudioConfig> eventToConfig = new Dictionary<AudioEvent, AudioConfig>();

        [System.Serializable]
        public class AudioEventConfig
        {
            public AudioEvent audioEvent;
            public AudioConfig audioConfig;
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
                return;
            }

            if (!ReferenceEquals(Instance, this))
                Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
                Instance = null;
        }

        protected virtual void Initialize()
        {
            InitializeDedicatedSources();
            CreateAudioSourcePools();
            MapAudioEvents();
        }

        protected virtual void InitializeDedicatedSources()
        {
            musicSource = EnsureAudioSource(musicSource, "Music_Source", musicGroupPath);
            ambientSource = EnsureAudioSource(ambientSource, "Ambient_Source", musicGroupPath);
        }

        protected AudioSource EnsureAudioSource(AudioSource source, string name, string mixerGroupPath)
        {
            if (source == null)
            {
                GameObject holder = new GameObject(name);
                holder.transform.SetParent(transform);
                source = holder.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = false;
            source.outputAudioMixerGroup = ResolveMixerGroup(mixerGroupPath);
            return source;
        }

        protected virtual void CreateAudioSourcePools()
        {
            BuildPool(sfxSourcePool, sfxPoolSize, "SFX_Pool", sfxGroupPath);
            BuildPool(uiSourcePool, uiPoolSize, "UI_Pool", uiGroupPath);
            BuildPool(spatialSourcePool, spatialPoolSize, "Spatial_Pool", spatialGroupPath);
        }

        protected void BuildPool(List<AudioSource> pool, int size, string namePrefix, string mixerGroupPath)
        {
            for (int i = 0; i < size; i++)
            {
                AudioSource source = CreatePooledAudioSource($"{namePrefix}_{i}", mixerGroupPath);
                pool.Add(source);
            }
        }

        protected AudioSource CreatePooledAudioSource(string name, string mixerGroupPath)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = ResolveMixerGroup(mixerGroupPath);
            return source;
        }

        protected virtual void MapAudioEvents()
        {
            eventToConfig.Clear();
            for (int i = 0; i < audioEvents.Count; i++)
            {
                AudioEventConfig config = audioEvents[i];
                eventToConfig[config.audioEvent] = config.audioConfig;
            }
        }

        public virtual void PlayEvent(AudioEvent audioEvent, Vector3? position = null)
        {
            if (eventToConfig.TryGetValue(audioEvent, out AudioConfig config))
                PlayConfig(config, position);
        }

        public virtual void PlayConfig(AudioConfig config, Vector3? position = null)
        {
            if (config.clip == null)
                return;

            AudioSource source = ResolveSource(config, position.HasValue);
            if (source == null)
                return;

            ConfigureSource(source, config, position);
            source.Play();

            if (!config.loop && (config.type == AudioType.Sfx || config.type == AudioType.Ui || position.HasValue))
                StartCoroutine(ReleaseAfterPlay(source, config.clip.length));
        }

        public virtual void StopEvent(AudioEvent audioEvent)
        {
            if (!eventToConfig.TryGetValue(audioEvent, out AudioConfig config))
                return;

            StopByConfig(config);
        }

        public virtual void PlayAmbient(AudioClip clip, float volume = 1f, bool loop = true, bool spatial = false, Vector3? position = null)
        {
            if (clip == null)
                return;

            AudioConfig config = new AudioConfig
            {
                clip = clip,
                type = AudioType.Ambient,
                volume = volume,
                pitch = 1f,
                loop = loop,
                spatialAudio = spatial
            };

            PlayConfig(config, position);
        }

        public virtual void StopAmbient()
        {
            if (ambientSource != null)
                ambientSource.Stop();
        }

        public virtual void StopAll()
        {
            musicSource?.Stop();
            ambientSource?.Stop();

            StopPool(sfxSourcePool);
            StopPool(uiSourcePool);
            StopPool(spatialSourcePool);
        }

        public virtual void SetBusVolume(AudioBus bus, float normalizedVolume)
        {
            switch (bus)
            {
                case AudioBus.Master:
                    SetMixerVolume(masterVolumeParameter, normalizedVolume);
                    break;
                case AudioBus.Music:
                    SetMixerVolume(musicVolumeParameter, normalizedVolume);
                    break;
                case AudioBus.Sfx:
                    SetMixerVolume(sfxVolumeParameter, normalizedVolume);
                    break;
            }
        }

        public virtual void SetMasterVolume(float normalizedVolume)
        {
            SetBusVolume(AudioBus.Master, normalizedVolume);
        }

        protected virtual void StopByConfig(AudioConfig config)
        {
            switch (config.type)
            {
                case AudioType.Music:
                    if (musicSource != null && musicSource.clip == config.clip)
                        musicSource.Stop();
                    break;
                case AudioType.Ambient:
                    if (ambientSource != null && ambientSource.clip == config.clip)
                        ambientSource.Stop();
                    break;
                case AudioType.Sfx:
                    StopClipInPool(sfxSourcePool, config.clip);
                    break;
                case AudioType.Ui:
                    StopClipInPool(uiSourcePool, config.clip);
                    break;
            }
        }

        protected virtual void StopClipInPool(List<AudioSource> pool, AudioClip clip)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].clip == clip)
                    pool[i].Stop();
            }
        }

        protected virtual void StopPool(List<AudioSource> pool)
        {
            for (int i = 0; i < pool.Count; i++)
                pool[i].Stop();
        }

        protected virtual AudioSource ResolveSource(AudioConfig config, bool hasPosition)
        {
            if (config.spatialAudio && hasPosition)
                return GetAvailableFromPool(spatialSourcePool, "Spatial_Pool", spatialGroupPath);

            switch (config.type)
            {
                case AudioType.Music:
                    return musicSource;
                case AudioType.Ambient:
                    return ambientSource;
                case AudioType.Ui:
                    return GetAvailableFromPool(uiSourcePool, "UI_Pool", uiGroupPath);
                default:
                    return GetAvailableFromPool(sfxSourcePool, "SFX_Pool", sfxGroupPath);
            }
        }

        protected virtual AudioSource GetAvailableFromPool(List<AudioSource> pool, string prefix, string mixerGroupPath)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                AudioSource source = pool[i];
                if (!source.isPlaying && source.clip == null)
                    return source;
            }

            for (int i = 0; i < pool.Count; i++)
            {
                AudioSource source = pool[i];
                if (!source.isPlaying)
                    return source;
            }

            AudioSource extraSource = CreatePooledAudioSource($"{prefix}_{pool.Count}", mixerGroupPath);
            pool.Add(extraSource);
            return extraSource;
        }

        protected virtual void ConfigureSource(AudioSource source, AudioConfig config, Vector3? position)
        {
            source.clip = config.clip;
            source.volume = config.volume;
            source.pitch = Mathf.Approximately(config.pitch, 0f) ? 1f : config.pitch;
            source.loop = config.loop;
            source.spatialBlend = config.spatialAudio && position.HasValue ? 1f : 0f;

            if (position.HasValue)
                source.transform.position = position.Value;
        }

        protected IEnumerator ReleaseAfterPlay(AudioSource source, float clipLength)
        {
            yield return new WaitForSeconds(clipLength + 0.1f);

            if (source != null && !source.loop && !source.isPlaying)
                source.clip = null;
        }

        protected void SetMixerVolume(string parameter, float normalizedVolume)
        {
            if (audioMixer == null || string.IsNullOrEmpty(parameter))
                return;

            float clamped = Mathf.Clamp01(normalizedVolume);
            float dbValue = clamped > 0f ? Mathf.Log10(clamped) * 20f : -80f;
            audioMixer.SetFloat(parameter, dbValue);
        }

        protected AudioMixerGroup ResolveMixerGroup(string path)
        {
            if (audioMixer == null || string.IsNullOrEmpty(path))
                return null;

            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups(path);
            if (groups != null && groups.Length > 0)
                return groups[0];

            return null;
        }
    }
}
