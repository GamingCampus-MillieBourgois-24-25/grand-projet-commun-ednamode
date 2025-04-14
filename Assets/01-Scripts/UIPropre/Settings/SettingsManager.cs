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
    private int _nativeWidth; // R�solution native (largeur)
    private int _nativeHeight; // R�solution native (hauteur)
    private int _lastSetWidth = -1; // Derni�re largeur d�finie
    private int _lastSetHeight = -1; // Derni�re hauteur d�finie

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Stocker la r�solution native au d�marrage
        _nativeWidth = Screen.currentResolution.width;
        _nativeHeight = Screen.currentResolution.height;
        Debug.Log($"R�solution native d�tect�e : {_nativeWidth}x{_nativeHeight}");

        LoadQualitySettings();
    }

    public void SetQuality(QualityLevel level)
    {
        switch (level)
        {
            case QualityLevel.Low:
                QualitySettings.SetQualityLevel(0, true);
                Debug.Log("Qualit� d�finie sur : Low (optimis� pour mobile)");
                break;
            case QualityLevel.Medium:
                QualitySettings.SetQualityLevel(1, true);
                Debug.Log("Qualit� d�finie sur : Medium (optimis� pour mobile)");
                break;
            case QualityLevel.High:
                QualitySettings.SetQualityLevel(2, true);
                Debug.Log("Qualit� d�finie sur : High (optimis� pour mobile)");
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
        // Utiliser la r�solution native comme base pour tous les calculs
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

        // S'assurer que la r�solution est un minimum raisonnable (�viter 0 ou valeurs trop petites)
        targetWidth = Mathf.Max(targetWidth, 480); // R�solution minimale
        targetHeight = Mathf.Max(targetHeight, 800);

        // Appliquer la r�solution uniquement si elle a chang�
        if (targetWidth != _lastSetWidth || targetHeight != _lastSetHeight)
        {
            Screen.SetResolution(targetWidth, targetHeight, true);
            _lastSetWidth = targetWidth;
            _lastSetHeight = targetHeight;
            Debug.Log($"R�solution d�finie � : {targetWidth}x{targetHeight}");
        }
        else
        {
            Debug.Log("R�solution inchang�e, pas besoin de mise � jour.");
        }
    }
}