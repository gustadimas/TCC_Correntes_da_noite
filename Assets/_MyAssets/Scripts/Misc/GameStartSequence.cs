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

        PlayerController _playerController;

        void Start()
        {
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                _playerController = player.GetComponent<PlayerController>();
                if (playerAnimator == null) playerAnimator = player.GetComponent<Animator>();
            }

            StartCoroutine(StartSequence());
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
        }

        void PlayWakeUpAnimation()
        {
            if (playerAnimator != null && !string.IsNullOrEmpty(wakeUpAnimationState))
                playerAnimator.Play(wakeUpAnimationState, 0, 0f);
        }
    }
}