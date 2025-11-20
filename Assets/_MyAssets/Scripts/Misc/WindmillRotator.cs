using UnityEngine;

namespace CorrentesDaNoite
{
    public class WindmillRotator : MonoBehaviour
    {
        [SerializeField] Vector3 rotationAxis = Vector3.forward;
        [SerializeField] float rotationSpeed = 35f;
        [SerializeField] bool clockwise = false;

        void Update()
        {
            float direction = clockwise ? -1f : 1f;
            transform.Rotate(rotationAxis * rotationSpeed * direction * Time.deltaTime, Space.Self);
        }

        public void SetSpeed(float speed) => rotationSpeed = speed;
        public void SetClockwise(bool isClockwise) => clockwise = isClockwise;
        public void ToggleDirection() => clockwise = !clockwise;
    }
}