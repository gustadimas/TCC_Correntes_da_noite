using UnityEngine;
using UnityEngine.Audio;

namespace CorrentesDaNoite.Audio
{
    public class AudioVolumeBootstrap : MonoBehaviour
    {
        [Header("Mixer")]
        [SerializeField] internal AudioMixer audioMixer;
        [SerializeField] internal string masterParameter = "MasterVolume";
        [SerializeField] internal string musicParameter = "MusicVolume";
        [SerializeField] internal string sfxParameter = "SfxVolume";

        [Header("Persistence Keys")]
        [SerializeField] internal string masterKey = "Audio.MasterVolume";
        [SerializeField] internal string musicKey = "Audio.MusicVolume";
        [SerializeField] internal string sfxKey = "Audio.SfxVolume";

        [Header("Range")]
        [SerializeField] internal float sliderMin = 0f;
        [SerializeField] internal float sliderMax = 1f;
        [SerializeField] internal float defaultValue = 1f;
        [SerializeField] internal float minDb = -80f;
        [SerializeField] internal float maxDb = 0f;

        [Header("Timing")]
        [SerializeField] internal bool applyOnAwake = true;
        [SerializeField] internal bool applyOnEnable = true;

        void Awake()
        {
            if (applyOnAwake)
                ApplyFromPrefs();
        }

        void OnEnable()
        {
            if (applyOnEnable)
                ApplyFromPrefs();
        }

        public void ApplyFromPrefs()
        {
            if (audioMixer == null)
                return;

            ApplyVolume(masterParameter, LoadValue(masterKey));
            ApplyVolume(musicParameter, LoadValue(musicKey));
            ApplyVolume(sfxParameter, LoadValue(sfxKey));
        }

        float LoadValue(string key)
        {
            float stored = PlayerPrefs.GetFloat(key, defaultValue);
            return NormalizeLegacyValue(stored);
        }

        void ApplyVolume(string parameter, float linearValue)
        {
            if (string.IsNullOrEmpty(parameter))
                return;

            float clamped = Mathf.Clamp(linearValue, sliderMin, sliderMax);
            float normalized = Mathf.InverseLerp(sliderMin, sliderMax, clamped);
            float db = normalized > 0f ? Mathf.Log10(normalized) * 20f : minDb;
            db = Mathf.Clamp(db, minDb, maxDb);
            audioMixer.SetFloat(parameter, db);
        }

        float NormalizeLegacyValue(float value)
        {
            if (sliderMax <= 1.01f && value > 1.01f)
                return value / 100f;
            return value;
        }
    }
}