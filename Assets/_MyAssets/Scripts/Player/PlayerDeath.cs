using UnityEngine;
using CorrentesDaNoite.Checkpoint;

namespace CorrentesDaNoite.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] float deathYThreshold = -10f;
        [SerializeField] bool checkFallDeath = true;

        void Update()
        {
            if (checkFallDeath && transform.position.y < deathYThreshold)
                Die();
        }

        public void Die() => CheckpointManager.Instance?.RespawnPlayer(gameObject, false);

        public void DieFromLight() => CheckpointManager.Instance?.RespawnPlayer(gameObject, true);

        public void DieFromCapture()
        {
            CheckpointManager.Instance?.RespawnPlayer(gameObject, true);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DeathZone"))
                Die();
        }
    }
}