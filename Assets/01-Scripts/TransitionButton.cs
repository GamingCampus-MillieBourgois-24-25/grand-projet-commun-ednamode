using UnityEngine;
using UnityEngine.UI; // N�cessaire pour utiliser Button

public class TransitionButton : MonoBehaviour
{
    private Button button; // R�f�rence au bouton dans l'�diteur
    private SceneManager sceneManager;

    private void Start()
    {
        sceneManager = SceneManager.Instance;
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(() => OnButtonClick(""));
        }
        else
        {
            Debug.LogError("Le bouton n'est pas assign� dans l'inspecteur.");
        }
    }

    public void OnButtonClick(string sceneName)
    {
        sceneManager.LoadScene(sceneName);
    }
}
