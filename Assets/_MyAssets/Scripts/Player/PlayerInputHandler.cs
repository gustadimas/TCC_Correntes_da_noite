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
            _inputActions.Player.Move.performed += ctx => _playerController.OnMove(ctx);
            _inputActions.Player.Move.canceled += ctx => _playerController.OnMove(ctx);
            _inputActions.Player.Run.performed += ctx => _playerController.OnRun(ctx);
            _inputActions.Player.Run.canceled += ctx => _playerController.OnRun(ctx);
            _inputActions.Player.Crouch.performed += ctx => _playerController.OnCrouch(ctx);
            _inputActions.Player.Jump.performed += ctx => _playerController.OnJump(ctx);
        }

        void OnDisable()
        {
            _inputActions.Player.Move.performed -= ctx => _playerController.OnMove(ctx);
            _inputActions.Player.Move.canceled -= ctx => _playerController.OnMove(ctx);
            _inputActions.Player.Run.performed -= ctx => _playerController.OnRun(ctx);
            _inputActions.Player.Run.canceled -= ctx => _playerController.OnRun(ctx);
            _inputActions.Player.Crouch.performed -= ctx => _playerController.OnCrouch(ctx);
            _inputActions.Player.Jump.performed -= ctx => _playerController.OnJump(ctx);
            _inputActions.Disable();
        }

        void Update()
        {
            if (!showDebugLogs) return;

            Vector2 moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            if (moveInput.magnitude > 0.1f)
                Debug.Log($"Move Input: {moveInput}");
        }
    }
}