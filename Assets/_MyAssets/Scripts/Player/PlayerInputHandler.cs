using UnityEngine;
using UnityEngine.InputSystem;

namespace CorrentesDaNoite.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInputHandler : MonoBehaviour
    {
        PlayerController _playerController;
        InputSystem_Actions _inputActions;

        void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _inputActions = new InputSystem_Actions();
        }

        void OnEnable()
        {
            _inputActions.Player.Enable();
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Run.performed += OnRunPerformed;
            _inputActions.Player.Run.canceled += OnRunCanceled;
            _inputActions.Player.Crouch.performed += OnCrouchPerformed;
            _inputActions.Player.Jump.performed += OnJumpPerformed;
        }

        void OnDisable()
        {
            // desliga antes de tirar os eventos pra soltar o que estava apertado
            _inputActions.Player.Disable();
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Run.performed -= OnRunPerformed;
            _inputActions.Player.Run.canceled -= OnRunCanceled;
            _inputActions.Player.Crouch.performed -= OnCrouchPerformed;
            _inputActions.Player.Jump.performed -= OnJumpPerformed;
        }

        void OnMovePerformed(InputAction.CallbackContext context) => _playerController.OnMove(context);
        void OnMoveCanceled(InputAction.CallbackContext context) => _playerController.OnMove(context);
        void OnRunPerformed(InputAction.CallbackContext context) => _playerController.OnRun(context);
        void OnRunCanceled(InputAction.CallbackContext context) => _playerController.OnRun(context);
        void OnCrouchPerformed(InputAction.CallbackContext context) => _playerController.OnCrouch(context);
        void OnJumpPerformed(InputAction.CallbackContext context) => _playerController.OnJump(context);
    }
}
