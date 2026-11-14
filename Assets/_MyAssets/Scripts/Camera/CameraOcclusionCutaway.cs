using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CorrentesDaNoite.Camera
{
    public class CameraOcclusionCutaway : MonoBehaviour
    {
        const float FOCUS_HEIGHT = 1f;
        const float CLEARANCE = 0.25f;
        const float FLOOR_CLEARANCE = 0.05f;
        const float DETECTION_RADIUS = 1f;
        const float SCAN_INTERVAL = 0.08f;
        const float FADE_TIME = 0.35f;
        const float OVERSHOOT = 1.7f;
        const float MAX_CAMERA_DISTANCE = 25f;

        static readonly int ActiveId = Shader.PropertyToID("_CameraOcclusionActive");
        static readonly int TargetId = Shader.PropertyToID("_CameraOcclusionTarget");
        static readonly int FloorId = Shader.PropertyToID("_CameraOcclusionFloor");
        static readonly int AmountId = Shader.PropertyToID("_OcclusionAmount");

        [SerializeField] Shader _shader;
        [SerializeField] LayerMask _obstacleLayers;
        [SerializeField] float _radius = 1.5f;

        readonly Dictionary<Renderer, Obstacle> _obstacles = new();
        readonly Dictionary<Material, Material> _cutawayMaterials = new();
        readonly HashSet<Renderer> _blocking = new();
        readonly List<Renderer> _expired = new();
        readonly List<Renderer> _candidates = new();
        UnityEngine.Camera _camera;
        Transform _player;
        Collider _playerCollider;
        MaterialPropertyBlock _block;
        float _nextScan;

        class Obstacle
        {
            public Material[] Originals;
            public Material[] Replacements;
            public float Amount;
        }

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            _block = new MaterialPropertyBlock();
        }

        private void Start()
        {
            foreach (MeshRenderer renderer in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if ((_obstacleLayers.value & 1 << renderer.gameObject.layer) != 0 && System.Array.Exists(renderer.sharedMaterials, CanCut)) _candidates.Add(renderer);
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += BeginCamera;
            RenderPipelineManager.endCameraRendering += EndCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCamera;
            RenderPipelineManager.endCameraRendering -= EndCamera;
            RestoreAll();
        }

        private void OnDestroy()
        {
            foreach (Material material in _cutawayMaterials.Values) Destroy(material);
        }

        void BeginCamera(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (camera != _camera) return;
            // longe do player e cutscene ou preview de rota entao nao fura nada
            if (!FindPlayer() || Vector3.Distance(_camera.transform.position, _player.position) > MAX_CAMERA_DISTANCE)
            {
                RestoreAll();
                return;
            }

            Vector3 focus = _player.position + Vector3.up * FOCUS_HEIGHT;
            if (Time.unscaledTime >= _nextScan)
            {
                Scan(focus);
                _nextScan = Time.unscaledTime + SCAN_INTERVAL;
            }
            UpdateAmounts();
            Shader.SetGlobalVector(TargetId, new Vector4(focus.x, focus.y, focus.z, _radius));
            Shader.SetGlobalFloat(FloorId, _playerCollider.bounds.min.y + FLOOR_CLEARANCE);
            Shader.SetGlobalFloat(ActiveId, 1f);
        }

        void EndCamera(ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (camera == _camera) Shader.SetGlobalFloat(ActiveId, 0f);
        }

        bool FindPlayer()
        {
            if (_player != null) return _player.gameObject.activeInHierarchy;
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return false;
            _player = player.transform;
            _playerCollider = player.GetComponent<Collider>();
            return true;
        }

        void Scan(Vector3 focus)
        {
            _blocking.Clear();
            Vector3 origin = _camera.transform.position;
            Ray ray = new(origin, focus - origin);
            float reach = Vector3.Distance(origin, focus) - CLEARANCE;
            Bounds player = _playerCollider.bounds;

            foreach (Renderer renderer in _candidates)
            {
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                Bounds bounds = renderer.bounds;
                // so o que e mais alto que o player tapa ele de verdade
                if (bounds.size.y < player.size.y || bounds.max.y < player.max.y) continue;
                bounds.Expand(DETECTION_RADIUS);
                if (bounds.IntersectRay(ray, out float distance) && distance < reach) Register(renderer);
            }
        }

        void Register(Renderer renderer)
        {
            _blocking.Add(renderer);
            if (_obstacles.ContainsKey(renderer)) return;

            Material[] originals = renderer.sharedMaterials;
            Material[] replacements = (Material[])originals.Clone();
            for (int i = 0; i < originals.Length; i++) if (CanCut(originals[i])) replacements[i] = GetCutaway(originals[i]);
            renderer.sharedMaterials = replacements;
            _obstacles.Add(renderer, new Obstacle { Originals = originals, Replacements = replacements });
        }

        // so o lit opaco tem a versao com recorte
        static bool CanCut(Material material) => material != null && material.shader.name == "Universal Render Pipeline/Lit" && material.GetFloat("_Surface") < 0.5f;

        Material GetCutaway(Material original)
        {
            if (_cutawayMaterials.TryGetValue(original, out Material cutaway)) return cutaway;
            cutaway = new Material(original) { shader = _shader, hideFlags = HideFlags.HideAndDontSave };
            _cutawayMaterials.Add(original, cutaway);
            return cutaway;
        }

        void UpdateAmounts()
        {
            _expired.Clear();
            foreach (var pair in _obstacles)
            {
                bool blocking = _blocking.Contains(pair.Key);
                Obstacle obstacle = pair.Value;
                obstacle.Amount = Mathf.MoveTowards(obstacle.Amount, blocking ? 1f : 0f, Time.unscaledDeltaTime / FADE_TIME);
                if (pair.Key == null || obstacle.Amount <= 0f)
                {
                    _expired.Add(pair.Key);
                    continue;
                }
                pair.Key.GetPropertyBlock(_block);
                _block.SetFloat(AmountId, blocking ? EaseOutBack(obstacle.Amount) : Mathf.SmoothStep(0f, 1f, obstacle.Amount));
                pair.Key.SetPropertyBlock(_block);
            }
            foreach (Renderer renderer in _expired)
            {
                Restore(renderer, _obstacles[renderer]);
                _obstacles.Remove(renderer);
            }
        }

        float EaseOutBack(float t)
        {
            float back = t - 1f;
            return 1f + (OVERSHOOT + 1f) * back * back * back + OVERSHOOT * back * back;
        }

        void RestoreAll()
        {
            Shader.SetGlobalFloat(ActiveId, 0f);
            foreach (var pair in _obstacles) Restore(pair.Key, pair.Value);
            _obstacles.Clear();
            _blocking.Clear();
            _nextScan = 0f;
        }

        void Restore(Renderer renderer, Obstacle obstacle)
        {
            if (renderer == null) return;
            Material[] current = renderer.sharedMaterials;
            for (int i = 0; i < current.Length && i < obstacle.Replacements.Length; i++) if (current[i] == obstacle.Replacements[i]) current[i] = obstacle.Originals[i];
            renderer.sharedMaterials = current;
            renderer.SetPropertyBlock(null);
        }
    }
}