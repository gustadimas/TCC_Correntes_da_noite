using System.Collections;
using CorrentesDaNoite.UI;
using UnityEngine;

namespace CorrentesDaNoite.Player
{
    public class StoneThrower : MonoBehaviour
    {
        const float PICK_DISTANCE = 1.6f;
        const float GRAB_REACH = 0.45f;
        const float STEP_TIME = 0.25f;
        const float GRAB_TIME = 1.05f;
        const float PICK_TIME = 1.75f;
        const float RELEASE_TIME = 0.63f;
        const float THROW_TIME = 1.75f;
        const float THROW_SPEED = 12f;
        const float THROW_LIFT = 5f;
        const float HOLD_TIME = 20f;

        static readonly int PickUpHash = Animator.StringToHash("PickUp");
        static readonly int ThrowHash = Animator.StringToHash("Throw");

        [SerializeField] ThrownStone _stonePrefab;
        [SerializeField] Transform _grip;

        PlayerController _player;
        Animator _animator;
        CharacterController _body;
        ThrownStone _stone;
        Coroutine _holdTimer;
        bool _busy;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _animator = GetComponent<Animator>();
            _body = GetComponent<CharacterController>();
        }

        public void PickUp()
        {
            if (_busy || _stone != null || !CanAct()) return;
            StonePile pile = NearestPile();
            if (pile != null) StartCoroutine(PickUpRoutine(pile.transform.position));
        }

        public void Throw()
        {
            if (_busy || _stone == null || !CanAct()) return;
            StartCoroutine(ThrowRoutine());
        }

        IEnumerator PickUpRoutine(Vector3 pile)
        {
            _busy = true;
            Vector3 toPile = pile - transform.position;
            toPile.y = 0f;
            if (toPile != Vector3.zero) transform.rotation = Quaternion.LookRotation(toPile);
            _player.SetExternalMovement(Vector2.zero, false);
            _animator.SetTrigger(PickUpHash);

            // da um passinho ate onde a mao alcanca o monte
            Vector3 grabSpot = pile - toPile.normalized * GRAB_REACH;
            for (float timer = 0f; timer < STEP_TIME; timer += Time.deltaTime)
            {
                Vector3 step = grabSpot - transform.position;
                step.y = 0f;
                _body.Move(step * Mathf.Clamp01(Time.deltaTime / (STEP_TIME - timer)));
                yield return null;
            }

            yield return new WaitForSeconds(GRAB_TIME - STEP_TIME);
            _stone = Instantiate(_stonePrefab, _grip.position, _grip.rotation, _grip);
            _holdTimer = StartCoroutine(HoldTimer());
            yield return new WaitForSeconds(PICK_TIME - GRAB_TIME);

            _player.ClearExternalMovement();
            _busy = false;
            TutorialPromptUI.Instance?.ShowPrompt("Aperte {jogar} para jogar a pedra e distrair os guardas");
        }

        // chamado pelo teleporte e quando acaba o tempo de segurar
        public void DropStone()
        {
            if (_holdTimer != null) StopCoroutine(_holdTimer);
            if (_stone != null) Destroy(_stone.gameObject);
            _holdTimer = null;
            _stone = null;
        }

        IEnumerator HoldTimer()
        {
            yield return new WaitForSeconds(HOLD_TIME);
            _holdTimer = null;
            DropStone();
        }

        IEnumerator ThrowRoutine()
        {
            _busy = true;
            if (_holdTimer != null) StopCoroutine(_holdTimer);
            _holdTimer = null;
            _player.SetExternalMovement(Vector2.zero, false);
            _animator.SetTrigger(ThrowHash);

            yield return new WaitForSeconds(RELEASE_TIME);
            if (_stone != null) _stone.Launch(transform.forward * THROW_SPEED + Vector3.up * THROW_LIFT, _body);
            _stone = null;
            yield return new WaitForSeconds(THROW_TIME - RELEASE_TIME);

            _player.ClearExternalMovement();
            _busy = false;
        }

        // o input chega mesmo com o controller desligado no respawn e no teleporte
        bool CanAct() => _player.enabled && !_player.IsCaptured;

        StonePile NearestPile()
        {
            foreach (StonePile pile in StonePile.All)
                if ((pile.transform.position - transform.position).sqrMagnitude < PICK_DISTANCE * PICK_DISTANCE) return pile;
            return null;
        }
    }
}