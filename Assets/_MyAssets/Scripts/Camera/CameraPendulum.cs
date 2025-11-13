using UnityEngine;
using Unity.Cinemachine;

namespace CorrentesDaNoite.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraPendulum : MonoBehaviour
    {
        [Header("Pendulum Settings")]
        [SerializeField] float rotationAmplitude = 2f;
        [SerializeField] float frequency = 1f;

        [Header("Rotation Axis")]
        [SerializeField] bool rotateX = false;
        [SerializeField] bool rotateY = false;
        [SerializeField] bool rotateZ = true;

        [Header("Advanced")]
        [SerializeField] float phaseOffset = 0f;

        CinemachineCamera _virtualCamera;
        Quaternion _initialRotation;
        float _time;

        void Awake()
        {
            _virtualCamera = GetComponent<CinemachineCamera>();
            _initialRotation = transform.rotation;
        }

        void LateUpdate()
        {
            _time += Time.deltaTime * frequency;
            float angle = Mathf.Sin(_time + phaseOffset) * rotationAmplitude;

            Vector3 euler = _initialRotation.eulerAngles;

            if (rotateX) euler.x += angle;
            if (rotateY) euler.y += angle;
            if (rotateZ) euler.z += angle;

            transform.rotation = Quaternion.Euler(euler);
        }

        public void SetAmplitude(float value) => rotationAmplitude = value;
        public void SetFrequency(float value) => frequency = value;

        public void ResetPendulum()
        {
            _time = 0f;
            transform.rotation = _initialRotation;
        }

        public void UpdateInitialRotation()
        {
            _initialRotation = transform.rotation;
            _time = 0f;
        }
    }
}
