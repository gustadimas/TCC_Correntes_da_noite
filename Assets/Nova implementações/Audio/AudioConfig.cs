using UnityEngine;

[System.Serializable]
public struct AudioConfig
{
    public AudioClip clip;
    public AudioType type;
    public float volume;
    public float pitch;
    public bool loop;
    public bool spatialAudio; // Para áudio 3D
}
