using UnityEngine;
using UnityEngine.Rendering;

namespace CorrentesDaNoite.Enemies
{
    public class EnemyVisionCone : MonoBehaviour
    {
        const int RAY_COUNT = 32;
        const float GROUND_OFFSET = 0.06f;
        const float SHOW_EXTRA_DISTANCE = 10f;
        const float FADE_SPEED = 3f;
        const float CENTER_ALPHA = 0.04f;
        const float EDGE_ALPHA = 0.14f;

        [SerializeField] Material _material;

        EnemyVisionDetector _vision;
        EnemyController _enemy;
        Transform _player;
        MeshRenderer _renderer;
        Mesh _mesh;
        readonly Vector3[] _vertices = new Vector3[RAY_COUNT + 2];
        readonly Color[] _colors = new Color[RAY_COUNT + 2];
        float _visibility;

        private void Awake()
        {
            _vision = GetComponent<EnemyVisionDetector>();
            _enemy = GetComponent<EnemyController>();

            var triangles = new int[RAY_COUNT * 3];
            for (int i = 0; i < RAY_COUNT; i++)
            {
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            _mesh = new Mesh { name = "VisionCone", vertices = _vertices, triangles = triangles };
            _mesh.MarkDynamic();

            var cone = new GameObject("VisionCone", typeof(MeshFilter), typeof(MeshRenderer));
            cone.transform.SetParent(transform, false);
            cone.GetComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = cone.GetComponent<MeshRenderer>();
            _renderer.sharedMaterial = _material;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.enabled = false;
        }

        private void OnDestroy() => Destroy(_mesh);

        // so aparece perto dela e mostra ate onde ele enxerga cortando nas paredes
        private void LateUpdate()
        {
            if (!FindPlayer()) return;
            float range = _vision.ViewDistance;
            bool near = _vision.enabled && (_player.position - transform.position).sqrMagnitude < (range + SHOW_EXTRA_DISTANCE) * (range + SHOW_EXTRA_DISTANCE);
            _visibility = Mathf.MoveTowards(_visibility, near ? 1f : 0f, Time.deltaTime * FADE_SPEED);
            _renderer.enabled = _visibility > 0f;
            if (_visibility <= 0f) return;

            Transform eye = _vision.Eye;
            Vector3 forward = Vector3.ProjectOnPlane(eye.forward, Vector3.up).normalized;
            float halfAngle = _vision.ViewAngle * 0.5f;
            Color color = UI.EnemyMarkers.ThreatColor(_enemy);

            _vertices[0] = ToGround(eye.position);
            _colors[0] = WithAlpha(color, CENTER_ALPHA * _visibility);
            for (int i = 0; i <= RAY_COUNT; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(Mathf.Lerp(-halfAngle, halfAngle, (float)i / RAY_COUNT), Vector3.up) * forward;
                float distance = Physics.Raycast(eye.position, direction, out RaycastHit hit, range, _vision.ObstructionMask, QueryTriggerInteraction.Ignore) ? hit.distance : range;
                _vertices[i + 1] = ToGround(eye.position + direction * distance);
                _colors[i + 1] = WithAlpha(color, EDGE_ALPHA * _visibility);
            }
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.RecalculateBounds();
        }

        Vector3 ToGround(Vector3 world)
        {
            Vector3 local = transform.InverseTransformPoint(world);
            local.y = GROUND_OFFSET;
            return local;
        }

        static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        bool FindPlayer()
        {
            if (_player == null) _player = GameObject.FindWithTag("Player")?.transform;
            return _player != null;
        }
    }
}