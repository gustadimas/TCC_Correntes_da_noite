using UnityEngine;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraShake : MonoBehaviour
    {
        [Header("Shake Presets")]
        [SerializeField] float detectionIntensity = 2f;
        [SerializeField] float detectionDuration = 0.5f;
        [SerializeField] float lightShakeIntensity = 0.5f;
        [SerializeField] float mediumShakeIntensity = 1.5f;
        [SerializeField] float heavyShakeIntensity = 3f;

        CinemachineCamera _virtualCamera;
        CinemachineBasicMultiChannelPerlin _noise;
        float _shakeTimer;
        float _initialAmplitude;

        void Awake()
        {
            _virtualCamera = GetComponent<CinemachineCamera>();
            _noise = _virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;

            if (_noise == null)
                Debug.LogWarning("CameraShake: Noise não encontrado!");
            else
                _initialAmplitude = _noise.AmplitudeGain;
        }

        void Update()
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;
                if (_shakeTimer <= 0f) StopShake();
            }
        }

        public void ShakeCamera(float intensity, float duration)
        {
            if (_noise == null) return;
            _noise.AmplitudeGain = intensity;
            _shakeTimer = duration;
        }

        public void ShakeOnDetection() => ShakeCamera(detectionIntensity, detectionDuration);
        public void LightShake(float duration = 0.3f) => ShakeCamera(lightShakeIntensity, duration);
        public void MediumShake(float duration = 0.5f) => ShakeCamera(mediumShakeIntensity, duration);
        public void HeavyShake(float duration = 1f) => ShakeCamera(heavyShakeIntensity, duration);

        public void StopShake()
        {
            if (_noise == null) return;
            _noise.AmplitudeGain = _initialAmplitude;
            _shakeTimer = 0f;
        }
    }
}
