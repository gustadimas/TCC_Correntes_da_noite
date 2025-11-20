using UnityEngine;

namespace CorrentesDaNoite.Audio
{
    public readonly struct SoundData
    {
        public readonly Vector3 position;
        public readonly float range;
        public readonly SoundType soundType;
        public readonly GameObject emitter;
        public readonly float timestamp;

        public SoundData(Vector3 position, float range, SoundType soundType, GameObject emitter = null)
        {
            this.position = position;
            this.range = range;
            this.soundType = soundType;
            this.emitter = emitter;
            this.timestamp = Time.time;
        }
    }
}