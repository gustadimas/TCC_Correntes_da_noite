using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UniversalAudioComponent : MonoBehaviour
{
    [Header("Configurações de Áudio")]
    [SerializeField] List<AudioTriggerConfig> audioTriggers = new List<AudioTriggerConfig>();
    [SerializeField] bool usePositionalAudio = false;
    [Tooltip("Posição do áudio, se null, usa a posição do próprio objeto")]
    [SerializeField] Transform audioSourcePosition; 

    [System.Serializable]
    public class AudioTriggerConfig
    {
        [Header("Trigger Configuration")]
        public string triggerName; // Nome personalizado para o trigger
        public AudioEvent audioEvent;
        [Tooltip("Configuração de áudio personalizada, se necessário")]
        public AudioConfig customAudioConfig; 
        public bool useCustomConfig = false;
        
        [Header("Trigger Conditions")]
        public TriggerCondition condition = TriggerCondition.Manual;
        [Tooltip("Cooldown em segundos antes de poder tocar novamente")]
        public float cooldown = 0f; 
        
        [Header("Events")]
        public UnityEvent onAudioPlayed;
        
        [HideInInspector]
        public float lastPlayedTime;
    }

    public enum TriggerCondition
    {
        Manual,           // Chamado por código
        OnStart,          // No Start()
        OnEnable,         // Quando o objeto é ativado
        OnTriggerEnter,   // Quando algo entra no trigger
        OnCollisionEnter, // Quando algo colide
        OnMouseClick,     // Quando clicado
        OnCustomEvent     // Quando um evento personalizado é chamado
    }

    Dictionary<string, AudioTriggerConfig> triggerMap = new Dictionary<string, AudioTriggerConfig>();

    void Awake()
    {
        MapTriggers();
        if (audioSourcePosition == null)
            audioSourcePosition = transform;
    }

    void Start()
    {
        ExecuteTriggersByCondition(TriggerCondition.OnStart);
    }

    void OnEnable()
    {
        ExecuteTriggersByCondition(TriggerCondition.OnEnable);
    }

    void OnTriggerEnter(Collider other)
    {
        ExecuteTriggersByCondition(TriggerCondition.OnTriggerEnter);
    }

    void OnCollisionEnter(Collision collision)
    {
        ExecuteTriggersByCondition(TriggerCondition.OnCollisionEnter);
    }

    void OnMouseDown()
    {
        ExecuteTriggersByCondition(TriggerCondition.OnMouseClick);
    }

    void MapTriggers()
    {
        triggerMap.Clear();
        foreach (var trigger in audioTriggers)
        {
            if (!string.IsNullOrEmpty(trigger.triggerName))
            {
                triggerMap[trigger.triggerName] = trigger;
            }
        }
    }

    void ExecuteTriggersByCondition(TriggerCondition condition)
    {
        foreach (var trigger in audioTriggers)
        {
            if (trigger.condition == condition)
            {
                PlayAudioTrigger(trigger);
            }
        }
    }

    public void PlayAudio(string triggerName)
    {
        if (triggerMap.TryGetValue(triggerName, out AudioTriggerConfig trigger))
        {
            PlayAudioTrigger(trigger);
        }
        else
        {
            Debug.LogWarning($"Trigger '{triggerName}' não encontrado em {gameObject.name}!");
        }
    }

    public void PlayAudio(AudioEvent audioEvent)
    {
        Vector3? position = usePositionalAudio ? audioSourcePosition.position : null;
        AudioManager.Instance.PlayAudio(audioEvent, position);
    }

    public void StopAudio(AudioEvent audioEvent)
    {
        AudioManager.Instance.StopAudioEvent(audioEvent);
    }
    
    public void StopAudio(string triggerName)
    {
        if (triggerMap.TryGetValue(triggerName, out AudioTriggerConfig trigger))
        {
            AudioManager.Instance.StopAudioEvent(trigger.audioEvent);
        }
        else
        {
            Debug.LogWarning($"Trigger '{triggerName}' não encontrado em {gameObject.name}!");
        }
    }
    
    public void StopAllAudio()
    {
        AudioManager.Instance.StopAllAudio();
    }

    public void PlayAudio(AudioConfig audioConfig)
    {
        Vector3? position = usePositionalAudio ? audioSourcePosition.position : null;
        AudioManager.Instance.PlayAudio(audioConfig, position);
    }

    void PlayAudioTrigger(AudioTriggerConfig trigger)
    {
        if (Time.time - trigger.lastPlayedTime < trigger.cooldown)
            return;

        trigger.lastPlayedTime = Time.time;

        Vector3? position = usePositionalAudio ? audioSourcePosition.position : null;

        if (trigger.useCustomConfig && trigger.customAudioConfig.clip != null)
        {
            AudioManager.Instance.PlayAudio(trigger.customAudioConfig, position);
        }
        else
        {
            AudioManager.Instance.PlayAudio(trigger.audioEvent, position);
        }

        trigger.onAudioPlayed?.Invoke();
    }

    public void TriggerCustomEvent(string triggerName)
    {
        if (triggerMap.TryGetValue(triggerName, out AudioTriggerConfig trigger))
        {
            if (trigger.condition == TriggerCondition.OnCustomEvent)
            {
                PlayAudioTrigger(trigger);
            }
        }
    }

    public void PlaySwipeSound()
    {
        PlayAudio(AudioEvent.WallTransition);
    }

    public void PlayJumpSound()
    {
        PlayAudio(AudioEvent.PlayerJump);
    }

    public void PlayLandingSound()
    {
        PlayAudio(AudioEvent.PlayerLanding);
    }

    public void AddAudioTrigger(string triggerName, AudioEvent audioEvent, TriggerCondition condition = TriggerCondition.Manual, float cooldown = 0f)
    {
        AudioTriggerConfig newTrigger = new AudioTriggerConfig
        {
            triggerName = triggerName,
            audioEvent = audioEvent,
            condition = condition,
            cooldown = cooldown,
            useCustomConfig = false
        };

        audioTriggers.Add(newTrigger);
        triggerMap[triggerName] = newTrigger;
    }

    public void RemoveAudioTrigger(string triggerName)
    {
        if (triggerMap.ContainsKey(triggerName))
        {
            audioTriggers.RemoveAll(t => t.triggerName == triggerName);
            triggerMap.Remove(triggerName);
        }
    }

    public bool HasTrigger(string triggerName)
    {
        return triggerMap.ContainsKey(triggerName);
    }

    public string[] GetTriggerNames()
    {
        return new string[triggerMap.Keys.Count];
    }

    [ContextMenu("Test All Audio Triggers")]
    public void TestAllAudioTriggers()
    {
        foreach (var trigger in audioTriggers)
        {
            PlayAudioTrigger(trigger);
        }
    }

    [ContextMenu("Test Stop Audio - First Trigger")]
    public void TestStopAudioFirstTrigger()
    {
        if (audioTriggers.Count > 0)
        {
            var firstTrigger = audioTriggers[0];
            Debug.Log($"Testing StopAudio for trigger: {firstTrigger.triggerName} with AudioEvent: {firstTrigger.audioEvent}");
            StopAudio(firstTrigger.audioEvent);
        }
        else
        {
            Debug.LogWarning("No audio triggers configured to test StopAudio!");
        }
    }

    [ContextMenu("Test Stop Audio - All Triggers")]
    public void TestStopAudioAllTriggers()
    {
        if (audioTriggers.Count > 0)
        {
            Debug.Log($"Testing StopAudio for all {audioTriggers.Count} triggers");
            foreach (var trigger in audioTriggers)
            {
                if (trigger.audioEvent != null)
                {
                    Debug.Log($"Stopping audio for trigger: {trigger.triggerName}");
                    StopAudio(trigger.audioEvent);
                }
            }
        }
        else
        {
            Debug.LogWarning("No audio triggers configured to test StopAudio!");
        }
    }

    [ContextMenu("Test Stop All Audio")]
    public void TestStopAllAudio()
    {
        Debug.Log("Testing StopAllAudio - stopping all audio from AudioManager");
        StopAllAudio();
    }
}