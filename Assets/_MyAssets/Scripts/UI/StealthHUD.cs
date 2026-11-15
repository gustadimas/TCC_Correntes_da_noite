using UnityEngine;
using UnityEngine.UI;
using CorrentesDaNoite.Audio;
using CorrentesDaNoite.Enemies;
using CorrentesDaNoite.Player;

namespace CorrentesDaNoite.UI
{
    public class StealthHUD : MonoBehaviour
    {
        const float FADE_SPEED = 3f;
        const float RUSTLE_VOLUME = 0.5f;
        const float SOUND_ALERT_DANGER = 0.4f;
        const float MOVING_SPEED = 0.2f;

        static readonly Color BAR_OFF = new(1f, 1f, 1f, 0.2f);
        static readonly Color BAR_LOUD = new(1f, 0.45f, 0.4f, 1f);

        public static float Danger { get; private set; }

        [SerializeField] CanvasGroup _group;
        [SerializeField] AudioClip _rustleSound;
        [SerializeField] VoiceLine _firstHideVoice;
        [SerializeField] CanvasGroup _danger;
        [SerializeField] CanvasGroup _noiseMeter;
        [SerializeField] Image[] _noiseBars;

        PlayerController _player;
        EnemyController[] _enemies;
        EnemyVisionDetector[] _visions;
        bool _hidden;
        bool _hidOnce;

        private void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            _player = player.GetComponent<PlayerController>();
            _enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _visions = System.Array.ConvertAll(_enemies, enemy => enemy.GetComponent<EnemyVisionDetector>());
            _group.alpha = 0f;
            _danger.alpha = 0f;
            _noiseMeter.alpha = 0f;
        }

        private void Update()
        {
            bool hidden = HidingSpot.IsHiding(_player);
            if (hidden && !_hidden) OnHide();
            _hidden = hidden;
            _group.alpha = Mathf.MoveTowards(_group.alpha, hidden ? 1f : 0f, Time.deltaTime * FADE_SPEED);
            Danger = GetDanger();
            _danger.alpha = Mathf.MoveTowards(_danger.alpha, Danger, Time.deltaTime * FADE_SPEED);
            UpdateNoiseMeter();
        }

        void OnHide()
        {
            if (_rustleSound != null) AudioManager.Instance?.PlayConfig(new AudioConfig { clip = _rustleSound, volume = RUSTLE_VOLUME, pitch = 1f, spatialAudio = true }, _player.transform.position);
            if (_hidOnce) return;
            _hidOnce = true;
            _firstHideVoice.Play();
        }

        // o inimigo mais perto de achar ela decide o quanto a tela avermelha
        float GetDanger()
        {
            float danger = 0f;
            for (int i = 0; i < _enemies.Length; i++)
            {
                EnemyController enemy = _enemies[i];
                if (enemy == null || !enemy.isActiveAndEnabled) continue;
                EnemyState state = enemy.StateMachine?.CurrentState;
                if (state is EnemySpottedState or EnemyChaseState or EnemyCaptureState or SleepingEnemyStandingUpState or SleepingEnemyIdleReadyState) return 1f;
                if (enemy.IsAlertedBySound || state is SleepingEnemyStartledState) danger = Mathf.Max(danger, SOUND_ALERT_DANGER);
                if (_visions[i] != null) danger = Mathf.Max(danger, _visions[i].NoticeProgress);
            }
            return danger;
        }

        // parada nada, agachada 1, andando 2 e correndo 3 barras
        void UpdateNoiseMeter()
        {
            int level = _player.CurrentSpeed < MOVING_SPEED ? 0 : _player.IsCrouching ? 1 : _player.IsRunning ? 3 : 2;
            for (int i = 0; i < _noiseBars.Length; i++) _noiseBars[i].color = i >= level ? BAR_OFF : level == 3 ? BAR_LOUD : Color.white;
            _noiseMeter.alpha = Mathf.MoveTowards(_noiseMeter.alpha, level > 0 ? 1f : 0f, Time.deltaTime * FADE_SPEED);
        }
    }
}