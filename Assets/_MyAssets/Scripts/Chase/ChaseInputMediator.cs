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
        Player.PlayerInputHandler _inputHandler;
        float _autoRunEndTime;

        // depois de liberar continua correndo sozinha esse tempo ou ate o jogador apertar algo
        const float RELEASE_RUN_TIME = 1.5f;

        public bool InputEnabled => inputEnabled;
        public bool IsAutoRunning => isAutoRunning;

        void Awake()
        {
            playerInput ??= GetComponent<PlayerInput>();
            playerController ??= GetComponent<Player.PlayerController>();
            _inputHandler = GetComponent<Player.PlayerInputHandler>();
        }

        void Update()
        {
            if (forceForwardMovement && isAutoRunning && playerController != null)
            {
                playerController.SetExternalMovement(Vector2.up, true);
            }

            if (isAutoRunning && inputEnabled && (Time.time >= _autoRunEndTime || (stopAutoRunOnPlayerInput && PlayerProvidedInput()))) ClearAutoRun();
        }

        public void EnableAllInputs()
        {
            inputEnabled = true;
            ResetAutoFlags();
            SetPlayerInput(true);
            playerController?.ClearExternalMovement();
        }

        public void DisableAllInputs()
        {
            inputEnabled = false;
            ResetAutoFlags();
            SetPlayerInput(false);
            playerController?.ClearExternalMovement();
        }

        public void EnableAutoRun()
        {
            inputEnabled = false;
            forceForwardMovement = true;
            isAutoRunning = true;
            SetPlayerInput(false);
            playerController?.SetExternalMovement(Vector2.up, true);
        }

        public void EnablePlayerControl(bool keepForwardUntilInput = false)
        {
            inputEnabled = true;
            forceForwardMovement = keepForwardUntilInput;
            isAutoRunning = keepForwardUntilInput;
            _autoRunEndTime = Time.time + RELEASE_RUN_TIME;
            SetPlayerInput(true);

            if (keepForwardUntilInput) playerController?.SetExternalMovement(Vector2.up, true);
            else playerController?.ClearExternalMovement();
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            SetPlayerInput(enabled);
            if (!enabled) playerController?.ClearExternalMovement();
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

        // o input do jogo vem do PlayerInputHandler entao desligar so o PlayerInput nao trava nada
        void SetPlayerInput(bool active)
        {
            if (playerInput != null) playerInput.enabled = active;
            if (_inputHandler != null) _inputHandler.enabled = active;
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
