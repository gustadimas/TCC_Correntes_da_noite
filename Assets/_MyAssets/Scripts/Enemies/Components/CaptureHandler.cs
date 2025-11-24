using UnityEngine;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite.Enemies
{
    public class CaptureHandler : MonoBehaviour
    {
        [Header("Capture Movement")]
        [SerializeField] protected float moveToHoldPointSpeed = 3f;
        [SerializeField] protected float rotationSpeed = 8f;
        [SerializeField, Tooltip("Se verdadeiro, mantém o player como filho do hold point durante a captura para evitar deslizamentos.")]
        protected bool parentCapturedPlayer = true;

        protected PlayerController _capturedPlayer;
        protected bool _isHoldingPlayer;
        protected bool _isMovingToHoldPoint;
        protected Vector3 _captureStartPosition;
        protected float _captureProgress;
        protected Transform _originalParent;

        public bool IsHoldingPlayer => _isHoldingPlayer;
        public PlayerController CapturedPlayer => _capturedPlayer;
        public bool IsMovingToHoldPoint => _isMovingToHoldPoint;

        public virtual void CapturePlayer(PlayerController player, Transform holdPoint)
        {
            if (player == null || holdPoint == null) return;
            if (player.IsCaptured) return;

            _capturedPlayer = player;
            _isHoldingPlayer = true;
            _isMovingToHoldPoint = true;
            _captureProgress = 0f;
            _captureStartPosition = player.transform.position;
            _originalParent = player.transform.parent;

            player.SetCapturedState(true);
            player.PlayCaptureAnimation();

            if (parentCapturedPlayer)
            {
                player.transform.SetParent(holdPoint, worldPositionStays: true);
            }

            var cc = player.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
        }

        public virtual void ReleasePlayer(bool detachFromHoldPoint = true, bool restoreController = true, bool clearCapturedState = true)
        {
            if (_capturedPlayer == null) return;
            if (clearCapturedState)
                _capturedPlayer.SetCapturedState(false);

            if (parentCapturedPlayer && detachFromHoldPoint && _capturedPlayer.transform != null)
                _capturedPlayer.transform.SetParent(_originalParent, worldPositionStays: true);

            var cc = _capturedPlayer != null ? _capturedPlayer.GetComponent<CharacterController>() : null;
            if (restoreController && cc != null)
                cc.enabled = true;

            _capturedPlayer = null;
            _isHoldingPlayer = false;
            _isMovingToHoldPoint = false;
            _captureProgress = 0f;
            _originalParent = null;
        }

        public virtual void UpdatePlayerPosition(Transform holdPoint)
        {
            if (!_isHoldingPlayer || _capturedPlayer == null || holdPoint == null) return;

            if (_isMovingToHoldPoint)
            {
                _captureProgress += Time.deltaTime * moveToHoldPointSpeed;
                _captureProgress = Mathf.Clamp01(_captureProgress);

                _capturedPlayer.transform.position = Vector3.Lerp(_captureStartPosition, holdPoint.position, _captureProgress);
                _capturedPlayer.transform.rotation = Quaternion.Slerp(
                    _capturedPlayer.transform.rotation,
                    holdPoint.rotation,
                    rotationSpeed * Time.deltaTime
                );

                if (_captureProgress >= 1f)
                    _isMovingToHoldPoint = false;
            }
            else
            {
                if (parentCapturedPlayer)
                {
                    _capturedPlayer.transform.localPosition = Vector3.zero;
                    _capturedPlayer.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    _capturedPlayer.transform.position = holdPoint.position;
                    _capturedPlayer.transform.rotation = holdPoint.rotation;
                }
            }
        }
    }
}