using UnityEngine;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite.Enemies
{
    public class CaptureHandler : MonoBehaviour
    {
        [Header("Capture Movement")]
        [SerializeField] protected float moveToHoldPointSpeed = 3f;
        [SerializeField] protected float rotationSpeed = 8f;

        protected PlayerController capturedPlayer;
        protected bool isHoldingPlayer;
        protected bool isMovingToHoldPoint;
        protected Vector3 captureStartPosition;
        protected float captureProgress;

        public bool IsHoldingPlayer => isHoldingPlayer;
        public PlayerController CapturedPlayer => capturedPlayer;
        public bool IsMovingToHoldPoint => isMovingToHoldPoint;

        public virtual void CapturePlayer(PlayerController player, Transform holdPoint)
        {
            if (player == null || holdPoint == null) return;
            if (player.IsCaptured) return;

            capturedPlayer = player;
            isHoldingPlayer = true;
            isMovingToHoldPoint = true;
            captureProgress = 0f;
            captureStartPosition = player.transform.position;

            player.SetCapturedState(true);
            player.PlayCaptureAnimation();
        }

        public virtual void ReleasePlayer()
        {
            if (capturedPlayer == null) return;
            capturedPlayer.SetCapturedState(false);
            capturedPlayer = null;
            isHoldingPlayer = false;
            isMovingToHoldPoint = false;
            captureProgress = 0f;
        }

        public virtual void UpdatePlayerPosition(Transform holdPoint)
        {
            if (!isHoldingPlayer || capturedPlayer == null || holdPoint == null) return;

            if (isMovingToHoldPoint)
            {
                captureProgress += Time.deltaTime * moveToHoldPointSpeed;
                captureProgress = Mathf.Clamp01(captureProgress);

                capturedPlayer.transform.position = Vector3.Lerp(captureStartPosition, holdPoint.position, captureProgress);
                capturedPlayer.transform.rotation = Quaternion.Slerp(
                    capturedPlayer.transform.rotation,
                    holdPoint.rotation,
                    rotationSpeed * Time.deltaTime
                );

                if (captureProgress >= 1f)
                    isMovingToHoldPoint = false;
            }
            else
            {
                capturedPlayer.transform.position = holdPoint.position;
                capturedPlayer.transform.rotation = holdPoint.rotation;
            }
        }
    }
}