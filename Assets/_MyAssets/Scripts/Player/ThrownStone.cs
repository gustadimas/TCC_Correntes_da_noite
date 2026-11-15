using CorrentesDaNoite.Audio;
using UnityEngine;

namespace CorrentesDaNoite.Player
{
    public class ThrownStone : MonoBehaviour
    {
        const float NOISE_RANGE = 14f;
        const float HIT_VOLUME = 0.6f;
        const float LIFE_TIME = 6f;
        const float LANDED_DAMPING = 4f;

        [SerializeField] AudioClip _hitSound;

        Rigidbody _body;
        bool _landed;

        // fica parada na mao ate ser jogada e sem interpolacao senao nao acompanha o osso
        public void Launch(Vector3 velocity, Collider thrower)
        {
            _body = GetComponent<Rigidbody>();
            var stoneCollider = GetComponent<Collider>();
            transform.SetParent(null, true);
            Physics.IgnoreCollision(stoneCollider, thrower);
            stoneCollider.enabled = true;
            _body.isKinematic = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.linearVelocity = velocity;
            Destroy(gameObject, LIFE_TIME);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_landed) return;
            _landed = true;
            _body.linearDamping = LANDED_DAMPING;
            _body.angularDamping = LANDED_DAMPING;
            AudioManager.Instance?.PlayConfig(new AudioConfig { clip = _hitSound, volume = HIT_VOLUME, pitch = Random.Range(0.9f, 1.1f), spatialAudio = true }, transform.position);
            SoundEmitter.EmitSound(transform.position, NOISE_RANGE, SoundType.Stone, gameObject);
        }
    }
}