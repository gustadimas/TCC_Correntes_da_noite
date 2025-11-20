using UnityEngine;
using UnityEngine.InputSystem;

namespace CorrentesDaNoite.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] bool showDebugLogs = false;

        PlayerController _playerController;
        InputSystem_Actions _inputActions;

        void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _inputActions = new InputSystem_Actions();
        }

        void OnEnable()
        {
            _inputActions.Enable();
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Run.performed += OnRunPerformed;
            _inputActions.Player.Run.canceled += OnRunCanceled;
            _inputActions.Player.Crouch.performed += OnCrouchPerformed;
            _inputActions.Player.Jump.performed += OnJumpPerformed;
        }

        void OnDisable()
        {
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Run.performed -= OnRunPerformed;
            _inputActions.Player.Run.canceled -= OnRunCanceled;
            _inputActions.Player.Crouch.performed -= OnCrouchPerformed;
            _inputActions.Player.Jump.performed -= OnJumpPerformed;
            _inputActions.Disable();
        }

        void OnMovePerformed(InputAction.CallbackContext context) => _playerController.OnMove(context);
        void OnMoveCanceled(InputAction.CallbackContext context) => _playerController.OnMove(context);
        void OnRunPerformed(InputAction.CallbackContext context) => _playerController.OnRun(context);
        void OnRunCanceled(InputAction.CallbackContext context) => _playerController.OnRun(context);
        void OnCrouchPerformed(InputAction.CallbackContext context) => _playerController.OnCrouch(context);
        void OnJumpPerformed(InputAction.CallbackContext context) => _playerController.OnJump(context);

        void Update()
        {
            if (!showDebugLogs) return;

            Vector2 moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            if (moveInput.magnitude > 0.1f)
                Debug.Log($"Move Input: {moveInput}");
        }
    }
}
