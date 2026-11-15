using System.Collections;
using TMPro;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class PhaseCard : MonoBehaviour
    {
        const float FADE_TIME = 0.6f;
        const float HOLD_TIME = 1.6f;

        public static PhaseCard Instance { get; private set; }

        [SerializeField] CanvasGroup _group;
        [SerializeField] TMP_Text _text;

        private void Awake()
        {
            Instance = this;
            _group.alpha = 0f;
        }

        // roda com a tela preta do teleporte
        public IEnumerator Show(string chapter)
        {
            _text.text = chapter;
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