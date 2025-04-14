using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Text messageText;
    [SerializeField]
    private GameObject AreYouSurePanel;

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

        userNameText.text = dts.GetUserName();
        userIdText.text = DataSaver.Instance.GetUserId();

        levelText.text = $"Niveau : {dts.GetCrrLevel()}";
        progressText.text = $"Progression : {dts.GetCrrLevelProgress()}/{dts.GetTotalLevelProgress()}";
        progressSlider.value = (float)dts.GetCrrLevelProgress() / dts.GetTotalLevelProgress();

        coinsText.text = dts.GetTotalCoins().ToString();
        jewelsText.text = dts.GetTotalJewels().ToString();
    }

    private void ShowMessage()
    {
        messageText.text = string.Format("Welcome, {0} In our game scene", References.userName);
    }

    public void ShowPanel()
    {
        AreYouSurePanel.SetActive(true);
    }
    public void HidePanel()
    {
        AreYouSurePanel.SetActive(false);
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
}
