using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    private void Awake()
    {
        // Vérifie s'il existe déjà une instance de SceneManager
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Une autre instance de SceneManager existe déjà. Cette instance sera détruite.");
            Destroy(gameObject);
            return;
        }

        // Assigne cette instance comme l'instance unique
        Instance = this;

        // Optionnel : Empêche la destruction de cet objet lors du chargement d'une nouvelle scène
        DontDestroyOnLoad(gameObject);
    }

    // Méthode pour charger une scène
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Chargement de la scène : {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
