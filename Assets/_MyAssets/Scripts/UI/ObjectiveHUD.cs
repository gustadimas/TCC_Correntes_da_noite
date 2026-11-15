using System.Collections;
using TMPro;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    public class ObjectiveHUD : MonoBehaviour
    {
        const float FADE_TIME = 0.6f;

        public static ObjectiveHUD Instance { get; private set; }

        [SerializeField] CanvasGroup _group;
        [SerializeField] TMP_Text _text;

        Coroutine _swap;

        private void Awake()
        {
            Instance = this;
            _group.alpha = 0f;
        }

        // texto vazio so esconde
        public void Show(string objective)
        {
            if (_swap != null) StopCoroutine(_swap);
            _swap = StartCoroutine(Swap(objective));
        }

        IEnumerator Swap(string objective)
        {
            yield return Fade(0f);
            _text.text = objective;
            if (!string.IsNullOrEmpty(objective)) yield return Fade(1f);
        }

        IEnumerator Fade(float target)
        {
            while (_group.alpha != target)
            {
                _group.alpha = Mathf.MoveTowards(_group.alpha, target, Time.unscaledDeltaTime / FADE_TIME);
                yield return null;
            }
        }
    }
}