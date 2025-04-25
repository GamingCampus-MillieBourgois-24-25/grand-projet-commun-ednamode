using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    private void Awake()
    {
        // V�rifie s'il existe d�j� une instance de SceneManager
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Une autre instance de SceneManager existe d�j�. Cette instance sera d�truite.");
            Destroy(gameObject);
            return;
        }

        // Assigne cette instance comme l'instance unique
        Instance = this;

        // Optionnel : Emp�che la destruction de cet objet lors du chargement d'une nouvelle sc�ne
        DontDestroyOnLoad(gameObject);
    }

    // M�thode pour charger une sc�ne
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Chargement de la sc�ne : {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
