using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManagerUI : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Textos de Volume")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Configuracoes")]
    [SerializeField] private float minVolumeDB = -80f;
    [SerializeField] private float maxVolumeDB = 0f;
    [SerializeField] private bool saveSettings = true;

    private const string MASTER_VOLUME_PARAM = "AudioMasterVolume";
    private const string MUSIC_VOLUME_PARAM = "AudioMusicVolume";
    private const string SFX_VOLUME_PARAM = "AudioSFXVolume";

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Start()
    {
        InitializeSliders();
        LoadAudioSettings();
        SetupSliderListeners();
    }

    private void InitializeSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 100f;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 100f;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 100f;
        }
    }

    private void SetupSliderListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        if (audioMixer == null)
        {
            return;
        }

        float dbValue = ConvertToDecibels(volume);
        audioMixer.SetFloat(MASTER_VOLUME_PARAM, dbValue);
        UpdateVolumeText(masterVolumeText, volume);

        if (saveSettings)
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer == null)
        {
            return;
        }

        float dbValue = ConvertToDecibels(volume);
        audioMixer.SetFloat(MUSIC_VOLUME_PARAM, dbValue);
        UpdateVolumeText(musicVolumeText, volume);

        if (saveSettings)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer == null)
        {
            return;
        }

        float dbValue = ConvertToDecibels(volume);
        audioMixer.SetFloat(SFX_VOLUME_PARAM, dbValue);
        UpdateVolumeText(sfxVolumeText, volume);

        if (saveSettings)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        }
    }

    private float ConvertToDecibels(float linearValue)
    {
        if (linearValue <= 0f)
        {
            return minVolumeDB;
        }

        float normalizedValue = linearValue / 100f;
        float dbValue = Mathf.Log10(normalizedValue) * 20f;

        return Mathf.Clamp(dbValue, minVolumeDB, maxVolumeDB);
    }

    private float ConvertToLinear(float dbValue)
    {
        if (dbValue <= minVolumeDB)
        {
            return 0f;
        }

        float normalizedValue = Mathf.Pow(10f, dbValue / 20f);
        return normalizedValue * 100f;
    }

    private void UpdateVolumeText(TextMeshProUGUI volumeText, float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(volume).ToString();
        }
    }

    public void LoadAudioSettings()
    {
        if (!saveSettings)
        {
            return;
        }

        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 100f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 100f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 100f);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume;
        }

        SetMasterVolume(masterVolume);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }

        SetMusicVolume(musicVolume);

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
        }

        SetSFXVolume(sfxVolume);

        UpdateVolumeText(masterVolumeText, masterVolume);
        UpdateVolumeText(musicVolumeText, musicVolume);
        UpdateVolumeText(sfxVolumeText, sfxVolume);
    }

    public void SaveAudioSettings()
    {
        if (saveSettings)
        {
            PlayerPrefs.Save();
        }
    }

    public void ResetToDefault()
    {
        const float defaultVolume = 100f;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = defaultVolume;
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = defaultVolume;
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = defaultVolume;
        }
    }

    public void MuteAll(bool mute)
    {
        float volume = mute ? 0f : 100f;

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = volume;
        }
    }

    public float GetVolume(AudioTypeUI audioType)
    {
        if (audioMixer == null)
        {
            return 0f;
        }

        float dbValue;
        bool success = false;

        switch (audioType)
        {
            case AudioTypeUI.Master:
                success = audioMixer.GetFloat(MASTER_VOLUME_PARAM, out dbValue);
                break;
            case AudioTypeUI.Music:
                success = audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out dbValue);
                break;
            case AudioTypeUI.SFX:
                success = audioMixer.GetFloat(SFX_VOLUME_PARAM, out dbValue);
                break;
            default:
                return 0f;
        }

        return success ? ConvertToLinear(dbValue) : 0f;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAudioSettings();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAudioSettings();
        }
    }

    private void OnDestroy()
    {
        SaveAudioSettings();
    }

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    private void Update()
    {
        if (showDebugInfo)
        {
            Debug.Log($"Master: {GetVolume(AudioTypeUI.Master):F1}% | " +
                      $"Music: {GetVolume(AudioTypeUI.Music):F1}% | " +
                      $"SFX: {GetVolume(AudioTypeUI.SFX):F1}%");
        }
    }

    [ContextMenu("Test Audio Settings")]
    private void TestAudioSettings()
    {
        Debug.Log("=== AUDIO SETTINGS TEST ===");
        Debug.Log($"Master Volume: {GetVolume(AudioTypeUI.Master)}%");
        Debug.Log($"Music Volume: {GetVolume(AudioTypeUI.Music)}%");
        Debug.Log($"SFX Volume: {GetVolume(AudioTypeUI.SFX)}%");
    }
#endif
}

public enum AudioTypeUI
{
    Master,
    Music,
    SFX
}
