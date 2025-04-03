using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button playButton;

    [Header("Theme")]
    [SerializeField] private GameObject sessionThemeManagerPrefab;

    private ulong localClientId => NetworkManager.Singleton.LocalClientId;

    private void Start()
    {
        playButton.gameObject.SetActive(false);
        playButton.onClick.AddListener(OnPlayClicked);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != localClientId) return;

        ulong firstPlayerId = GetFirstPlayerId();

        if (localClientId == firstPlayerId)
        {
            Debug.Log($"[GameplayManager] Je suis le premier joueur ({localClientId}), je crée le ThemeManager.");
            SpawnThemeManager(localClientId);
            playButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log($"[GameplayManager] Joueur {localClientId} connecté (non premier).");
            playButton.gameObject.SetActive(false);
        }
    }

    private void OnPlayClicked()
    {
        if (SessionThemeManager.Instance == null)
        {
            Debug.LogWarning("[GameplayManager] Aucun SessionThemeManager disponible !");
            return;
        }

        if (SessionThemeManager.Instance.IsOwner)
        {
            Debug.Log("[GameplayManager] Lancement de la sélection du thème...");
            SessionThemeManager.Instance.AssignThemeIfNone();
        }
        else
        {
            Debug.LogWarning("[GameplayManager] Ce client ne possède pas le ThemeManager.");
        }
    }

    private ulong GetFirstPlayerId()
    {
        ulong lowestId = ulong.MaxValue;

        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (id != 0 && id < lowestId)
                lowestId = id;
        }

        return lowestId;
    }

    private void SpawnThemeManager(ulong ownerClientId)
    {
        if (SessionThemeManager.Instance != null)
        {
            Debug.LogWarning("[GameplayManager] ThemeManager existant trouvé. Suppression forcée...");

            if (SessionThemeManager.Instance.NetworkObject != null && SessionThemeManager.Instance.NetworkObject.IsSpawned)
            {
                SessionThemeManager.Instance.NetworkObject.Despawn(true);
            }
            else
            {
                Destroy(SessionThemeManager.Instance.gameObject);
            }
        }

        GameObject managerObj = Instantiate(sessionThemeManagerPrefab);
        NetworkObject netObj = managerObj.GetComponent<NetworkObject>();

        if (netObj != null)
        {
            netObj.SpawnAsPlayerObject(ownerClientId, true);
            Debug.Log("[GameplayManager] SessionThemeManager instancié et assigné.");
        }
        else
        {
            Debug.LogError("[GameplayManager] Le prefab SessionThemeManager n'a pas de NetworkObject !");
        }
    }
}
