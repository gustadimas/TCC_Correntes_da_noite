using UnityEngine;
using System.Collections.Generic;

public class FootstepSystem : MonoBehaviour
{
    [Header("Configurações de Footsteps")]
    [SerializeField] FootstepConfig calcadaConfig;
    [SerializeField] FootstepConfig metalConfig;
    [SerializeField] FootstepConfig normalConfig;
    [SerializeField] FootstepConfig tetoBarracaConfig;

    [Header("Detecção de Superfície")]
    [SerializeField] Transform playerFeet;
    [SerializeField] LayerMask groundLayer = 1;
    [SerializeField] float raycastDistance = 0.5f;

    [Header("Audio Source")]
    [SerializeField] bool usePositionalAudio = true;
    [SerializeField] Transform audioSourcePosition;

    // Cache e controle
    SurfaceType currentSurface = SurfaceType.Normal;
    Dictionary<SurfaceType, FootstepConfig> surfaceConfigs;
    float lastStepTime;

    void Awake()
    {
        InitializeSurfaceConfigs();

        if (audioSourcePosition == null)
            audioSourcePosition = transform;

        if (playerFeet == null)
            playerFeet = transform;
    }

    void InitializeSurfaceConfigs()
    {
        surfaceConfigs = new Dictionary<SurfaceType, FootstepConfig>
        {
            { SurfaceType.Calcada, calcadaConfig },
            { SurfaceType.Metal, metalConfig },
            { SurfaceType.Normal, normalConfig },
            { SurfaceType.TetoBarraca, tetoBarracaConfig }
        };
    }

    void Update()
    {
        DetectSurface();
    }

    void DetectSurface()
    {
        if (Physics.Raycast(playerFeet.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            SurfaceDetector detector = hit.collider.GetComponent<SurfaceDetector>();
            if (detector != null)
            {
                currentSurface = detector.GetSurfaceType();
            }
            else
            {
                currentSurface = SurfaceType.Normal;
            }
        }
    }

    public void PlayFootstep()
    {
        PlayFootstep(currentSurface);
    }

    public void PlayFootstep(SurfaceType surfaceType)
    {
        if (!surfaceConfigs.TryGetValue(surfaceType, out FootstepConfig config))
        {
            Debug.LogWarning($"Configuração não encontrada para superfície: {surfaceType}");
            return;
        }

        if (Time.time - lastStepTime < config.minTimeBetweenSteps)
            return;

        lastStepTime = Time.time;

        AudioConfig audioConfig = config.GetRandomAudioConfig();

        Vector3? position = usePositionalAudio ? audioSourcePosition.position : null;
        AudioManager.Instance.PlayAudio(audioConfig, position);
    }

    public void PlayCalcadaStep() => PlayFootstep(SurfaceType.Calcada);
    public void PlayMetalStep() => PlayFootstep(SurfaceType.Metal);
    public void PlayNormalStep() => PlayFootstep(SurfaceType.Normal);
    public void PlayTetoBarracaStep() => PlayFootstep(SurfaceType.TetoBarraca);

    public SurfaceType GetCurrentSurface() => currentSurface;

    public void PlayFootstepViaUniversalAudio()
    {
        AudioEvent footstepEvent = GetAudioEventForSurface(currentSurface);
        UniversalAudioComponent audioComponent = GetComponent<UniversalAudioComponent>();

        if (audioComponent != null)
        {
            audioComponent.PlayAudio(footstepEvent);
        }
    }

    AudioEvent GetAudioEventForSurface(SurfaceType surface)
    {
        return surface switch
        {
            SurfaceType.Calcada => AudioEvent.FootstepCalcada,
            SurfaceType.Metal => AudioEvent.FootstepMetal,
            SurfaceType.Normal => AudioEvent.FootstepNormal,
            SurfaceType.TetoBarraca => AudioEvent.FootstepTetoBarraca,
            _ => AudioEvent.FootstepNormal
        };
    }

    void OnDrawGizmosSelected()
    {
        if (playerFeet != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerFeet.position, Vector3.down * raycastDistance);
        }
    }
}