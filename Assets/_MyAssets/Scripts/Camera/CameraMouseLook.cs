using UnityEngine;
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

        void Update()
        {
            if (!enableMouseLook) return;

            Vector2 mousePosition = Input.mousePosition;
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            float normalizedX = Mathf.Clamp((mousePosition.x - screenCenter.x) / screenCenter.x, -1f, 1f);
            float normalizedY = Mathf.Clamp((mousePosition.y - screenCenter.y) / screenCenter.y, -1f, 1f);

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
    }
}