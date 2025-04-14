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
    private int _nativeWidth; // Résolution native (largeur)
    private int _nativeHeight; // Résolution native (hauteur)
    private int _lastSetWidth = -1; // Dernière largeur définie
    private int _lastSetHeight = -1; // Dernière hauteur définie

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Stocker la résolution native au démarrage
        _nativeWidth = Screen.currentResolution.width;
        _nativeHeight = Screen.currentResolution.height;
        Debug.Log($"Résolution native détectée : {_nativeWidth}x{_nativeHeight}");

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
        // Utiliser la résolution native comme base pour tous les calculs
        int targetWidth;
        int targetHeight;

        switch (level)
        {
            case QualityLevel.Low:
                targetWidth = _nativeWidth / 2;
                targetHeight = _nativeHeight / 2;
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                Application.targetFrameRate = 30;
                break;
            case QualityLevel.Medium:
                targetWidth = _nativeWidth * 3 / 4;
                targetHeight = _nativeHeight * 3 / 4;
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = true;
                Application.targetFrameRate = 60;
                break;
            case QualityLevel.High:
                targetWidth = _nativeWidth;
                targetHeight = _nativeHeight;
                QualitySettings.antiAliasing = 2;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.billboardsFaceCameraPosition = true;
                Application.targetFrameRate = 60;
                break;
            default:
                targetWidth = _nativeWidth / 2;
                targetHeight = _nativeHeight / 2;
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                Application.targetFrameRate = 30;
                break;
        }

        // S'assurer que la résolution est un minimum raisonnable (éviter 0 ou valeurs trop petites)
        targetWidth = Mathf.Max(targetWidth, 480); // Résolution minimale
        targetHeight = Mathf.Max(targetHeight, 800);

        // Appliquer la résolution uniquement si elle a changé
        if (targetWidth != _lastSetWidth || targetHeight != _lastSetHeight)
        {
            Screen.SetResolution(targetWidth, targetHeight, true);
            _lastSetWidth = targetWidth;
            _lastSetHeight = targetHeight;
            Debug.Log($"Résolution définie à : {targetWidth}x{targetHeight}");
        }
        else
        {
            Debug.Log("Résolution inchangée, pas besoin de mise à jour.");
        }
    }
}