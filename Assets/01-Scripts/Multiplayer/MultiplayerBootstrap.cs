using UnityEngine;

public class MultiplayerBootstrap : MonoBehaviour
{
    [Header("Prefabs � instancier si n�cessaires")]
    [SerializeField] private GameObject multiplayerManagerPrefab;
    [SerializeField] private GameObject sessionStorePrefab;

    private void Awake()
    {
        SetupSingleton<MultiplayerManager>(multiplayerManagerPrefab, "MultiplayerManager");
        SetupSingleton<SessionStore>(sessionStorePrefab, "SessionStore");

        DontDestroyOnLoad(gameObject);
    }

    private void SetupSingleton<T>(GameObject prefab, string objectName) where T : MonoBehaviour
    {
        if (FindObjectOfType<T>() != null)
            return;

        GameObject obj = Instantiate(prefab);
        obj.name = objectName;
        DontDestroyOnLoad(obj);
    }
}
