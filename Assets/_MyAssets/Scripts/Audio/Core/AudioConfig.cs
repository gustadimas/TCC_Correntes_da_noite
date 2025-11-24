using UnityEngine;

namespace CorrentesDaNoite.Audio
{
    [System.Serializable]
    public struct AudioConfig
    {
        public AudioClip clip;
        public AudioType type;
        public float volume;
        public float pitch;
        public bool loop;
        public bool spatialAudio;
    }
}