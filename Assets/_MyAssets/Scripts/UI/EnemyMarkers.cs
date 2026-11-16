using System.Collections.Generic;
using CorrentesDaNoite.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class EnemyMarkers : MonoBehaviour
    {
        const int FOOTPRINT_COUNT = 24;
        const float RANGE = 20f;
        const float FOOTPRINT_TIME = 2.2f;
        const float FOOT_GAP = 0.13f;
        const float EDGE_MARGIN = 90f;
        const float ARROW_OFFSET = 42f;
        const float MIN_ALPHA = 0.35f;
        const float FADE_SPEED = 4f;
        const float CHEST_HEIGHT = 1.8f;
        static readonly Vector2 FOOTPRINT_SIZE = new(34f, 50f);
        static readonly Vector2 EYE_SIZE = new(60f, 60f);
        static readonly Vector2 ARROW_SIZE = new(26f, 26f);
        static readonly Color CALM_COLOR = new(0.91f, 0.88f, 0.82f);
        static readonly Color ALERT_COLOR = new(1f, 0.6f, 0.15f);
        static readonly Color CHASE_COLOR = new(0.8f, 0.05f, 0.05f);

        public static EnemyMarkers Instance { get; private set; }

        [SerializeField] Sprite _footprint;
        [SerializeField] Sprite _eye;
        [SerializeField] Sprite _arrow;

        class Footprint
        {
            public Image Image;
            public Vector3 Position;
            public Vector3 Forward;
            public float Alpha;
        }

        class EdgeMarker
        {
            public EnemyController Enemy;
            public Image Eye;
            public Image Arrow;
            public float Alpha;
        }

        readonly Footprint[] _footprints = new Footprint[FOOTPRINT_COUNT];
        readonly List<EdgeMarker> _edgeMarkers = new();
        RectTransform _area;
        Transform _player;
        int _occluders;
        int _next;
        bool _leftFoot;

        private void Awake()
        {
            Instance = this;
            _area = (RectTransform)transform;
            _occluders = LayerMask.GetMask("Default", "Ground", "Obstacle");
            for (int i = 0; i < FOOTPRINT_COUNT; i++) _footprints[i] = new Footprint { Image = CreateImage(_footprint, FOOTPRINT_SIZE) };
        }

        private void Start()
        {
            foreach (EnemyController enemy in FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                _edgeMarkers.Add(new EdgeMarker { Enemy = enemy, Eye = CreateImage(_eye, EYE_SIZE), Arrow = CreateImage(_arrow, ARROW_SIZE) });
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            UnityEngine.Camera view = UnityEngine.Camera.main;
            if (view == null || !FindPlayer()) return;
            foreach (Footprint footprint in _footprints) UpdateFootprint(footprint, view);
            foreach (EdgeMarker marker in _edgeMarkers) UpdateEdgeMarker(marker, view);
        }

        // cada passo perto deixa uma pegada no lugar exato e ela aparece ate atras de parede
        public void Step(EnemyController enemy, Transform feet)
        {
            if (!FindPlayer() || (feet.position - _player.position).sqrMagnitude > RANGE * RANGE) return;
            Footprint footprint = _footprints[_next];
            _next = (_next + 1) % FOOTPRINT_COUNT;
            _leftFoot = !_leftFoot;
            footprint.Forward = Vector3.ProjectOnPlane(feet.forward, Vector3.up).normalized;
            footprint.Position = feet.position + Vector3.Cross(Vector3.up, footprint.Forward) * (_leftFoot ? -FOOT_GAP : FOOT_GAP);
            footprint.Alpha = 1f;
            footprint.Image.color = ThreatColor(enemy);
            footprint.Image.rectTransform.localScale = new Vector3(_leftFoot ? -1f : 1f, 1f, 1f);
        }

        public static Color ThreatColor(EnemyController enemy)
        {
            EnemyState state = enemy.StateMachine?.CurrentState;
            if (state is EnemySpottedState or EnemyChaseState or EnemyCaptureState or SleepingEnemyStandingUpState or SleepingEnemyIdleReadyState) return CHASE_COLOR;
            if (enemy.IsAlertedBySound || state is EnemySearchState or SleepingEnemyStartledState) return ALERT_COLOR;
            return Color.Lerp(CALM_COLOR, ALERT_COLOR, enemy.TryGetComponent(out EnemyVisionDetector vision) ? vision.NoticeProgress : 0f);
        }

        void UpdateFootprint(Footprint footprint, UnityEngine.Camera view)
        {
            if (footprint.Alpha <= 0f) return;
            footprint.Alpha -= Time.deltaTime / FOOTPRINT_TIME;
            Vector3 screen = view.WorldToScreenPoint(footprint.Position);
            Vector3 ahead = view.WorldToScreenPoint(footprint.Position + footprint.Forward);
            SetAlpha(footprint.Image, screen.z > 0f ? footprint.Alpha : 0f);
            if (screen.z <= 0f) return;

            footprint.Image.rectTransform.anchoredPosition = ToLocal(screen);
            footprint.Image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(ahead.y - screen.y, ahead.x - screen.x) * Mathf.Rad2Deg - 90f);
        }

        // fora da tela vira um olho na borda com seta e atras de parede um olho no lugar dele
        void UpdateEdgeMarker(EdgeMarker marker, UnityEngine.Camera view)
        {
            EnemyController enemy = marker.Enemy;
            float distance = enemy != null && enemy.isActiveAndEnabled ? Vector3.Distance(enemy.transform.position, _player.position) : RANGE;
            bool inRange = distance < RANGE;
            Vector3 chest = inRange ? enemy.transform.position + Vector3.up * CHEST_HEIGHT : Vector3.zero;
            Vector3 viewport = inRange ? view.WorldToViewportPoint(chest) : Vector3.zero;
            bool offScreen = inRange && (viewport.z < 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f);
            bool behindWall = inRange && !offScreen && Physics.Linecast(view.transform.position, chest, out RaycastHit hit, _occluders, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(enemy.transform);
            marker.Alpha = Mathf.MoveTowards(marker.Alpha, offScreen || behindWall ? Mathf.Lerp(MIN_ALPHA, 1f, 1f - distance / RANGE) : 0f, Time.deltaTime * FADE_SPEED);
            marker.Eye.enabled = marker.Alpha > 0f;
            marker.Arrow.enabled = marker.Alpha > 0f && offScreen;
            if (marker.Alpha <= 0f || !inRange) return;

            Color color = ThreatColor(enemy);
            color.a = marker.Alpha;
            marker.Eye.color = marker.Arrow.color = color;
            if (!offScreen)
            {
                marker.Eye.rectTransform.anchoredPosition = ToLocal(view.WorldToScreenPoint(chest));
                return;
            }

            Vector2 direction = Vector2.Scale(new Vector2(viewport.x - 0.5f, viewport.y - 0.5f), _area.rect.size);
            if (viewport.z < 0f) direction = -direction;
            if (direction.sqrMagnitude < 1f) direction = Vector2.down;
            Vector2 limit = _area.rect.size * 0.5f - Vector2.one * EDGE_MARGIN;
            Vector2 edge = direction * Mathf.Min(limit.x / Mathf.Abs(direction.x), limit.y / Mathf.Abs(direction.y));
            marker.Eye.rectTransform.anchoredPosition = edge;
            marker.Arrow.rectTransform.anchoredPosition = edge + direction.normalized * ARROW_OFFSET;
            marker.Arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }

        Image CreateImage(Sprite sprite, Vector2 size)
        {
            var image = new GameObject(sprite != null ? sprite.name : "Marker", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.rectTransform.SetParent(_area, false);
            image.rectTransform.sizeDelta = size;
            image.sprite = sprite;
            image.raycastTarget = false;
            image.color = Color.clear;
            return image;
        }

        Vector2 ToLocal(Vector3 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_area, screen, null, out Vector2 local);
            return local;
        }

        static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = Mathf.Max(alpha, 0f);
            image.color = color;
        }

        bool FindPlayer()
        {
            if (_player == null) _player = GameObject.FindWithTag("Player")?.transform;
            return _player != null;
        }
    }
}