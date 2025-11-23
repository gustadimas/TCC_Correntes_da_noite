using UnityEngine;
using CorrentesDaNoite.UI;

namespace CorrentesDaNoite.Tutorial
{
    [RequireComponent(typeof(Collider))]
    public class TutorialPromptTrigger : MonoBehaviour
    {
        [TextArea]
        [SerializeField] string message = "Mensagem do tutorial";
        [SerializeField] float displayTime = 3f;
        [SerializeField] bool triggerOnce = true;
        [SerializeField] bool hideOnExit;
        [SerializeField] Color gizmoColor = new Color(0f, 0.8f, 1f, 0.35f);

        bool _wasTriggered;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_wasTriggered && triggerOnce)
            {
                return;
            }

            if (!other.CompareTag("Player"))
            {
                return;
            }

            ShowPrompt();

            if (triggerOnce)
            {
                _wasTriggered = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!hideOnExit || !other.CompareTag("Player"))
            {
                return;
            }

            if (TutorialPromptUI.Instance != null)
            {
                TutorialPromptUI.Instance.HideImmediate();
            }
        }

        void ShowPrompt()
        {
            if (TutorialPromptUI.Instance == null)
            {
                Debug.LogWarning("[TutorialPromptTrigger] TutorialPromptUI nao encontrado na cena.");
                return;
            }

            TutorialPromptUI.Instance.ShowPrompt(message, displayTime);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            if (box == null)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawCube(transform.position, Vector3.one);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
                return;
            }

            var previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(box.center, box.size);

            Gizmos.matrix = previousMatrix;
        }
    }
}