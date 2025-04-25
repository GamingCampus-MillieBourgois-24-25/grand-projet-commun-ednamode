using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }

    [Header("Dropdown de qualité")]
    public TMP_Dropdown qualityDropdown;

    [Header("Sliders de volume")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Boutons de mute")]
    public Button masterMuteButton;
    public Button sfxMuteButton;
    public Button musicMuteButton;

    [Header("Icônes de mute")]
    [SerializeField] private Sprite muteIcon; 
    [SerializeField] private Sprite unmuteIcon; 

    [Header("Référence à l'Audio Mixer")]
    public AudioMixer audioMixer;

    private const string QUALITY_KEY = "QualityLevel";
    private const string MASTER_MUTE_KEY = "IsMasterMuted";
    private const string SFX_MUTE_KEY = "IsSFXMuted";
    private const string MUSIC_MUTE_KEY = "IsMusicMuted";
    private const string LAST_MASTER_VOLUME_KEY = "LastMasterVolume";
    private const string LAST_SFX_VOLUME_KEY = "LastSFXVolume";
    private const string LAST_MUSIC_VOLUME_KEY = "LastMusicVolume";

    private bool isMasterMuted = false;
    private bool isSFXMuted = false;
    private bool isMusicMuted = false;
    private float lastMasterVolume = 1f;
    private float lastSFXVolume = 1f;
    private float lastMusicVolume = 1f;

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

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            List<string> options = new List<string> { "High", "Medium", "Low" };
            qualityDropdown.AddOptions(options);

            // Définir la valeur initiale en fonction du niveau de qualité actuel
            QualityLevel currentQuality = GetCurrentQuality();
            qualityDropdown.value = (int)currentQuality; // High=0, Medium=1, Low=2

            qualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
        }

        // Configuration des sliders de volume 
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

        // Charger les états de mute 
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

        // Configuration des boutons de mute
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
        UpdateDropdownState();
    }

    // Gérer les changements dans le Dropdown
    private void OnQualityDropdownChanged(int index)
    {
        QualityLevel selectedQuality = (QualityLevel)index;
        SetQuality(selectedQuality);
    }

    public void SetQuality(QualityLevel level)
    {
        switch (level)
        {
            case QualityLevel.Low:
                QualitySettings.SetQualityLevel(2, true);
                break;
            case QualityLevel.Medium:
                QualitySettings.SetQualityLevel(1, true);
                break;
            case QualityLevel.High:
                QualitySettings.SetQualityLevel(0, true);
                break;
        }

        ApplyMobileOptimizations(level);

        PlayerPrefs.SetInt(QUALITY_KEY, (int)level);
        PlayerPrefs.Save();

        UpdateDropdownState();
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
                QualitySettings.pixelLightCount = 0; // Réduire les lumières en temps réel
                QualitySettings.vSyncCount = 0; // Désactiver VSync pour éviter les blocages
                QualitySettings.lodBias = 0.3f; // Réduire la distance des LOD
                Application.targetFrameRate = 30; // 30 FPS pour faible performance
                Screen.sleepTimeout = SleepTimeout.SystemSetting; // Éviter l'écran en veille
                break;
            case QualityLevel.Medium:
                QualitySettings.antiAliasing = 0; // Anti-aliasing coûteux sur mobile
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = true;
                QualitySettings.pixelLightCount = 1;
                QualitySettings.vSyncCount = 0;
                QualitySettings.lodBias = 0.5f;
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
                break;
            case QualityLevel.High:
                QualitySettings.antiAliasing = 2; // Activer seulement sur appareils haut de gamme
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.billboardsFaceCameraPosition = true;
                QualitySettings.pixelLightCount = 2;
                QualitySettings.vSyncCount = 0;
                QualitySettings.lodBias = 0.7f;
                Application.targetFrameRate = 60;
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
                break;
            default:
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.vSyncCount = 0;
                QualitySettings.lodBias = 0.3f;
                Application.targetFrameRate = 30;
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
                break;
        }
    }

    private void UpdateDropdownState()
    {
        if (qualityDropdown != null)
        {
            QualityLevel currentLevel = GetCurrentQuality();
            qualityDropdown.value = (int)currentLevel;
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
        // Mettre à jour l'icône et le texte pour Master
        if (masterMuteButton != null)
        {
            Image buttonImage = masterMuteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isMasterMuted ? muteIcon : unmuteIcon;
                buttonImage.enabled = buttonImage.sprite != null; // Désactiver si pas de sprite
            }
            else
            {
                Debug.LogWarning("Aucun composant Image trouvé sur masterMuteButton.");
            }
        }
       

        // Mettre à jour l'icône et le texte pour SFX
        if (sfxMuteButton != null)
        {
            Image buttonImage = sfxMuteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isSFXMuted ? muteIcon : unmuteIcon;
                buttonImage.enabled = buttonImage.sprite != null;
            }
            else
            {
                Debug.LogWarning("Aucun composant Image trouvé sur sfxMuteButton.");
            }
        }
       

        // Mettre à jour l'icône et le texte pour Music
        if (musicMuteButton != null)
        {
            Image buttonImage = musicMuteButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isMusicMuted ? muteIcon : unmuteIcon;
                buttonImage.enabled = buttonImage.sprite != null;
            }
            else
            {
                Debug.LogWarning("Aucun composant Image trouvé sur musicMuteButton.");
            }
        }
        
    }
}