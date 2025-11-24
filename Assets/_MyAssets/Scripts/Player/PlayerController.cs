using CorrentesDaNoite.Audio;
using CorrentesDaNoite.Camera;
using CorrentesDaNoite.Chase;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [Header("Suavizacao")]
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
        [SerializeField, Tooltip("Inverte o eixo horizontal ao usar zonas direcionais (corrige cameras voltadas para o player)")]
        bool invertDirectionalHorizontal;

        CharacterController _characterController;
        Animator _animator;
        PlayerSoundEmitter _soundEmitter;
        ChaseLookBackController _lookBackController;
        ChaseStumbleHandler _stumbleHandler;

        Vector2 _movementInput;
        bool _isRunning;
        bool _isCrouching;

        Vector3 _currentVelocity;
        float _currentSpeed;
        float _targetSpeed;
        Vector3 _verticalVelocity;
        bool _isCaptured;
        Vector2 _externalMovementInput;
        bool _hasExternalMovementInput;
        bool _forceRunExternal;
        Vector2 _inputMultiplier = Vector2.one;

        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        static readonly int Jump = Animator.StringToHash("Jump");
        static readonly int VelocityX = Animator.StringToHash("VelocityX");
        static readonly int VelocityZ = Animator.StringToHash("VelocityZ");
        static readonly int Struggling = Animator.StringToHash("Struggling");

        public bool IsCaptured => _isCaptured;
        public float CurrentSpeed => _currentSpeed;
        public bool IsRunning => _isRunning;
        public bool IsCrouching => _isCrouching;
        public bool UseDirectionalZones => useDirectionalZones;
        public bool UseCameraRelativeMovement => useCameraRelativeMovement;

        void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _soundEmitter = GetComponent<PlayerSoundEmitter>();
            _lookBackController = GetComponent<ChaseLookBackController>();
            _stumbleHandler = GetComponent<ChaseStumbleHandler>();

            if (cameraTransform == null)
            {
                var mainCamera = UnityEngine.Camera.main;
                if (mainCamera != null)
                    cameraTransform = mainCamera.transform;
                else
                {
                    Debug.LogWarning("PlayerController: Main Camera nao encontrada!");
                    useCameraRelativeMovement = false;
                }
            }

            MusicManager.GetOrCreate().PlayMusic("Game", 1.5f);
            var asc = FindFirstObjectByType<AudioStateController>();
            asc?.SetExplorationState();
        }

        void Update()
        {
            if (_isCaptured) return;

            ApplyExternalMovementOverride();
            HandleMovement();
            ApplyGravity();
            UpdateAnimator();
        }

        void HandleMovement()
        {
            _targetSpeed = CalculateTargetSpeed() * GetChaseSpeedMultiplier();

            bool hasInput = _movementInput.sqrMagnitude > 0.01f;
            float lerpFactor = (hasInput ? acceleration : deceleration) * Time.deltaTime;
            _currentSpeed = Mathf.Lerp(_currentSpeed, hasInput ? _targetSpeed : 0f, lerpFactor);

            if (hasInput)
            {
                Vector3 moveDirection = GetMoveDirection();
                Vector3 movement = moveDirection * _currentSpeed;
                _characterController.Move(movement * Time.deltaTime);

                if (movement.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = useSnapRotation
                        ? targetRotation
                        : Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
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
            if (useDirectionalZones && CameraDirectionManager.Instance != null)
            {
                return CameraDirectionManager.Instance.ConvertInputToWorldDirection(_movementInput, invertDirectionalHorizontal);
            }

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

            return new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;
        }

        float CalculateTargetSpeed()
        {
            if (_movementInput.magnitude < 0.1f) return 0f;
            if (_isCrouching) return crouchSpeed;
            if (_isRunning) return runSpeed;
            return walkSpeed;
        }

        float GetChaseSpeedMultiplier()
        {
            float multiplier = 1f;

            if (_lookBackController != null)
                multiplier *= _lookBackController.GetCurrentSpeedMultiplier();

            if (_stumbleHandler != null)
                multiplier *= _stumbleHandler.CurrentSpeedMultiplier;

            return multiplier;
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
            _animator.SetBool(IsCrouchingHash, _isCrouching);
            _animator.SetBool(IsRunningHash, _isRunning && !_isCrouching && _movementInput.magnitude > 0.1f);

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

        public void OnMove(InputAction.CallbackContext context)
        {
            if (_isCaptured) return;
            _movementInput = Vector2.Scale(context.ReadValue<Vector2>(), _inputMultiplier);
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (_isCaptured) return;
            if (context.performed && !_isCrouching)
                _isRunning = true;
            else if (context.canceled)
                _isRunning = false;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (_isCaptured) return;

            if (context.performed)
            {
                _isCrouching = !_isCrouching;

                if (_isCrouching)
                    _isRunning = false;

                AdjustCharacterControllerHeight();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (_isCaptured) return;

            if (context.performed && _characterController.isGrounded)
            {
                _animator.SetTrigger(Jump);
                if (enableJumpPhysics)
                    _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                _soundEmitter?.EmitJumpSound();
            }
        }

        public virtual void SetCapturedState(bool captured)
        {
            _isCaptured = captured;
            if (captured)
            {
                ResetMovementState();
                _characterController.enabled = false;
            }
            else
            {
                _characterController.enabled = true;
            }
        }

        public virtual void PlayCaptureAnimation()
        {
            _animator?.SetTrigger(Struggling);
        }

        public void SetExternalMovement(Vector2 input, bool forceRun)
        {
            _externalMovementInput = input;
            _hasExternalMovementInput = true;
            _forceRunExternal = forceRun;
        }

        public void ClearExternalMovement()
        {
            _hasExternalMovementInput = false;
            _forceRunExternal = false;
            _externalMovementInput = Vector2.zero;
        }

        public void SetDirectionalZonesEnabled(bool enabled) => useDirectionalZones = enabled;
        public void SetCameraRelativeMovement(bool enabled) => useCameraRelativeMovement = enabled;
        public void SetInvertDirectionalHorizontal(bool invert) => invertDirectionalHorizontal = invert;
        public void SetInputMultiplier(Vector2 multiplier) => _inputMultiplier = multiplier;

        public void StopMovementImmediate()
        {
            ResetMovementState();
            _verticalVelocity = Vector3.zero;
            ClearExternalMovement();
        }

        void ApplyExternalMovementOverride()
        {
            if (_hasExternalMovementInput)
            {
                _movementInput = _externalMovementInput;
                if (_forceRunExternal)
                    _isRunning = true;
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

        void ResetMovementState()
        {
            _movementInput = Vector2.zero;
            _currentVelocity = Vector3.zero;
            _currentSpeed = 0f;
            _isRunning = false;
            _isCrouching = false;
        }
    }
}
