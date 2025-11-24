using UnityEngine;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.Audio.Footsteps
{
    [System.Serializable]
    public struct FootstepConfig
    {
        [Header("Clips")]
        public AudioClip[] clips;
        public AudioType type;
        public bool spatialAudio;

        [Header("Volumes e Pitch")]
        public float baseVolume;
        public bool usePitchVariation;
        public float minPitch;
        public float maxPitch;

        [Header("Timing")]
        public float minTimeBetweenSteps;

        public AudioConfig ToAudioConfig(int clipIndex)
        {
            if (clips == null || clips.Length == 0)
                return default;

            int clamped = Mathf.Clamp(clipIndex, 0, clips.Length - 1);
            return BuildConfig(clamped);
        }

        public AudioConfig GetRandomAudioConfig()
        {
            if (clips == null || clips.Length == 0)
                return default;

            int randomIndex = Random.Range(0, clips.Length);
            return BuildConfig(randomIndex);
        }

        internal AudioConfig BuildConfig(int clipIndex)
        {
            return new AudioConfig
            {
                clip = clips[clipIndex],
                type = type,
                volume = baseVolume,
                pitch = usePitchVariation ? Random.Range(minPitch, maxPitch) : 1f,
                loop = false,
                spatialAudio = spatialAudio
            };
        }
    }
}