using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CorrentesDaNoite.UI;

namespace CorrentesDaNoite.Checkpoint
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [SerializeField] Checkpoint initialCheckpoint;
        [SerializeField] float fadeDuration = 1f;
        [SerializeField] Color fadeColor = Color.black;
        [SerializeField] float delayBeforeRespawn = 0.5f;
        [SerializeField] bool resetAnimatorOnRespawn = true;
        [SerializeField] string deathAnimationTrigger = "";
        [SerializeField] string capturedAnimationTrigger = "";
        [SerializeField] string defaultDeathReactionTrigger = "";
        [SerializeField] string respawnAnimationState = "";
        [SerializeField] string idleAnimationState = "";
        [SerializeField] bool waitForRespawnAnimation = false;
        [SerializeField] bool usePersistentSave = false;

        ISaveSystem _saveSystem;
        Checkpoint _currentCheckpoint;
        bool _isRespawning;
        Dictionary<string, Checkpoint> _checkpoints = new Dictionary<string, Checkpoint>();

        public static System.Action OnPlayerRespawned;

        public Checkpoint CurrentCheckpoint => _currentCheckpoint;
        public ISaveSystem SaveSystem => _saveSystem;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _saveSystem = usePersistentSave
                ? (ISaveSystem)new PlayerPrefsSaveSystem()
                : new MemorySaveSystem();

            RegisterAllCheckpoints();
        }

        void Start() => LoadCheckpointFromSave();

        void RegisterAllCheckpoints()
        {
            Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            foreach (var checkpoint in allCheckpoints)
            {
                if (!_checkpoints.ContainsKey(checkpoint.CheckpointId))
                    _checkpoints.Add(checkpoint.CheckpointId, checkpoint);
            }
        }

        void LoadCheckpointFromSave()
        {
            if (_saveSystem.HasSaveData())
            {
                string savedId = _saveSystem.LoadCheckpoint();
                if (_checkpoints.TryGetValue(savedId, out Checkpoint checkpoint))
                {
                    SetCheckpoint(checkpoint);
                    return;
                }
            }

            if (initialCheckpoint != null)
                SetCheckpoint(initialCheckpoint);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (_currentCheckpoint != null)
                _currentCheckpoint.SetActive(false);

            _currentCheckpoint = checkpoint;
            _currentCheckpoint.SetActive(true);

            _saveSystem.SaveCheckpoint(checkpoint.CheckpointId);
        }

        public void ClearSaveData() => _saveSystem.ClearSaveData();

        public Checkpoint GetCheckpointById(string checkpointId)
        {
            return _checkpoints.TryGetValue(checkpointId, out Checkpoint checkpoint) ? checkpoint : null;
        }

        public void RespawnPlayer(GameObject player, bool fromCapture = false, string reactionTrigger = "")
        {
            if (_isRespawning || _currentCheckpoint == null) return;
            StartCoroutine(RespawnSequence(player, fromCapture, reactionTrigger));
        }

        IEnumerator RespawnSequence(GameObject player, bool fromCapture, string reactionTrigger)
        {
            _isRespawning = true;

            var playerController = player.GetComponent<Player.PlayerController>();
            var animator = player.GetComponent<Animator>();

            if (playerController != null) playerController.enabled = false;

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("IsRunning");
                animator.SetBool("IsCrouching", false);
                animator.ResetTrigger("IsTeleporting");
                animator.SetFloat("Speed", 0f);

                if (fromCapture)
                {
                    if (!string.IsNullOrEmpty(capturedAnimationTrigger))
                        animator.ResetTrigger(capturedAnimationTrigger);
                }
                else
                {
                    string reactionToUse = !string.IsNullOrEmpty(reactionTrigger) ? reactionTrigger : defaultDeathReactionTrigger;
                    if (!string.IsNullOrEmpty(reactionToUse))
                        animator.SetTrigger(reactionToUse);
                    else if (!string.IsNullOrEmpty(deathAnimationTrigger))
                        animator.SetTrigger(deathAnimationTrigger);
                }
            }

            yield return new WaitForSeconds(delayBeforeRespawn);

            if (FadeController.Instance == null)
            {
                ExecuteRespawn(player, animator);
                if (playerController != null) playerController.enabled = true;
                _isRespawning = false;
                yield break;
            }

            FadeController.Instance.SetFadeColor(fadeColor);

            bool fadeOutComplete = false;
            FadeController.Instance.FadeOut(() => fadeOutComplete = true, fadeDuration);
            yield return new WaitUntil(() => fadeOutComplete);

            ExecuteRespawn(player, animator);

            bool fadeInComplete = false;
            FadeController.Instance.FadeIn(() => fadeInComplete = true, fadeDuration);
            yield return new WaitUntil(() => fadeInComplete);

            if (waitForRespawnAnimation && animator != null && !string.IsNullOrEmpty(respawnAnimationState))
            {
                yield return null;
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                while (!stateInfo.IsName(respawnAnimationState))
                {
                    yield return null;
                    stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                }

                yield return new WaitForSeconds(stateInfo.length);
            }

            if (animator != null && !string.IsNullOrEmpty(idleAnimationState))
                animator.Play(idleAnimationState, 0, 0f);

            OnPlayerRespawned?.Invoke();

            if (playerController != null) playerController.enabled = true;

            _isRespawning = false;
        }

        void ExecuteRespawn(GameObject player, Animator animator)
        {
            if (_currentCheckpoint == null) return;

            var playerController = player.GetComponent<Player.PlayerController>();

            if (playerController != null)
                playerController.SetCapturedState(false);

            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
                player.transform.position = _currentCheckpoint.Position;
                player.transform.rotation = _currentCheckpoint.Rotation;
                charController.enabled = true;
            }
            else
            {
                player.transform.position = _currentCheckpoint.Position;
                player.transform.rotation = _currentCheckpoint.Rotation;
            }

            _currentCheckpoint.ActivateCamera();

            if (animator != null && resetAnimatorOnRespawn)
            {
                animator.Rebind();
                animator.Update(0f);

                if (!string.IsNullOrEmpty(capturedAnimationTrigger))
                    animator.ResetTrigger(capturedAnimationTrigger);
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("IsTeleporting");
            }

            if (animator != null && !string.IsNullOrEmpty(respawnAnimationState))
                animator.Play(respawnAnimationState, 0, 0f);
        }

        public void SetFadeDuration(float duration) => fadeDuration = duration;
        public void SetFadeColor(Color color) => fadeColor = color;
    }
}