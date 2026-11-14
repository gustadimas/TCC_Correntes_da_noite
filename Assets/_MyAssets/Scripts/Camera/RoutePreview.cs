using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using CorrentesDaNoite.Player;
using CorrentesDaNoite.UI;

namespace CorrentesDaNoite.Camera
{
    public class RoutePreview : MonoBehaviour
    {
        [SerializeField] CinemachineCamera _camera;
        [SerializeField] Transform _goal;
        [SerializeField] float _pitch = 30f;
        [SerializeField] float _distance = 24f;

        Vector3[] _route;
        bool _played;

        const float FLY_SPEED = 32f;
        const float DAMPING = 2.5f;
        const float LOOK_HEIGHT = 1.5f;
        const float GOAL_HOLD = 1.5f;
        const float FADE_TIME = 0.5f;

        private void Awake() => _camera.enabled = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_played || !other.TryGetComponent(out PlayerController player) || !TryBuildRoute(player.transform.position)) return;
            _played = true;
            StartCoroutine(Play(player));
        }

        IEnumerator Play(PlayerController player)
        {
            player.StopMovementImmediate();
            player.enabled = false;
            if (player.TryGetComponent(out Animator animator)) { animator.SetFloat("Speed", 0f); animator.SetBool("IsRunning", false); }

            // sai de onde a camera do jogo esta e anda de lado pela rota virada pro mesmo lado
            var brain = CinemachineCore.FindPotentialTargetBrain(_camera);
            Transform view = brain.OutputCamera.transform;
            var rotation = Quaternion.Euler(_pitch, view.eulerAngles.y, 0f);
            Vector3 offset = Vector3.up * LOOK_HEIGHT + rotation * Vector3.back * _distance;
            _camera.transform.SetPositionAndRotation(view.position, rotation);
            _camera.enabled = true;

            float end = GetLength();
            for (float traveled = 0f; traveled < end && !SkipPressed(); traveled += FLY_SPEED * Time.deltaTime)
            {
                MoveCamera(GetPoint(traveled) + offset);
                yield return null;
            }

            for (float timer = 0f; timer < GOAL_HOLD && !SkipPressed(); timer += Time.deltaTime)
            {
                MoveCamera(_route[^1] + offset);
                yield return null;
            }

            yield return CutBack(brain);
            player.enabled = true;
        }

        bool TryBuildRoute(Vector3 start)
        {
            if (_goal == null || !NavMesh.SamplePosition(start, out var startHit, 3f, NavMesh.AllAreas) || !NavMesh.SamplePosition(_goal.position, out var goalHit, 10f, NavMesh.AllAreas)) return false;

            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(startHit.position, goalHit.position, NavMesh.AllAreas, path) || path.corners.Length < 2) return false;

            _route = path.corners;
            _route[^1] = _goal.position;
            return true;
        }

        void MoveCamera(Vector3 target) => _camera.transform.position = Vector3.Lerp(_camera.transform.position, target, 1f - Mathf.Exp(-DAMPING * Time.deltaTime));

        Vector3 GetPoint(float distance)
        {
            for (int i = 1; i < _route.Length; i++)
            {
                float segment = Vector3.Distance(_route[i - 1], _route[i]);
                if (distance <= segment && segment > 0f) return Vector3.Lerp(_route[i - 1], _route[i], distance / segment);
                distance -= segment;
            }

            return _route[^1];
        }

        float GetLength()
        {
            float length = 0f;
            for (int i = 1; i < _route.Length; i++) length += Vector3.Distance(_route[i - 1], _route[i]);
            return length;
        }

        // volta com fade porque o blend atravessaria o mapa inteiro ate ela
        IEnumerator CutBack(CinemachineBrain brain)
        {
            var fade = FadeController.Instance;
            bool done = fade == null;
            if (fade != null) fade.FadeOut(() => done = true, FADE_TIME);
            yield return new WaitUntil(() => done);

            _camera.enabled = false;
            brain.ResetState();
            if (fade == null) yield break;

            done = false;
            fade.FadeIn(() => done = true, FADE_TIME);
            yield return new WaitUntil(() => done);
        }

        // no pause o espaco e dos botoes do menu
        static bool SkipPressed() => Time.timeScale > 0f && ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame));
    }
}