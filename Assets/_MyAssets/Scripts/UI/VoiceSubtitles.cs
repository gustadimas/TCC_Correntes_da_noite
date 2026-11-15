using System.Collections;
using TMPro;
using UnityEngine;

namespace CorrentesDaNoite.UI
{
    [System.Serializable]
    public struct VoiceLine
    {
        public AudioClip clip;
        public string subtitle;

        public void Play()
        {
            if (clip != null) VoiceSubtitles.Instance?.Say(this);
        }
    }

    public class VoiceSubtitles : MonoBehaviour
    {
        const float FADE_TIME = 0.25f;
        const float EXTRA_TIME = 0.6f;
        const float CAPTION_TIME = 1.5f;

        public static VoiceSubtitles Instance { get; private set; }

        [SerializeField] AudioSource _source;
        [SerializeField] CanvasGroup _group;
        [SerializeField] TMP_Text _text;

        Coroutine _show;

        private void Awake()
        {
            Instance = this;
            _group.alpha = 0f;
        }

        // a fala nova corta a que estiver tocando
        public void Say(VoiceLine line)
        {
            _source.Stop();
            _source.clip = line.clip;
            _source.Play();
            Show(line.subtitle, line.clip.length + EXTRA_TIME);
        }

        // som importante so aparece se nao tiver fala tocando
        public void Caption(string text)
        {
            if (!_source.isPlaying) Show(text, CAPTION_TIME);
        }

        void Show(string text, float duration)
        {
            if (_show != null) StopCoroutine(_show);
            _show = StartCoroutine(ShowFor(text, duration));
        }

        IEnumerator ShowFor(string text, float duration)
        {
            _text.text = text;
            for (float timer = 0f; timer < duration; timer += Time.deltaTime)
            {
                _group.alpha = Mathf.MoveTowards(_group.alpha, 1f, Time.deltaTime / FADE_TIME);
                yield return null;
            }
            while (_group.alpha > 0f)
            {
                _group.alpha = Mathf.MoveTowards(_group.alpha, 0f, Time.deltaTime / FADE_TIME);
                yield return null;
            }
        }
    }
}