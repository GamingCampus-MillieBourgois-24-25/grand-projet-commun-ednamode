using UnityEngine;
using UnityEngine.UI; // Nécessaire pour utiliser Button

public class TransitionButton : MonoBehaviour
{
    private Button button; // Référence au bouton dans l'éditeur
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
            Debug.LogError("Le bouton n'est pas assigné dans l'inspecteur.");
        }
    }

    public void OnButtonClick(string sceneName)
    {
        sceneManager.LoadScene(sceneName);
    }
}
