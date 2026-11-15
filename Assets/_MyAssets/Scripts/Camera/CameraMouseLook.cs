using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraMouseLook : CinemachineExtension
    {
        [SerializeField] float horizontalSensitivity = 3f;
        [SerializeField] float verticalSensitivity = 2f;
        [SerializeField] float smoothSpeed = 5f;
        [SerializeField] float maxHorizontalAngle = 15f;
        [SerializeField] float maxVerticalAngle = 10f;
        [SerializeField] bool invertHorizontal = false;
        [SerializeField] bool invertVertical = false;
        [SerializeField] bool enableMouseLook = true;

        float _targetYaw;
        float _targetPitch;
        float _currentYaw;
        float _currentPitch;
        Vector2 _mouseOffset;

        void Update()
        {
            if (!enableMouseLook) return;

            // no controle o analogico direito faz o papel da posicao do mouse
            Vector2 look = !CameraSway.Enabled ? Vector2.zero : ActiveInputDevice.UsingGamepad && Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : GetMouseOffset();
            float normalizedX = look.x;
            float normalizedY = look.y;

            if (invertHorizontal) normalizedX = -normalizedX;
            if (invertVertical) normalizedY = -normalizedY;

            _targetYaw = Mathf.Clamp(normalizedX * horizontalSensitivity, -maxHorizontalAngle, maxHorizontalAngle);
            _targetPitch = Mathf.Clamp(-normalizedY * verticalSensitivity, -maxVerticalAngle, maxVerticalAngle);

            _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw, smoothSpeed * Time.deltaTime);
            _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, smoothSpeed * Time.deltaTime);
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (!enableMouseLook || stage != CinemachineCore.Stage.Finalize) return;
            state.RawOrientation *= Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        public void UpdateBaseRotation()
        {
            _currentYaw = 0f;
            _currentPitch = 0f;
            _targetYaw = 0f;
            _targetPitch = 0f;
        }

        public void SetEnabled(bool enabled) => enableMouseLook = enabled;

        // o cursor fica travado e escondido no jogo entao a posicao vem da soma do movimento do mouse
        Vector2 GetMouseOffset()
        {
            if (Mouse.current == null || Cursor.lockState != CursorLockMode.Locked) return _mouseOffset;
            _mouseOffset += Mouse.current.delta.ReadValue() / new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            _mouseOffset = new Vector2(Mathf.Clamp(_mouseOffset.x, -1f, 1f), Mathf.Clamp(_mouseOffset.y, -1f, 1f));
            return _mouseOffset;
        }
    }
}