using UnityEngine;

namespace CorrentesDaNoite.Audio
{
    public enum AudioBus
    {
        Master,
        Music,
        Sfx
    }

    public interface IAudioService
    {
        void PlayEvent(AudioEvent audioEvent, Vector3? position = null);
        void PlayConfig(AudioConfig config, Vector3? position = null);
        void PlayAmbient(AudioClip clip, float volume = 1f, bool loop = true, bool spatial = false, Vector3? position = null);
        void StopAmbient();
        void StopEvent(AudioEvent audioEvent);
        void StopAll();
        void SetBusVolume(AudioBus bus, float normalizedVolume);
        void SetMasterVolume(float normalizedVolume);
    }
}