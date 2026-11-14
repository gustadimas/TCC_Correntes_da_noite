using TMPro;
using UnityEngine;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyAlertIcon : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;

        EnemyController _enemy;
        EnemyVisionDetector _vision;
        Transform _head;

        const float HEAD_OFFSET = 1f;
        const string SPOTTED = "<color=#E53935>!</color>";
        const string NOTICING = "<color=#FFC107>?</color>";

        private void Awake()
        {
            _enemy = GetComponentInParent<EnemyController>();
            _vision = _enemy != null ? _enemy.GetComponent<EnemyVisionDetector>() : null;
            var animator = _enemy != null ? _enemy.GetComponentInChildren<Animator>() : null;
            _head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
        }

        private void LateUpdate()
        {
            if (_enemy == null) return;

            _text.text = GetIcon();
            if (_text.text.Length == 0) return;

            // segue a cabeca porque sentado ou deitado ela fica bem mais baixa que o root
            if (_head != null) transform.position = _head.position + Vector3.up * HEAD_OFFSET;
            if (UnityEngine.Camera.main != null) transform.rotation = UnityEngine.Camera.main.transform.rotation;
        }

        string GetIcon()
        {
            var state = _enemy.StateMachine?.CurrentState;
            if (state is EnemySpottedState or EnemyChaseState or EnemyCaptureState or SleepingEnemyStandingUpState or SleepingEnemyIdleReadyState) return SPOTTED;
            if (state is SleepingEnemyStartledState || _enemy.IsAlertedBySound || _enemy.IsRotatingToSound || (_vision != null && _vision.IsNoticingPlayer)) return NOTICING;
            return "";
        }
    }
}