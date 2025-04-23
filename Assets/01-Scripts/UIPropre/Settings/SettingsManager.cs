using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }

    [Header("Boutons de qualité")]
    public Button highQualityButton;
    public Button mediumQualityButton;
    public Button lowQualityButton;

    [Header("Sliders de volume")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Boutons de mute")]
    public Button masterMuteButton;
    public TextMeshProUGUI masterMuteButtonText;
    public Button sfxMuteButton;
    public TextMeshProUGUI sfxMuteButtonText;
    public Button musicMuteButton;
    public TextMeshProUGUI musicMuteButtonText;

    [Header("Toggle de vibration")]
    public Toggle vibrationToggle;

    [Header("Feedback visuel (TextMeshPro)")]
    public TextMeshProUGUI qualityFeedbackText;

    [Header("Panneau de paramètres")]
    public GameObject settingsPanel;

    [Header("Référence à l'Audio Mixer")]
    public AudioMixer audioMixer;

    private const string QUALITY_KEY = "QualityLevel";
    private const string MASTER_MUTE_KEY = "IsMasterMuted";
    private const string SFX_MUTE_KEY = "IsSFXMuted";
    private const string MUSIC_MUTE_KEY = "IsMusicMuted";
    private const string VIBRATION_ENABLED_KEY = "IsVibrationEnabled";
    private const string LAST_MASTER_VOLUME_KEY = "LastMasterVolume";
    private const string LAST_SFX_VOLUME_KEY = "LastSFXVolume";
    private const string LAST_MUSIC_VOLUME_KEY = "LastMusicVolume";

    private bool isMasterMuted = false;
    private bool isSFXMuted = false;
    private bool isMusicMuted = false;
    private float lastMasterVolume = 1f;
    private float lastSFXVolume = 1f;
    private float lastMusicVolume = 1f;

    private bool vibrationEnabled = true;
    private float vibrationIntensity = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LoadQualitySettings();
    }

    private void Start()
    {
        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer n'est pas assigné dans SettingsManager !");
            return;
        }

        

        if (highQualityButton != null)
        {
            highQualityButton.onClick.AddListener(() => SetQuality(QualityLevel.High));
        }
        if (mediumQualityButton != null)
        {
            mediumQualityButton.onClick.AddListener(() => SetQuality(QualityLevel.Medium));
        }
        if (lowQualityButton != null)
        {
            lowQualityButton.onClick.AddListener(() => SetQuality(QualityLevel.Low));
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            float savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.value = savedMasterVolume;
            lastMasterVolume = savedMasterVolume;
            SetMasterVolume(savedMasterVolume);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.value = savedSFXVolume;
            lastSFXVolume = savedSFXVolume;
            SetSFXVolume(savedSFXVolume);
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.value = savedMusicVolume;
            lastMusicVolume = savedMusicVolume;
            SetMusicVolume(savedMusicVolume);
        }

        isMasterMuted = PlayerPrefs.GetInt(MASTER_MUTE_KEY, 0) == 1;
        if (isMasterMuted)
        {
            lastMasterVolume = PlayerPrefs.GetFloat(LAST_MASTER_VOLUME_KEY, 1f);
            SetMasterVolume(0f);
            masterVolumeSlider.value = 0f;
        }

        isSFXMuted = PlayerPrefs.GetInt(SFX_MUTE_KEY, 0) == 1;
        if (isSFXMuted)
        {
            lastSFXVolume = PlayerPrefs.GetFloat(LAST_SFX_VOLUME_KEY, 1f);
            SetSFXVolume(0f);
            sfxVolumeSlider.value = 0f;
        }

        isMusicMuted = PlayerPrefs.GetInt(MUSIC_MUTE_KEY, 0) == 1;
        if (isMusicMuted)
        {
            lastMusicVolume = PlayerPrefs.GetFloat(LAST_MUSIC_VOLUME_KEY, 1f);
            SetMusicVolume(0f);
            musicVolumeSlider.value = 0f;
        }

        vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_ENABLED_KEY, 1) == 1;
        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = vibrationEnabled;
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggleChanged);
        }

        if (masterMuteButton != null)
        {
            masterMuteButton.onClick.AddListener(ToggleMasterMute);
        }
        if (sfxMuteButton != null)
        {
            sfxMuteButton.onClick.AddListener(ToggleSFXMute);
        }
        if (musicMuteButton != null)
        {
            musicMuteButton.onClick.AddListener(ToggleMusicMute);
        }

        UpdateMuteButtonStates();
        UpdateButtonStates();
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool newState = !settingsPanel.activeSelf;
            settingsPanel.SetActive(newState);
            Debug.Log($"SettingsPanel défini à l'état : {newState}");
        }
    }

    public void SetQuality(QualityLevel level)
    {
        Debug.Log($"Application du niveau de qualité : {level}");
        switch (level)
        {
            case QualityLevel.Low:
                QualitySettings.SetQualityLevel(2, true);
                Debug.Log("Qualité définie sur : Low (optimisé pour mobile)");
                break;
            case QualityLevel.Medium:
                QualitySettings.SetQualityLevel(1, true);
                Debug.Log("Qualité définie sur : Medium (optimisé pour mobile)");
                break;
            case QualityLevel.High:
                QualitySettings.SetQualityLevel(0, true);
                Debug.Log("Qualité définie sur : High (optimisé pour mobile)");
                break;
        }

        ApplyMobileOptimizations(level);

        PlayerPrefs.SetInt(QUALITY_KEY, (int)level);
        PlayerPrefs.Save();

        UpdateButtonStates();

        if (qualityFeedbackText != null)
        {
            qualityFeedbackText.text = $"Qualité définie sur : {level}\n" +
                (level == QualityLevel.High ? "(Attention : peut consommer plus de batterie)" : "");
            Invoke(nameof(ClearFeedbackText), 3f);
        }
    }

    private void LoadQualitySettings()
    {
        if (PlayerPrefs.HasKey(QUALITY_KEY))
        {
            int savedLevel = PlayerPrefs.GetInt(QUALITY_KEY);
            SetQuality((QualityLevel)savedLevel);
        }
        else
        {
            SetQuality(QualityLevel.Medium);
        }
    }

    public QualityLevel GetCurrentQuality()
    {
        int currentLevel = QualitySettings.GetQualityLevel();
        switch (currentLevel)
        {
            case 0: return QualityLevel.High;
            case 1: return QualityLevel.Medium;
            case 2: return QualityLevel.Low;
            default: return QualityLevel.Medium;
        }
    }

    private void ApplyMobileOptimizations(QualityLevel level)
    {
        switch (level)
        {
            case QualityLevel.Low:
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                Application.targetFrameRate = 30;
                break;
            case QualityLevel.Medium:
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = true;
                Application.targetFrameRate = 60;
                break;
            case QualityLevel.High:
                QualitySettings.antiAliasing = 2;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.billboardsFaceCameraPosition = true;
                Application.targetFrameRate = 60;
                break;
            default:
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                Application.targetFrameRate = 30;
                break;
        }

        Debug.Log($"Optimisations appliquées pour le niveau : {level}");
    }

    private void ClearFeedbackText()
    {
        if (qualityFeedbackText != null)
        {
            qualityFeedbackText.text = "";
        }
    }

    private void UpdateButtonStates()
    {
        QualityLevel currentLevel = GetCurrentQuality();

        if (highQualityButton != null)
        {
            highQualityButton.interactable = (currentLevel != QualityLevel.High);
        }
        if (mediumQualityButton != null)
        {
            mediumQualityButton.interactable = (currentLevel != QualityLevel.Medium);
        }
        if (lowQualityButton != null)
        {
            lowQualityButton.interactable = (currentLevel != QualityLevel.Low);
        }
    }

    private void SetMasterVolume(float volume)
    {
        float dbVolume = Mathf.Clamp(volume, 0.0001f, 1f) > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MasterVolume", dbVolume);
        if (!isMasterMuted)
        {
            lastMasterVolume = volume;
            PlayerPrefs.SetFloat(LAST_MASTER_VOLUME_KEY, lastMasterVolume);
        }
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"Master Volume défini à : {volume} (db: {dbVolume})");
    }

    private void SetSFXVolume(float volume)
    {
        float dbVolume = Mathf.Clamp(volume, 0.0001f, 1f) > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("SFXVolume", dbVolume);
        if (!isSFXMuted)
        {
            lastSFXVolume = volume;
            PlayerPrefs.SetFloat(LAST_SFX_VOLUME_KEY, lastSFXVolume);
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"SFX Volume défini à : {volume} (db: {dbVolume})");
    }

    private void SetMusicVolume(float volume)
    {
        float dbVolume = Mathf.Clamp(volume, 0.0001f, 1f) > 0.0001f ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MusicVolume", dbVolume);
        if (!isMusicMuted)
        {
            lastMusicVolume = volume;
            PlayerPrefs.SetFloat(LAST_MUSIC_VOLUME_KEY, lastMusicVolume);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"Music Volume défini à : {volume} (db: {dbVolume})");
    }

    private void ToggleMasterMute()
    {
        isMasterMuted = !isMasterMuted;
        if (isMasterMuted)
        {
            lastMasterVolume = masterVolumeSlider.value;
            PlayerPrefs.SetFloat(LAST_MASTER_VOLUME_KEY, lastMasterVolume);
            SetMasterVolume(0f);
            masterVolumeSlider.value = 0f;
        }
        else
        {
            SetMasterVolume(lastMasterVolume);
            masterVolumeSlider.value = lastMasterVolume;
        }

        PlayerPrefs.SetInt(MASTER_MUTE_KEY, isMasterMuted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMuteButtonStates();
        Debug.Log($"Master Volume : {(isMasterMuted ? "Muté" : "Réactivé")}");
    }

    private void ToggleSFXMute()
    {
        isSFXMuted = !isSFXMuted;
        if (isSFXMuted)
        {
            lastSFXVolume = sfxVolumeSlider.value;
            PlayerPrefs.SetFloat(LAST_SFX_VOLUME_KEY, lastSFXVolume);
            SetSFXVolume(0f);
            sfxVolumeSlider.value = 0f;
        }
        else
        {
            SetSFXVolume(lastSFXVolume);
            sfxVolumeSlider.value = lastSFXVolume;
        }

        PlayerPrefs.SetInt(SFX_MUTE_KEY, isSFXMuted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMuteButtonStates();
        Debug.Log($"SFX Volume : {(isSFXMuted ? "Muté" : "Réactivé")}");
    }

    private void ToggleMusicMute()
    {
        isMusicMuted = !isMusicMuted;
        if (isMusicMuted)
        {
            lastMusicVolume = musicVolumeSlider.value;
            PlayerPrefs.SetFloat(LAST_MUSIC_VOLUME_KEY, lastMusicVolume);
            SetMusicVolume(0f);
            musicVolumeSlider.value = 0f;
        }
        else
        {
            SetMusicVolume(lastMusicVolume);
            musicVolumeSlider.value = lastMusicVolume;
        }

        PlayerPrefs.SetInt(MUSIC_MUTE_KEY, isMusicMuted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMuteButtonStates();
        Debug.Log($"Music Volume : {(isMusicMuted ? "Muté" : "Réactivé")}");
    }

    private void UpdateMuteButtonStates()
    {
        if (masterMuteButtonText != null)
        {
            masterMuteButtonText.text = isMasterMuted ? "Unmute" : "Mute";
        }
        if (sfxMuteButtonText != null)
        {
            sfxMuteButtonText.text = isSFXMuted ? "Unmute" : "Mute";
        }
        if (musicMuteButtonText != null)
        {
            musicMuteButtonText.text = isMusicMuted ? "Unmute" : "Mute";
        }
    }

    private void Vibrate()
    {
        Debug.Log("[Vibration] Tentative de déclenchement...");
#if UNITY_IOS || UNITY_ANDROID
        Debug.Log("[Vibration] Plateforme compatible détectée (Android/iOS).");
        if (!vibrationEnabled)
        {
            Debug.Log("[Vibration] Échec : vibrations désactivées (vibrationEnabled = false).");
            return;
        }

        // Méthode standard Unity
        Handheld.Vibrate();
        Debug.Log("[Vibration] Tentative avec Handheld.Vibrate()");

#if UNITY_ANDROID
        try
        {
            // Utiliser l'API native Android pour vibrer
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator != null && vibrator.Call<bool>("hasVibrator"))
                {
                    // Vibrer pendant 200ms (tu peux ajuster la durée)
                    vibrator.Call("vibrate", 200L);
                    Debug.Log("[Vibration] Vibration déclenchée via l'API native Android.");
                }
                else
                {
                    Debug.LogWarning("[Vibration] Aucun vibrateur détecté sur l'appareil via l'API native.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Vibration] Erreur lors de l'utilisation de l'API native Android : " + e.Message);
        }
#endif
#else
           Debug.LogWarning("[Vibration] Plateforme non compatible (pas Android/iOS).");
#endif
    }

    public void VibratePattern(params float[] delays)
    {
        if (!vibrationEnabled) return;
        StartCoroutine(VibrateRoutine(delays));
    }

    private IEnumerator VibrateRoutine(float[] delays)
    {
        foreach (var d in delays)
        {
#if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
#endif
            yield return new WaitForSeconds(d * vibrationIntensity);
        }
    }

    private void SetVibrationEnabled(bool enabled)
    {
        vibrationEnabled = enabled;
        PlayerPrefs.SetInt(VIBRATION_ENABLED_KEY, vibrationEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Vibrations : {(vibrationEnabled ? "Activées" : "Désactivées")}");
    }

    private void SetVibrationIntensity(float intensity)
    {
        vibrationIntensity = Mathf.Clamp01(intensity);
    }

    public bool IsVibrationEnabled() => vibrationEnabled;
    public float GetVibrationIntensity() => vibrationIntensity;

    private void OnVibrationToggleChanged(bool isOn)
    {
        SetVibrationEnabled(isOn);
        if (isOn)
        {
            Vibrate();
        }
    }
}