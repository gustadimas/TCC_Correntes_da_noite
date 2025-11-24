using System.Collections.Generic;
using UnityEngine;

namespace CorrentesDaNoite.Audio
{
    [RequireComponent(typeof(Collider))]
    public class AudioZone : MonoBehaviour
    {
        [Header("Identidade")]
        [SerializeField] internal AudioZoneType zoneType = AudioZoneType.Default;
        [SerializeField] internal int priority;
        [SerializeField] internal string playerTag = "Player";

        [Header("Música")]
        [SerializeField] internal AudioClip musicClip;
        [SerializeField] internal float musicFadeTime = 1.5f;
        [SerializeField] internal string musicKey;

        [Header("Ambiente")]
        [SerializeField] internal AudioClip ambientClip;
        [SerializeField] internal float ambientVolume = 1f;
        [SerializeField] internal bool ambientSpatial;
        [SerializeField] internal bool stopAmbientOnExit = true;

        [Header("Estado de Mixer")]
        [SerializeField] internal AudioStateController stateController;
        [SerializeField] internal bool setChaseState;
        [SerializeField] internal bool setMenuState;
        [SerializeField] internal bool setExplorationState = true;
        [SerializeField] internal float stateTransitionTime = 0.6f;

        protected static readonly List<AudioZone> activeZones = new List<AudioZone>();
        protected static AudioZone currentZone;

        protected void Reset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
                return;

            if (!activeZones.Contains(this))
                activeZones.Add(this);

            EvaluateZones();
        }

        protected void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other))
                return;

            activeZones.Remove(this);
            EvaluateZones();
        }

        protected bool IsPlayer(Collider other)
        {
            if (string.IsNullOrEmpty(playerTag))
                return true;

            return other.CompareTag(playerTag);
        }

        protected void EvaluateZones()
        {
            AudioZone best = null;
            for (int i = 0; i < activeZones.Count; i++)
            {
                AudioZone zone = activeZones[i];
                if (best == null || zone.priority > best.priority)
                    best = zone;
            }

            if (best == currentZone)
                return;

            currentZone = best;
            if (currentZone != null)
                currentZone.Activate();
            else
                DeactivateAmbient();
        }

        protected void Activate()
        {
            PlayMusicForZone();
            PlayAmbientForZone();
            ApplyState();
        }

        protected void PlayMusicForZone()
        {
            if (MusicManager.Instance == null)
                return;

            if (!string.IsNullOrEmpty(musicKey))
                MusicManager.Instance.PlayMusic(musicKey, musicFadeTime);
            else
                MusicManager.Instance.PlayMusic(musicClip, musicFadeTime);
        }

        protected void PlayAmbientForZone()
        {
            if (AudioManager.Instance == null)
                return;

            if (ambientClip != null)
            {
                AudioConfig config = new AudioConfig
                {
                    clip = ambientClip,
                    type = AudioType.Ambient,
                    volume = ambientVolume,
                    pitch = 1f,
                    loop = true,
                    spatialAudio = ambientSpatial
                };

                Vector3? position = ambientSpatial ? transform.position : (Vector3?)null;
                AudioManager.Instance.PlayConfig(config, position);
            }
            else if (stopAmbientOnExit)
            {
                AudioManager.Instance.StopAmbient();
            }
        }

        protected void ApplyState()
        {
            if (stateController == null)
                return;

            if (setChaseState)
            {
                stateController.SetChaseState(stateTransitionTime);
                return;
            }

            if (setMenuState)
            {
                stateController.SetMenuState(stateTransitionTime);
                return;
            }

            if (setExplorationState)
                stateController.SetExplorationState(stateTransitionTime);
        }

        protected void DeactivateAmbient()
        {
            if (AudioManager.Instance == null || !stopAmbientOnExit)
                return;

            AudioManager.Instance.StopAmbient();
        }
    }
}