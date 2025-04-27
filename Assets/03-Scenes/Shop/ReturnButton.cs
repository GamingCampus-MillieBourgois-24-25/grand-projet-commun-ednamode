using UnityEngine;
public class ReturnButton : MonoBehaviour
{
    private UIManager uiManager;
    private void Start()
    {
        uiManager = Object.FindFirstObjectByType<UIManager>();
    }

    public void OnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby_Horizontal v2");
    }
}
