using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Boutons de qualit�")]
    public Button highQualityButton;
    public Button mediumQualityButton;
    public Button lowQualityButton;

    [Header("Feedback visuel (TextMeshPro)")]
    public TextMeshProUGUI qualityFeedbackText;

    [Header("Panneau de param�tres")]
    public GameObject settingsPanel;

    [Header("R�f�rence au SettingsManager")]
    public SettingsManager settingsManager;

    private void Start()
    {
        if (settingsManager == null)
        {
            return;
        }

   /*     if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }*/

        if (highQualityButton != null)
        {
            highQualityButton.onClick.AddListener(() => SetQuality(SettingsManager.QualityLevel.High));
        }
        if (mediumQualityButton != null)
        {
            mediumQualityButton.onClick.AddListener(() => SetQuality(SettingsManager.QualityLevel.Medium));
        }
        if (lowQualityButton != null)
        {
            lowQualityButton.onClick.AddListener(() => SetQuality(SettingsManager.QualityLevel.Low));
        }

        UpdateButtonStates();
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool newState = !settingsPanel.activeSelf;
            settingsPanel.SetActive(newState);
        }
    }

    private void SetQuality(SettingsManager.QualityLevel level)
    {
        settingsManager.SetQuality(level);
        UpdateButtonStates();

        if (qualityFeedbackText != null)
        {
            qualityFeedbackText.text = $"Qualit� d�finie sur : {level}\n" + "";
            Invoke(nameof(ClearFeedbackText), 3f);
        }
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
        SettingsManager.QualityLevel currentLevel = settingsManager.GetCurrentQuality();

        if (highQualityButton != null)
        {
            highQualityButton.interactable = (currentLevel != SettingsManager.QualityLevel.High);
        }
        if (mediumQualityButton != null)
        {
            mediumQualityButton.interactable = (currentLevel != SettingsManager.QualityLevel.Medium);
        }
        if (lowQualityButton != null)
        {
            lowQualityButton.interactable = (currentLevel != SettingsManager.QualityLevel.Low);
        }
    }
}