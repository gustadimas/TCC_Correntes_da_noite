using UnityEngine;

[System.Serializable]
public struct FootstepConfig
{
    [Header("Configuração Básica")]
    public AudioClip[] clips;
    public AudioType type;
    public float baseVolume;
    public bool spatialAudio;
    
    [Header("Variação de Pitch")]
    public bool usePitchVariation;
    public float minPitch;
    public float maxPitch;
    
    [Header("Timing")]
    public float minTimeBetweenSteps;
    
    public AudioConfig ToAudioConfig(int clipIndex = 0)
    {
        if (clips == null || clips.Length == 0) return default;
        
        int index = clipIndex >= 0 && clipIndex < clips.Length ? clipIndex : 0;
        
        return new AudioConfig
        {
            clip = clips[index],
            type = type,
            volume = baseVolume,
            pitch = usePitchVariation ? Random.Range(minPitch, maxPitch) : 1f,
            loop = false,
            spatialAudio = spatialAudio
        };
    }
    
    public AudioConfig GetRandomAudioConfig()
    {
        if (clips == null || clips.Length == 0) return default;
        
        int randomIndex = Random.Range(0, clips.Length);
        return ToAudioConfig(randomIndex);
    }
}
