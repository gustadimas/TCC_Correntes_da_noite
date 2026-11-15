using UnityEngine;
using CorrentesDaNoite.UI;

namespace CorrentesDaNoite.Audio
{
    public class TensionMusic : MonoBehaviour
    {
        const string CALM_KEY = "Game";
        const string TENSE_KEY = "Chase";
        const float TENSE_FADE = 1f;
        const float CALM_FADE = 3f;
        const float CALM_DELAY = 5f;

        bool _tense;
        float _calmTimer;

        // so devolve a musica calma se foi ela que trocou, a perseguicao da mata cuida da propria musica
        private void Update()
        {
            if (StealthHUD.Danger >= 1f)
            {
                _calmTimer = 0f;
                if (_tense) return;
                _tense = true;
                MusicManager.GetOrCreate().PlayMusic(TENSE_KEY, TENSE_FADE);
                return;
            }

            if (!_tense) return;
            _calmTimer += Time.deltaTime;
            if (_calmTimer < CALM_DELAY) return;
            _tense = false;
            MusicManager.GetOrCreate().PlayMusic(CALM_KEY, CALM_FADE);
        }
    }
}