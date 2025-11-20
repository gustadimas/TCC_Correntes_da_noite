using UnityEngine;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite.Enemies
{
    public class CaptureHandler : MonoBehaviour
    {
        [Header("Capture Movement")]
        [SerializeField] protected float moveToHoldPointSpeed = 3f;
        [SerializeField] protected float rotationSpeed = 8f;

        protected PlayerController _capturedPlayer;
        protected bool _isHoldingPlayer;
        protected bool _isMovingToHoldPoint;
        protected Vector3 _captureStartPosition;
        protected float _captureProgress;

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

            player.SetCapturedState(true);
            player.PlayCaptureAnimation();
        }

        public virtual void ReleasePlayer()
        {
            if (_capturedPlayer == null) return;
            _capturedPlayer.SetCapturedState(false);
            _capturedPlayer = null;
            _isHoldingPlayer = false;
            _isMovingToHoldPoint = false;
            _captureProgress = 0f;
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
                _capturedPlayer.transform.position = holdPoint.position;
                _capturedPlayer.transform.rotation = holdPoint.rotation;
            }
        }
    }
}