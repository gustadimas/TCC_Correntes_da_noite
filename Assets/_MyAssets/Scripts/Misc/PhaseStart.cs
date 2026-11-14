using UnityEngine;
using Unity.Cinemachine;
using CorrentesDaNoite.Checkpoint;
using CorrentesDaNoite.Teleport;

namespace CorrentesDaNoite
{
    public class PhaseStart : MonoBehaviour
    {
        public enum Phase { Senzala, Patio, Casa, Perseguicao }

        public static bool ContinueRequested;

        [SerializeField] GameStartSequence _gameStart;
        [SerializeField] CinemachineCamera _startCamera;
        [SerializeField] TeleportZone[] _phaseTeleports;
        [SerializeField] Transform[] _phaseRoots;

        Phase _phase;

        private void Awake()
        {
            if (!ContinueRequested) return;
            ContinueRequested = false;
            StartAt(GetSavedPhase());
        }

        // entra pelo teleporte da fase anterior pra ter fade mapa camera checkpoint e intro igual no jogo
        private void Start()
        {
            if (_phase != Phase.Senzala) _phaseTeleports[(int)_phase - 1].TeleportPlayer(GameObject.FindGameObjectWithTag("Player"));
        }

        // tem que vir antes do start do game start pra pular o acordar
        public void StartAt(Phase phase)
        {
            _phase = phase;
            if (phase == Phase.Senzala) return;

            _gameStart.enabled = false;
            _startCamera.Priority = 0;
        }

        Phase GetSavedPhase()
        {
            string checkpointId = SaveSystemProvider.Get(true).LoadCheckpoint();
            foreach (var checkpoint in FindObjectsByType<Checkpoint.Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (checkpoint.CheckpointId != checkpointId) continue;
                for (int i = _phaseRoots.Length - 1; i > 0; i--)
                    if (checkpoint.transform.IsChildOf(_phaseRoots[i])) return (Phase)i;
            }

            return Phase.Senzala;
        }
    }
}