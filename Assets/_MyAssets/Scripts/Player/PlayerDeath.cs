using UnityEngine;
using CorrentesDaNoite.Checkpoint;

namespace CorrentesDaNoite.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] float deathYThreshold = -10f;
        [SerializeField] bool checkFallDeath = true;

        [Header("Capture")]
        [SerializeField] float captureRespawnDelay = 1.5f;

        public System.Action OnCaptureStarted;
        public System.Action OnCaptureFinished;

        bool _captureInProgress;
        float _captureTimer;

        public bool IsCaptureInProgress => _captureInProgress;

        void Update()
        {
            if (checkFallDeath && transform.position.y < deathYThreshold)
                Die();

            if (_captureInProgress)
            {
                _captureTimer += Time.deltaTime;
                if (_captureTimer >= captureRespawnDelay)
                {
                    _captureInProgress = false;
                    _captureTimer = 0f;
                    CheckpointManager.Instance?.RespawnPlayer(gameObject, true);
                    OnCaptureFinished?.Invoke();
                }
            }
        }

        public void Die() => CheckpointManager.Instance?.RespawnPlayer(gameObject, false);

        public void DieFromLight() => CheckpointManager.Instance?.RespawnPlayer(gameObject, true);

        public void DieFromCapture()
        {
            _captureInProgress = true;
            _captureTimer = 0f;
            OnCaptureStarted?.Invoke();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DeathZone"))
                Die();
        }
    }
}