using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{
    [Header("Lobby UI")]
    public TMP_Text coinsLobbyText;
    public TMP_Text jewelsLobbyText;
    public TMP_Text levelLobbyText;
    public TMP_Text levelProgressText;
    public Image imageFillwithProgress;



    [Header("Profile Button")]
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
    public TMP_Text coinsText;
    public TMP_Text jewelsText;

    void Start()
    {
        FillAccountData();
        UpdateCoinAndJewelLobby();
        UpdateLevelLobby();
    }


    void UpdateCoinAndJewelLobby()
    {
        // Récupération des données de la sauvegarde
        var dts = DataSaver.Instance.dts;
        // Mise à jour des textes dans le lobby
        coinsLobbyText.text = dts.totalCoins.ToString();
        jewelsLobbyText.text = dts.totalJewels.ToString();
    }

    void UpdateLevelLobby()
    {
        // Récupération des données de la sauvegarde
        var dts = DataSaver.Instance.dts;
        // Mise à jour des textes dans le lobby
        levelLobbyText.text = dts.crrLevel.ToString();
        levelProgressText.text = dts.crrLevelProgress.ToString() + "exp / " + dts.totalLevelProgress.ToString()+ "exp";
        imageFillwithProgress.fillAmount = (float)dts.crrLevelProgress / (float)dts.totalLevelProgress;
    }

    void FillAccountData()
    {
        var dts = DataSaver.Instance.dts;

        // Remplacement des anciens getters par des accès directs aux propriétés publiques
        userNameText.text = "UserName : " + dts.userName;
        userIdText.text = "ID : " + DataSaver.Instance.GetUserId();

        levelText.text = $"Niveau : {dts.crrLevel}";
        progressText.text = $"Progression : {dts.crrLevelProgress}/{dts.totalLevelProgress}";

        coinsText.text = "coins : " + dts.totalCoins.ToString();
        jewelsText.text = "jewels : " + dts.totalJewels.ToString();
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

    #region SceneManagement

    public void loadShop()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Melvin_Shop");
    }
    public void loadPass()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattlePassDemoScene");
    }
    #endregion
}
