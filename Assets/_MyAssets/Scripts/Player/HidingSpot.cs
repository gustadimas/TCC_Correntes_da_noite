using System.Collections.Generic;
using UnityEngine;

namespace CorrentesDaNoite.Player
{
    public class HidingSpot : MonoBehaviour
    {
        // colado nela o inimigo acha mesmo dentro da moita
        const float REVEAL_DISTANCE = 1.5f;

        static readonly List<Collider> _areas = new();

        Collider _area;

        private void Awake() => _area = GetComponent<Collider>();
        private void OnEnable() => _areas.Add(_area);
        private void OnDisable() => _areas.Remove(_area);

        public static bool IsHiding(PlayerController player)
        {
            if (player == null || !player.IsCrouching) return false;
            Vector3 point = player.transform.position + Vector3.up * 0.5f;
            foreach (Collider area in _areas) if (area.ClosestPoint(point) == point) return true;
            return false;
        }

        public static bool IsHiddenFrom(PlayerController player, Vector3 viewer) => IsHiding(player) && (player.transform.position - viewer).sqrMagnitude > REVEAL_DISTANCE * REVEAL_DISTANCE;
    }
}