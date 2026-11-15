using System.Collections;
using TMPro;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class CaptureScreen : MonoBehaviour
    {
        const float FADE_TIME = 0.3f;
        const float HOLD_TIME = 2.2f;

        [SerializeField] CanvasGroup _group;
        [SerializeField] TMP_Text _title;
        [SerializeField] TMP_Text _tip;
        [SerializeField] string[] _tips;

        private void Awake() => _group.alpha = 0f;

        // roda com a tela preta do respawn
        public IEnumerator Show(bool captured)
        {
            _title.text = captured ? "Você foi capturada" : "Tente de novo";
            _tip.text = _tips.Length > 0 ? _tips[Random.Range(0, _tips.Length)] : "";
            yield return Fade(1f);
            yield return new WaitForSeconds(HOLD_TIME);
            yield return Fade(0f);
        }

        IEnumerator Fade(float target)
        {
            while (_group.alpha != target)
            {
                _group.alpha = Mathf.MoveTowards(_group.alpha, target, Time.deltaTime / FADE_TIME);
                yield return null;
            }
        }
    }
}