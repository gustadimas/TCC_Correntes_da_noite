using System.Collections;
using UnityEngine;
using CorrentesDaNoite.Checkpoint;

namespace CorrentesDaNoite.UI
{
    public class SaveNotice : MonoBehaviour
    {
        const float FADE_TIME = 0.4f;
        const float HOLD_TIME = 2f;

        [SerializeField] CanvasGroup _group;

        Coroutine _show;

        private void Awake() => _group.alpha = 0f;
        private void OnEnable() => CheckpointManager.OnCheckpointReached += Show;
        private void OnDisable() => CheckpointManager.OnCheckpointReached -= Show;

        void Show()
        {
            if (_show != null) StopCoroutine(_show);
            _show = StartCoroutine(ShowRoutine());
        }

        IEnumerator ShowRoutine()
        {
            while (_group.alpha < 1f)
            {
                _group.alpha = Mathf.MoveTowards(_group.alpha, 1f, Time.deltaTime / FADE_TIME);
                yield return null;
            }
            yield return new WaitForSeconds(HOLD_TIME);
            while (_group.alpha > 0f)
            {
                _group.alpha = Mathf.MoveTowards(_group.alpha, 0f, Time.deltaTime / FADE_TIME);
                yield return null;
            }
        }
    }
}