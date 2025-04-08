using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Toggle Options")]
    public Toggle soundToggle;
    public Toggle vibrationToggle;
    public TMP_Dropdown languageDropdown;

    private void Start()
    {
        // Chargement des préférences
        soundToggle.isOn = PlayerPrefs.GetInt("sound", 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt("vibrate", 1) == 1;
        languageDropdown.value = PlayerPrefs.GetInt("language", 0);

        // Enregistrement au changement
        soundToggle.onValueChanged.AddListener(val => PlayerPrefs.SetInt("sound", val ? 1 : 0));
        vibrationToggle.onValueChanged.AddListener(val => PlayerPrefs.SetInt("vibrate", val ? 1 : 0));
        languageDropdown.onValueChanged.AddListener(val => PlayerPrefs.SetInt("language", val));
    }
}
