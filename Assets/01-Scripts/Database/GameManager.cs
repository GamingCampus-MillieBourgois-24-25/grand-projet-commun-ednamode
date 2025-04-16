using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Text messageText;
    [SerializeField]
    private GameObject AreYouSurePanelMenu;

    [SerializeField]
    private GameObject AreYouSurePanelLogout;

    public TMP_Text userNameText;
    public TMP_Text userIdText;
    public TMP_Text levelText;
    public TMP_Text progressText;
    public Slider progressSlider;
    public TMP_Text coinsText;
    public TMP_Text jewelsText;

    void Start()
    {
        FillAccountData();
    }

    void FillAccountData()
    {
        var dts = DataSaver.Instance.dts;

        // Remplacement des anciens getters par des accès directs aux propriétés publiques
        userNameText.text = dts.userName;
        userIdText.text = DataSaver.Instance.GetUserId();

        levelText.text = $"Niveau : {dts.crrLevel}";
        progressText.text = $"Progression : {dts.crrLevelProgress}/{dts.totalLevelProgress}";
        progressSlider.value = (float)dts.crrLevelProgress / dts.totalLevelProgress;

        coinsText.text = dts.totalCoins.ToString();
        jewelsText.text = dts.totalJewels.ToString();
    }

    private void ShowMessage()
    {
        messageText.text = string.Format("Welcome, {0} In our game scene", References.userName);
    }

    public void ShowPanelMenu()
    {
        AreYouSurePanelMenu.SetActive(true);
    }
    public void HidePanelMenu()
    {
        AreYouSurePanelMenu.SetActive(false);
    }
    public void ShowPaneLogout()
    {
        AreYouSurePanelLogout.SetActive(true);
    }
    public void HidePaneLogout()
    {
        AreYouSurePanelLogout.SetActive(false);
    }
    public void BackToLogin()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("FirebaseLogin");
    }
    public void BackToMenu()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby_Horizontal v2");
    }

    public void GoToAccount()
    {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("AccountScene");

    }
    public void Logout()
    {
        if (FirebaseAuthManager.Instance != null)
        {
            FirebaseAuthManager.Instance.Logout();
        }
        else
        {
            Debug.LogError("FirebaseAuthManager.Instance est null. Assurez-vous que FirebaseAuthManager est chargé.");
        }
    }
}
