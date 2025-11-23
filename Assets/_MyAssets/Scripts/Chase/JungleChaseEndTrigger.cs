using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using CorrentesDaNoite.UI;

namespace CorrentesDaNoite.Chase
{
    [RequireComponent(typeof(Collider))]
    public class JungleChaseEndTrigger : MonoBehaviour
    {
        [Header("Sequencia")]
        [SerializeField] JungleChaseSequenceController sequenceController;
        [SerializeField] Player.PlayerController playerOverride;
        [SerializeField] Animator playerAnimatorOverride;
        [SerializeField] Transform endRunTarget;
        [SerializeField] float endRunSpeed = 6f;
        [SerializeField] float stopDistance = 1f;
        [SerializeField] float maxRunDuration = 5f;
        [SerializeField] bool triggerOnce = true;

        [Header("Camera")]
        [SerializeField] CinemachineCamera endCamera;

        [Header("Fade/Cena")]
        [SerializeField] string nextSceneName = "";
        [SerializeField] float fadeDuration = 1.5f;
        [SerializeField] Color fadeColor = Color.black;

        [Header("Tutorial")]
        [SerializeField] TutorialPromptUI tutorialUI;
        [SerializeField] string runPrompt = "Corra!";
        [SerializeField] float promptDelay = 0.1f;

        [Header("Enemy Handling")]
        [SerializeField] bool stopEnemyChase = true;
        [SerializeField] bool disableEnemyOnEnd = true;

        [Header("Animacao")]
        [SerializeField] string runStateName = "Run";
        [SerializeField] string runBoolName = "IsRunning";
        [SerializeField] string speedFloatName = "Speed";

        bool _triggered;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered && triggerOnce) return;
            if (!other.CompareTag("Player")) return;
            if (endRunTarget == null)
            {
                Debug.LogWarning("[JungleChaseEndTrigger] endRunTarget nao definido.");
                return;
            }

            _triggered = true;
            StartCoroutine(EndSequenceRoutine());
        }

        IEnumerator EndSequenceRoutine()
        {
            var controller = sequenceController ?? FindFirstObjectByType<JungleChaseSequenceController>();
            if (controller == null)
            {
                Debug.LogWarning("[JungleChaseEndTrigger] JungleChaseSequenceController nao encontrado.");
                yield break;
            }

            var playerCtrl = playerOverride != null ? playerOverride : controller.PlayerController;
            if (playerCtrl == null)
            {
                Debug.LogWarning("[JungleChaseEndTrigger] PlayerController nao encontrado.");
                yield break;
            }

            var characterController = playerCtrl.GetComponent<CharacterController>();
            var anim = playerAnimatorOverride != null ? playerAnimatorOverride : (playerCtrl.GetComponent<Animator>() ?? controller.PlayerAnimator);
            var playerInput = playerCtrl.GetComponent<UnityEngine.InputSystem.PlayerInput>();

            if (anim != null)
            {
                SetRunAnimation(anim, 1f, true);
                if (IsValidState(anim, runStateName))
                    anim.Play(runStateName, 0, 0f);
            }

            controller.InputMediator?.DisableAllInputs();
            if (playerInput != null) playerInput.enabled = false;
            playerCtrl.StopMovementImmediate();
            playerCtrl.SetInputMultiplier(Vector2.zero);

            if (endCamera != null)
                controller.SetActiveCamera(endCamera);

            HandleEnemy(controller);

            if (tutorialUI == null)
                tutorialUI = FindFirstObjectByType<TutorialPromptUI>();

            if (tutorialUI != null && !string.IsNullOrEmpty(runPrompt))
                StartCoroutine(ShowPromptDelayed());

            float elapsed = 0f;
            while (elapsed < maxRunDuration)
            {
                Vector3 dir = endRunTarget.position - playerCtrl.transform.position;
                dir.y = 0f;
                float dist = dir.magnitude;
                if (dist <= stopDistance)
                    break;

                if (dist > 0.001f)
                {
                    dir /= dist;
                    float dt = Time.unscaledDeltaTime;

                    if (characterController != null)
                        characterController.Move(dir * endRunSpeed * dt);
                    else
                        playerCtrl.transform.position += dir * endRunSpeed * dt;

                    playerCtrl.transform.rotation = Quaternion.Slerp(playerCtrl.transform.rotation, Quaternion.LookRotation(dir), dt * 6f);

                    if (anim != null)
                        SetRunAnimation(anim, 1f, true);
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            FadeAndLoadScene();
        }

        IEnumerator ShowPromptDelayed()
        {
            if (promptDelay > 0f)
                yield return new WaitForSecondsRealtime(promptDelay);

            tutorialUI?.ShowPrompt(runPrompt);
        }

        void HandleEnemy(JungleChaseSequenceController controller)
        {
            var enemyController = controller.ChaseEnemyController;
            var enemyObject = controller.ChaseEnemy;

            if (stopEnemyChase)
                enemyController?.StopChase();

            if (disableEnemyOnEnd && enemyObject != null)
                enemyObject.SetActive(false);
        }

        bool IsValidState(Animator animator, string stateName)
        {
            if (animator == null || string.IsNullOrEmpty(stateName))
                return false;

            if (animator.HasState(0, Animator.StringToHash(stateName)))
                return true;

            Debug.LogWarning($"[JungleChaseEndTrigger] Estado '{stateName}' nao encontrado no Animator {animator.name}. Ajuste o nome em runStateName.");
            return false;
        }

        void SetRunAnimation(Animator anim, float speedValue, bool isRunningValue)
        {
            if (!string.IsNullOrEmpty(runBoolName))
                anim.SetBool(runBoolName, isRunningValue);

            if (!string.IsNullOrEmpty(speedFloatName))
                anim.SetFloat(speedFloatName, speedValue);
        }

        void FadeAndLoadScene()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("[JungleChaseEndTrigger] nextSceneName nao definido; mantendo cena atual.");
                return;
            }

            if (FadeController.Instance == null)
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            FadeController.Instance.SetFadeColor(fadeColor);
            FadeController.Instance.FadeOut(() => SceneManager.LoadScene(nextSceneName), fadeDuration);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (endRunTarget == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, endRunTarget.position);
            Gizmos.DrawWireSphere(endRunTarget.position, stopDistance);
        }
#endif
    }
}