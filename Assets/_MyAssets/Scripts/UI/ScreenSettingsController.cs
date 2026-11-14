using System.Collections.Generic;
using UnityEngine;
using CorrentesDaNoite.Camera;

namespace CorrentesDaNoite.UI
{
    public class ScreenSettingsController : MonoBehaviour
    {
        static readonly string[] YES_NO = { "Não", "Sim" };
        static readonly string[] ON_OFF = { "Desligado", "Ligado" };

        [SerializeField] OptionSelector _fullscreen;
        [SerializeField] OptionSelector _resolution;
        [SerializeField] OptionSelector _cameraSway;

        readonly List<Resolution> _resolutions = new();

        private void Awake()
        {
            foreach (Resolution resolution in Screen.resolutions)
                if (!_resolutions.Exists(added => added.width == resolution.width && added.height == resolution.height)) _resolutions.Add(resolution);

            _fullscreen.Changed += index => Screen.fullScreenMode = index == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            _resolution.Changed += index => Screen.SetResolution(_resolutions[index].width, _resolutions[index].height, Screen.fullScreenMode);
            _cameraSway.Changed += index => CameraSway.SetEnabled(index == 1);
        }

        private void OnEnable()
        {
            _fullscreen.Setup(YES_NO, Screen.fullScreen ? 1 : 0);
            _resolution.Setup(_resolutions.ConvertAll(added => added.width + " x " + added.height).ToArray(), GetCurrentResolution());
            _cameraSway.Setup(ON_OFF, CameraSway.Enabled ? 1 : 0);
        }

        // janela com tamanho solto cai na maior resolucao que cabe nela
        int GetCurrentResolution()
        {
            int index = 0;
            for (int i = 0; i < _resolutions.Count; i++) if (_resolutions[i].width <= Screen.width) index = i;
            return index;
        }
    }
}