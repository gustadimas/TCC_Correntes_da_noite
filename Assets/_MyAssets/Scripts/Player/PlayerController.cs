using UnityEngine;
using UnityEngine.InputSystem;
using CorrentesDaNoite.Camera;

namespace CorrentesDaNoite.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Velocidades")]
        [SerializeField] float walkSpeed = 3f;
        [SerializeField] float runSpeed = 6f;
        [SerializeField] float crouchSpeed = 1.5f;

        [Header("Suavização")]
        [SerializeField] float acceleration = 10f;
        [SerializeField] float deceleration = 15f;
        [SerializeField] float rotationSpeed = 12f;
        [SerializeField] bool useSnapRotation = true;

        [Header("Gravidade")]
        [SerializeField] float gravity = -9.81f;

        [Header("Jump")]
        [SerializeField] float jumpHeight = 1.5f;
        [SerializeField] bool enableJumpPhysics = false;

        [Header("Camera")]
        [SerializeField] Transform cameraTransform;
        [SerializeField] bool useCameraRelativeMovement = true;
        [SerializeField] bool useDirectionalZones = true;

        CharacterController _characterController;
        Animator _animator;

        Vector2 _movementInput;
        bool _isRunning;
        bool _isCrouching;

        Vector3 _currentVelocity;
        float _currentSpeed;
        float _targetSpeed;
        Vector3 _verticalVelocity;

        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int IsRunning = Animator.StringToHash("IsRunning");
        static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
        static readonly int Jump = Animator.StringToHash("Jump");
        static readonly int VelocityX = Animator.StringToHash("VelocityX");
        static readonly int VelocityZ = Animator.StringToHash("VelocityZ");

        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();

            if (cameraTransform == null)
            {
                UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                    cameraTransform = mainCamera.transform;
                else
                {
                    Debug.LogWarning("PlayerController: Main Camera não encontrada!");
                    useCameraRelativeMovement = false;
                }
            }
        }

        void Update()
        {
            HandleMovement();
            ApplyGravity();
            UpdateAnimator();
        }

        void HandleMovement()
        {
            _targetSpeed = CalculateTargetSpeed();

            if (_movementInput.magnitude > 0.1f)
                _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, acceleration * Time.deltaTime);
            else
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, deceleration * Time.deltaTime);

            if (_movementInput.magnitude > 0.1f)
            {
                Vector3 moveDirection = GetMoveDirection();
                Vector3 movement = moveDirection * _currentSpeed;
                _characterController.Move(movement * Time.deltaTime);

                if (movement.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = useSnapRotation
                        ? targetRotation
                        : Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                _currentVelocity = movement;
            }
            else
            {
                _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
            }
        }

        Vector3 GetMoveDirection()
        {
            // Prioriza o sistema de zonas direcionais se ativo e disponível
            if (useDirectionalZones && CameraDirectionManager.Instance != null)
            {
                return CameraDirectionManager.Instance.ConvertInputToWorldDirection(_movementInput);
            }

            // Fallback para movimento relativo à câmera tradicional
            if (useCameraRelativeMovement && cameraTransform != null)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();
                return (cameraRight * _movementInput.x + cameraForward * _movementInput.y).normalized;
            }

            // Fallback para movimento absoluto
            return new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;
        }

        float CalculateTargetSpeed()
        {
            if (_movementInput.magnitude < 0.1f) return 0f;
            if (_isCrouching) return crouchSpeed;
            if (_isRunning) return runSpeed;
            return walkSpeed;
        }

        void ApplyGravity()
        {
            if (_characterController.isGrounded && _verticalVelocity.y < 0)
                _verticalVelocity.y = -2f;
            else
                _verticalVelocity.y += gravity * Time.deltaTime;

            _characterController.Move(_verticalVelocity * Time.deltaTime);
        }

        void UpdateAnimator()
        {
            _animator.SetFloat(Speed, _currentSpeed / runSpeed);
            _animator.SetBool(IsRunning, _isRunning && _movementInput.magnitude > 0.1f);
            _animator.SetBool(IsCrouching, _isCrouching);

            Vector3 localVelocity = transform.InverseTransformDirection(_currentVelocity);
            _animator.SetFloat(VelocityX, localVelocity.x);
            _animator.SetFloat(VelocityZ, localVelocity.z);
        }

        void AdjustCharacterControllerHeight()
        {
            if (_isCrouching)
            {
                _characterController.height = 1.0f;
                _characterController.center = new Vector3(0, 0.5f, 0);
            }
            else
            {
                _characterController.height = 2.0f;
                _characterController.center = new Vector3(0, 1.0f, 0);
            }
        }

        public void OnMove(InputAction.CallbackContext context) => _movementInput = context.ReadValue<Vector2>();

        public void OnRun(InputAction.CallbackContext context)
        {
            if (context.performed) _isRunning = true;
            else if (context.canceled) _isRunning = false;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _isCrouching = !_isCrouching;
                AdjustCharacterControllerHeight();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && _characterController.isGrounded)
            {
                _animator.SetTrigger(Jump);
                if (enableJumpPhysics)
                    _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, _currentVelocity);
            }
        }
    }
}