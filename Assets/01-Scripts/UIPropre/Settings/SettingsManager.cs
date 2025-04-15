using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }

    private const string QUALITY_KEY = "QualityLevel";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LoadQualitySettings();
    }

    public void SetQuality(QualityLevel level)
    {
        switch (level)
        {
            case QualityLevel.Low:
                QualitySettings.SetQualityLevel(0, true);
                Debug.Log("Qualité définie sur : Low (optimisé pour mobile)");
                break;
            case QualityLevel.Medium:
                QualitySettings.SetQualityLevel(1, true);
                Debug.Log("Qualité définie sur : Medium (optimisé pour mobile)");
                break;
            case QualityLevel.High:
                QualitySettings.SetQualityLevel(2, true);
                Debug.Log("Qualité définie sur : High (optimisé pour mobile)");
                break;
        }

        ApplyMobileOptimizations(level);

        PlayerPrefs.SetInt(QUALITY_KEY, (int)level);
        PlayerPrefs.Save();
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
            SetQuality(QualityLevel.Low);
        }
    }

    public QualityLevel GetCurrentQuality()
    {
        int currentLevel = QualitySettings.GetQualityLevel();
        switch (currentLevel)
        {
            case 0: return QualityLevel.Low;
            case 1: return QualityLevel.Medium;
            case 2: return QualityLevel.High;
            default: return QualityLevel.Low;
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
}