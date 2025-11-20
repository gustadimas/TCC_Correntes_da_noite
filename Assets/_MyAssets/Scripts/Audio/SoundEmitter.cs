using UnityEngine;

namespace CorrentesDaNoite.Audio
{
    public static class SoundEmitter
    {
        public static void EmitSound(SoundData sound)
        {
            Collider[] colliders = Physics.OverlapSphere(sound.position, sound.range);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].TryGetComponent(out IHear listener))
                {
                    listener.OnSoundHeard(sound);
                }
            }
        }

        public static void EmitSound(Vector3 position, float range, SoundType soundType, GameObject emitter = null)
        {
            SoundData sound = new SoundData(position, range, soundType, emitter);
            EmitSound(sound);
        }
    }
}