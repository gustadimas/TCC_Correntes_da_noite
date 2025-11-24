using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class AudioSettingsController : MonoBehaviour
    {
        [Header("Mixer")]
        [SerializeField] internal AudioMixer audioMixer;
        [SerializeField] internal string masterParameter = "MasterVolume";
        [SerializeField] internal string musicParameter = "MusicVolume";
        [SerializeField] internal string sfxParameter = "SfxVolume";

        [Header("Sliders")]
        [SerializeField] internal Slider masterSlider;
        [SerializeField] internal Slider musicSlider;
        [SerializeField] internal Slider sfxSlider;
        [SerializeField] internal float sliderMin = 0f;
        [SerializeField] internal float sliderMax = 1f;
        [SerializeField] internal float defaultValue = 1f;

        [Header("Persistence Keys")]
        [SerializeField] internal string masterKey = "Audio.MasterVolume";
        [SerializeField] internal string musicKey = "Audio.MusicVolume";
        [SerializeField] internal string sfxKey = "Audio.SfxVolume";

        [Header("dB Range")]
        [SerializeField] internal float minDb = -80f;
        [SerializeField] internal float maxDb = 0f;

        protected bool suppressEvents;

        protected void Awake()
        {
            ConfigureSliders();
            BindListeners();
        }

        protected void OnEnable()
        {
            LoadAndApply();
        }

        protected void OnDisable()
        {
            SaveAll();
        }

        protected void OnDestroy()
        {
            UnbindListeners();
        }

        protected void ConfigureSliders()
        {
            ConfigureSlider(masterSlider);
            ConfigureSlider(musicSlider);
            ConfigureSlider(sfxSlider);
        }

        protected void ConfigureSlider(Slider slider)
        {
            if (slider == null)
                return;

            slider.minValue = sliderMin;
            slider.maxValue = sliderMax;
        }

        protected void BindListeners()
        {
            masterSlider?.onValueChanged.AddListener(OnMasterChanged);
            musicSlider?.onValueChanged.AddListener(OnMusicChanged);
            sfxSlider?.onValueChanged.AddListener(OnSfxChanged);
        }

        protected void UnbindListeners()
        {
            masterSlider?.onValueChanged.RemoveListener(OnMasterChanged);
            musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
            sfxSlider?.onValueChanged.RemoveListener(OnSfxChanged);
        }

        protected void LoadAndApply()
        {
            suppressEvents = true;

            float masterValue = NormalizeLegacyValue(LoadValue(masterKey));
            float musicValue = NormalizeLegacyValue(LoadValue(musicKey));
            float sfxValue = NormalizeLegacyValue(LoadValue(sfxKey));

            ApplyVolume(masterParameter, masterValue);
            ApplyVolume(musicParameter, musicValue);
            ApplyVolume(sfxParameter, sfxValue);

            SetSliderValue(masterSlider, masterValue);
            SetSliderValue(musicSlider, musicValue);
            SetSliderValue(sfxSlider, sfxValue);

            suppressEvents = false;
        }

        protected void SaveAll()
        {
            SaveValue(masterKey, masterSlider != null ? masterSlider.value : defaultValue);
            SaveValue(musicKey, musicSlider != null ? musicSlider.value : defaultValue);
            SaveValue(sfxKey, sfxSlider != null ? sfxSlider.value : defaultValue);
            PlayerPrefs.Save();
        }

        protected void OnMasterChanged(float value)
        {
            if (suppressEvents)
                return;

            ApplyVolume(masterParameter, value);
            SaveValue(masterKey, value);
        }

        protected void OnMusicChanged(float value)
        {
            if (suppressEvents)
                return;

            ApplyVolume(musicParameter, value);
            SaveValue(musicKey, value);
        }

        protected void OnSfxChanged(float value)
        {
            if (suppressEvents)
                return;

            ApplyVolume(sfxParameter, value);
            SaveValue(sfxKey, value);
        }

        protected float LoadValue(string key)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        protected void SaveValue(string key, float value)
        {
            float clamped = Mathf.Clamp(value, sliderMin, sliderMax);
            PlayerPrefs.SetFloat(key, clamped);
        }

        protected void SetSliderValue(Slider slider, float value)
        {
            if (slider == null)
                return;

            slider.SetValueWithoutNotify(value);
        }

        protected void ApplyVolume(string parameter, float linearValue)
        {
            if (audioMixer == null || string.IsNullOrEmpty(parameter))
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