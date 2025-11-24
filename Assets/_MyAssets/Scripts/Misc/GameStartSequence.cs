using UnityEngine;
using System.Collections;
using CorrentesDaNoite.UI;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite
{
    public class GameStartSequence : MonoBehaviour
    {
        [SerializeField] GameObject player;
        [SerializeField] Animator playerAnimator;
        [SerializeField] string wakeUpAnimationState = "WakeUp";
        [SerializeField] string idleAnimationState = "Idle";
        [SerializeField] float fadeDuration = 2f;
        [SerializeField] float delayBeforeFadeIn = 0.5f;
        [SerializeField] bool waitForAnimationToComplete = true;
        [SerializeField] Unity.Cinemachine.CinemachineCamera startCamera;
        [SerializeField] Unity.Cinemachine.CinemachineCamera gameplayCamera;
        [SerializeField] int gameplayCameraPriority = 15;
        [Header("Tutorial")]
        [SerializeField] TutorialPromptUI tutorialUI;
        [SerializeField] bool showMovementTutorialAfterSequence = true;
        [SerializeField] string movementPromptText = "Use WASD para se mover";
        [SerializeField] float movementPromptDelay = 0.3f;
        [Header("Lifecycle")]
        [SerializeField] bool disableAfterSequence = true;
        [SerializeField] bool deactivateObjectAfterSequence = true;
        [Header("Checkpoint Integration")]
        [SerializeField] bool hideAfterFirstCheckpoint = true;

        PlayerController _playerController;
        bool _checkpointReached;

        void Start()
        {
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                _playerController = player.GetComponent<PlayerController>();
                if (playerAnimator == null) playerAnimator = player.GetComponent<Animator>();
            }

            SubscribeCheckpoint();
            StartCoroutine(StartSequence());
        }

        void OnDestroy()
        {
            UnsubscribeCheckpoint();
        }

        IEnumerator StartSequence()
        {
            if (_playerController != null) _playerController.enabled = false;
            if (startCamera != null) startCamera.Priority = 20;
            if (gameplayCamera != null) gameplayCamera.Priority = 0;

            if (FadeController.Instance == null)
            {
                PlayWakeUpAnimation();
                if (_playerController != null) _playerController.enabled = true;
                yield break;
            }

            FadeController.Instance.SetFadeColor(Color.black);
            FadeController.Instance.SetAlpha(1f);
            yield return new WaitForSeconds(delayBeforeFadeIn);

            PlayWakeUpAnimation();

            bool fadeComplete = false;
            FadeController.Instance.FadeIn(() => fadeComplete = true, fadeDuration);
            yield return new WaitUntil(() => fadeComplete);

            if (waitForAnimationToComplete && playerAnimator != null && !string.IsNullOrEmpty(wakeUpAnimationState))
            {
                AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
                float waitTime = Mathf.Max(0f, stateInfo.length - stateInfo.normalizedTime * stateInfo.length);
                yield return new WaitForSeconds(waitTime);
            }

            if (playerAnimator != null && !string.IsNullOrEmpty(idleAnimationState))
                playerAnimator.Play(idleAnimationState, 0, 0f);

            if (startCamera != null) startCamera.Priority = 0;

            if (gameplayCamera != null)
            {
                gameplayCamera.enabled = false;
                gameplayCamera.enabled = true;
                gameplayCamera.Priority = gameplayCameraPriority;
            }

            if (_playerController != null) _playerController.enabled = true;

            TryShowMovementTutorial();

            if (deactivateObjectAfterSequence)
            {
                gameObject.SetActive(false);
            }
            else if (disableAfterSequence)
            {
                enabled = false;
            }
        }

        void SubscribeCheckpoint()
        {
            if (!hideAfterFirstCheckpoint)
                return;

            Checkpoint.CheckpointManager.OnPlayerRespawned += OnCheckpointEvent;
        }

        void UnsubscribeCheckpoint()
        {
            if (!hideAfterFirstCheckpoint)
                return;

            Checkpoint.CheckpointManager.OnPlayerRespawned -= OnCheckpointEvent;
        }

        void OnCheckpointEvent()
        {
            if (_checkpointReached)
                return;

            _checkpointReached = true;
            gameObject.SetActive(false);
        }

        void PlayWakeUpAnimation()
        {
            if (playerAnimator != null && !string.IsNullOrEmpty(wakeUpAnimationState))
                playerAnimator.Play(wakeUpAnimationState, 0, 0f);
        }

        IEnumerator DelayedMovementTutorial()
        {
            if (movementPromptDelay > 0f)
                yield return new WaitForSeconds(movementPromptDelay);

            tutorialUI?.ShowPrompt(movementPromptText);
        }

        void TryShowMovementTutorial()
        {
            if (!showMovementTutorialAfterSequence)
                return;

            if (tutorialUI == null)
                tutorialUI = FindFirstObjectByType<TutorialPromptUI>();

            if (tutorialUI != null)
                StartCoroutine(DelayedMovementTutorial());
        }
    }
}
