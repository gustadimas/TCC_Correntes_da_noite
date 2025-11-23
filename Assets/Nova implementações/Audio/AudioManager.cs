using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject audioManager = new GameObject("AudioManager");
                    _instance = audioManager.AddComponent<AudioManager>();
                    DontDestroyOnLoad(audioManager);
                }
            }
            return _instance;
        }
    }

    [Header("Audio Sources - Dedicated")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource ambientSource;

    [Header("Audio Mixers")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Audio Configurations")]
    [SerializeField] List<AudioEventConfig> audioEvents = new List<AudioEventConfig>();

    [Header("Pool Settings")]
    [SerializeField] int sfxPoolSize = 15;
    [SerializeField] int uiPoolSize = 5;
    [SerializeField] int spatialPoolSize = 10;

    List<AudioSource> sfxSourcePool = new List<AudioSource>();
    List<AudioSource> uiSourcePool = new List<AudioSource>();
    List<AudioSource> spatialSourcePool = new List<AudioSource>();

    Dictionary<AudioEvent, AudioConfig> eventToConfig = new Dictionary<AudioEvent, AudioConfig>();

    [System.Serializable]
    public class AudioEventConfig
    {
        public AudioEvent audioEvent;
        public AudioConfig audioConfig;
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioManager()
    {
        if (musicSource == null)
            musicSource = CreateAudioSource("Music_Source");
        
        if (ambientSource == null)
            ambientSource = CreateAudioSource("Ambient_Source");

        musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/Music")[0];
        ambientSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/Music")[0];
        CreateAudioSourcePools();
        MapAudioEvents();
    }

    AudioSource CreateAudioSource(string name)
    {
        GameObject audioObject = new GameObject(name);
        audioObject.transform.SetParent(transform);
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    void CreateAudioSourcePools()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource sfxSource = CreateAudioSource($"SFX_Pool_{i}");
            sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/SFX")[0];
            sfxSource.gameObject.SetActive(true);
            sfxSourcePool.Add(sfxSource);
        }

        for (int i = 0; i < uiPoolSize; i++)
        {
            AudioSource uiSource = CreateAudioSource($"UI_Pool_{i}");
            uiSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/SFX")[0];
            uiSource.gameObject.SetActive(true);
            uiSourcePool.Add(uiSource);
        }

        for (int i = 0; i < spatialPoolSize; i++)
        {
            AudioSource spatialSource = CreateAudioSource($"Spatial_Pool_{i}");
            spatialSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master/SFX")[0];            
            spatialSource.gameObject.SetActive(true);
            spatialSourcePool.Add(spatialSource);
        }
    }

    void MapAudioEvents()
    {
        eventToConfig.Clear();
        foreach (var eventConfig in audioEvents)
        {
            eventToConfig[eventConfig.audioEvent] = eventConfig.audioConfig;
        }
    }

    public void PlayAudio(AudioEvent audioEvent, Vector3? position = null)
    {        
        if (eventToConfig.TryGetValue(audioEvent, out AudioConfig config))
        {
            if (config.clip == null)
            {
                Debug.LogError($"AudioClip para {audioEvent} é null!");
                return;
            }
            
            PlayAudio(config, position);
        }
        else
        {
            Debug.LogWarning($"AudioEvent {audioEvent} não configurado no AudioManager!");
        }
    }

    public void PlayAudio(AudioConfig config, Vector3? position = null)
    {
        if (config.clip == null)
        {
            Debug.LogWarning("AudioClip é null!");
            return;
        }

        AudioSource sourceToUse = null;

        if (config.spatialAudio && position.HasValue)
        {
            sourceToUse = GetAvailableAudioSource(spatialSourcePool, "Spatial");
            if (sourceToUse != null)
            {
                ConfigureAudioSource(sourceToUse, config);
                sourceToUse.transform.position = position.Value;
                sourceToUse.spatialBlend = 1f;
                sourceToUse.Play();
                
                if (!config.loop)
                    StartCoroutine(ReleaseAudioSource(sourceToUse, config.clip.length));
            }
            else
            {
                Debug.LogError("Não conseguiu obter AudioSource espacial!");
            }
            return;
        }

        switch (config.type)
        {
            case AudioType.SFX:
                sourceToUse = GetAvailableAudioSource(sfxSourcePool, "SFX");
                break;
                
            case AudioType.UI:
                sourceToUse = GetAvailableAudioSource(uiSourcePool, "UI");
                break;
                
            case AudioType.Music:
                sourceToUse = musicSource;
                break;
                
            case AudioType.Ambient:
                sourceToUse = ambientSource;
                break;
        }

        if (sourceToUse != null)
        {
            ConfigureAudioSource(sourceToUse, config);
            sourceToUse.spatialBlend = 0f;
            sourceToUse.Play();

            if (!config.loop && (config.type == AudioType.SFX || config.type == AudioType.UI))
            {
                StartCoroutine(ReleaseAudioSource(sourceToUse, config.clip.length));
            }
        }
        else
        {
            Debug.LogError($"Não foi possível obter AudioSource para {config.type}");
        }
    }

    void ConfigureAudioSource(AudioSource source, AudioConfig config)
    {
        source.clip = config.clip;
        source.volume = config.volume;
        source.pitch = config.pitch;
        source.loop = config.loop;
    }

    AudioSource GetAvailableAudioSource(List<AudioSource> pool, string poolType)
    {
        foreach (var source in pool)
        {
            if (!source.isPlaying && source.clip == null)
            {
                return source;
            }
        }
        
        foreach (var source in pool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        AudioSource newSource = CreateAudioSource($"{poolType}_Pool_{pool.Count}");
        newSource.gameObject.SetActive(true);
        pool.Add(newSource);
        return newSource;
    }

    IEnumerator ReleaseAudioSource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        
        if (source != null && !source.loop && !source.isPlaying)
        {
            source.clip = null;
        }
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void SetVolumeByType(AudioType type, float volume)
    {
        switch (type)
        {
            case AudioType.Music:
                musicSource.volume = volume;
                break;
            case AudioType.Ambient:
                ambientSource.volume = volume;
                break;
            case AudioType.SFX:
                foreach (var source in sfxSourcePool)
                    source.volume = volume;
                break;
            case AudioType.UI:
                foreach (var source in uiSourcePool)
                    source.volume = volume;
                break;
        }
    }

    public void StopAudio(AudioType type)
    {
        switch (type)
        {
            case AudioType.Music:
                musicSource.Stop();
                break;
            case AudioType.Ambient:
                ambientSource.Stop();
                break;
            case AudioType.SFX:
                foreach (var source in sfxSourcePool)
                    source.Stop();
                break;
            case AudioType.UI:
                foreach (var source in uiSourcePool)
                    source.Stop();
                break;
        }
    }

    public void StopAllAudio()
    {
        musicSource.Stop();
        ambientSource.Stop();
        
        foreach (var source in sfxSourcePool)
            source.Stop();
        
        foreach (var source in uiSourcePool)
            source.Stop();
        
        foreach (var source in spatialSourcePool)
            source.Stop();
    }

    public void StopAudioEvent(AudioEvent audioEvent)
    {
        if (!eventToConfig.TryGetValue(audioEvent, out AudioConfig config))
            return;

        List<AudioSource> poolToCheck = null;
        
        switch (config.type)
        {
            case AudioType.SFX:
                poolToCheck = sfxSourcePool;
                break;
            case AudioType.UI:
                poolToCheck = uiSourcePool;
                break;
            case AudioType.Music:
                if (musicSource.clip == config.clip && musicSource.isPlaying)
                    musicSource.Stop();
                return;
            case AudioType.Ambient:
                if (ambientSource.clip == config.clip && ambientSource.isPlaying)
                    ambientSource.Stop();
                return;
        }

        if (poolToCheck != null)
        {
            foreach (var source in poolToCheck)
            {
                if (source.clip == config.clip && source.isPlaying)
                {
                    source.Stop();
                }
            }
        }
    }

    [ContextMenu("Debug Pool Status")]
    public void DebugPoolStatus()
    {
        Debug.Log($"SFX Pool: {sfxSourcePool.Count} total, {GetActiveSourceCount(sfxSourcePool)} tocando");
        Debug.Log($"UI Pool: {uiSourcePool.Count} total, {GetActiveSourceCount(uiSourcePool)} tocando");
        Debug.Log($"Spatial Pool: {spatialSourcePool.Count} total, {GetActiveSourceCount(spatialSourcePool)} tocando");
    }

    int GetActiveSourceCount(List<AudioSource> pool)
    {
        int count = 0;
        foreach (var source in pool)
        {
            if (source.isPlaying)
                count++;
        }
        return count;
    }
}