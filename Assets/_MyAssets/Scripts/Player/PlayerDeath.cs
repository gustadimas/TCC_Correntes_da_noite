using System.Collections;
using UnityEngine;
using CorrentesDaNoite.Checkpoint;
using CorrentesDaNoite.UI;

namespace CorrentesDaNoite.Player
{
    public class PlayerDeath : MonoBehaviour
    {
        const float CAPTURE_SLOW_SCALE = 0.3f;
        const float CAPTURE_SLOW_TIME = 0.7f;

        [SerializeField] float deathYThreshold = -10f;
        [SerializeField] bool checkFallDeath = true;

        [Header("Capture")]
        [SerializeField] float captureRespawnDelay = 1.5f;
        [SerializeField] VoiceLine[] _captureVoices;

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

        // chamado pelo CaptureHandler na hora que ele agarra ela
        public void OnGrabbed()
        {
            if (_captureVoices.Length > 0) _captureVoices[Random.Range(0, _captureVoices.Length)].Play();
            StartCoroutine(CaptureSlowMotion());
        }

        IEnumerator CaptureSlowMotion()
        {
            Time.timeScale = CAPTURE_SLOW_SCALE;
            yield return new WaitForSecondsRealtime(CAPTURE_SLOW_TIME);
            // se pausou no meio espera despausar senao o jogo volta a rodar com o menu aberto
            while (Time.timeScale == 0f) yield return null;
            Time.timeScale = 1f;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DeathZone"))
                Die();
        }
    }
}