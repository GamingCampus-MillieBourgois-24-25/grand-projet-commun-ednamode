using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private Text messageText;
    [SerializeField]
    private GameObject AreYouSurePanel;

    // Start is called before the first frame update
    void Start()
    {
        ShowMessage();   
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

}
