using UnityEngine;

namespace CorrentesDaNoite.Camera
{
    public static class CameraSway
    {
        const string KEY = "Camera.Sway";

        static bool? _enabled;

        public static bool Enabled => _enabled ??= PlayerPrefs.GetInt(KEY, 1) == 1;

        public static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            PlayerPrefs.SetInt(KEY, enabled ? 1 : 0);
        }
    }
}