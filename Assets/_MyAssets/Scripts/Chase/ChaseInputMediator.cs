using UnityEngine;
using UnityEngine.InputSystem;

namespace CorrentesDaNoite.Chase
{
    public class ChaseInputMediator : MonoBehaviour
    {
        [Header("Input Control")]
        [SerializeField] bool inputEnabled = true;

        [Header("Auto Control")]
        [SerializeField] bool forceForwardMovement;
        [SerializeField] string moveActionName = "Move";
        [SerializeField] bool stopAutoRunOnPlayerInput = true;
        [SerializeField, Tooltip("Magnitude minima do input para assumir controle")] float autoRunInputThreshold = 0.25f;

        [Header("References")]
        [SerializeField] PlayerInput playerInput;
        [SerializeField] Player.PlayerController playerController;

        bool isAutoRunning;

        public bool InputEnabled => inputEnabled;
        public bool IsAutoRunning => isAutoRunning;

        void Awake()
        {
            playerInput ??= GetComponent<PlayerInput>();
            playerController ??= GetComponent<Player.PlayerController>();
        }

        void Update()
        {
            if (forceForwardMovement && isAutoRunning && playerController != null)
            {
                playerController.SetExternalMovement(Vector2.up, true);
            }

            if (stopAutoRunOnPlayerInput && isAutoRunning && inputEnabled)
            {
                if (PlayerProvidedInput())
                    ClearAutoRun();
            }
        }

        public void EnableAllInputs()
        {
            inputEnabled = true;
            ResetAutoFlags();

            if (playerInput != null)
                playerInput.enabled = true;

            playerController?.ClearExternalMovement();
        }

        public void DisableAllInputs()
        {
            inputEnabled = false;
            ResetAutoFlags();

            if (playerInput != null)
                playerInput.enabled = false;

            playerController?.ClearExternalMovement();
        }

        public void EnableAutoRun()
        {
            inputEnabled = false;
            forceForwardMovement = true;
            isAutoRunning = true;

            if (playerInput != null)
                playerInput.enabled = false;

            playerController?.SetExternalMovement(Vector2.up, true);
        }

        public void EnablePlayerControl(bool keepForwardUntilInput = false)
        {
            inputEnabled = true;
            forceForwardMovement = keepForwardUntilInput;
            isAutoRunning = keepForwardUntilInput;

            if (playerInput != null)
                playerInput.enabled = true;

            if (keepForwardUntilInput)
                playerController?.SetExternalMovement(Vector2.up, true);
            else
                playerController?.ClearExternalMovement();
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (playerInput != null)
                playerInput.enabled = enabled;

            if (!enabled)
                playerController?.ClearExternalMovement();
        }

        void ClearAutoRun()
        {
            forceForwardMovement = false;
            isAutoRunning = false;
            playerController?.ClearExternalMovement();
        }

        void ResetAutoFlags()
        {
            forceForwardMovement = false;
            isAutoRunning = false;
        }

        bool PlayerProvidedInput()
        {
            if (playerInput == null || playerInput.actions == null)
                return false;

            var moveAction = playerInput.actions.FindAction(moveActionName);
            if (moveAction == null)
                return false;

            Vector2 value = moveAction.ReadValue<Vector2>();
            return value.sqrMagnitude >= autoRunInputThreshold * autoRunInputThreshold;
        }
    }
}
