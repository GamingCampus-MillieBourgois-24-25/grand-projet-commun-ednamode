using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SessionThemeManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI themeTextUI;
    [SerializeField] private ThemeData themeData;

    private readonly NetworkVariable<int> selectedThemeIndex = new NetworkVariable<int>(
        -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    public static SessionThemeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (themeTextUI != null)
            themeTextUI.gameObject.SetActive(false);
    }

public override void OnNetworkDespawn()
{
    Debug.Log("[SessionThemeManager] NetworkDespawn appelé. Nettoyage en cours...");
    Cleanup();
}

    private void OnDestroy()
    {
        selectedThemeIndex.OnValueChanged -= OnThemeChanged;

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        Cleanup();
    }

    private void Cleanup()
    {
        if (themeTextUI != null)
        {
            themeTextUI.text = string.Empty;
            themeTextUI.gameObject.SetActive(false);
            Debug.Log("[SessionThemeManager] UI nettoyée.");
        }

        if (Instance == this)
            Instance = null;

        selectedThemeIndex.Value = -1;
    }


    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        // Si le propriétaire du ThemeManager quitte
        if (clientId == OwnerClientId)
        {
            Debug.Log("[SessionThemeManager] Le propriétaire a quitté. Suppression du ThemeManager.");

            // Réinitialiser l’état du thème
            selectedThemeIndex.Value = -1;

            if (NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true); // Supprime l'objet pour tous les clients
            }
        }
    }


    private void OnThemeChanged(int oldValue, int newValue)
    {
        if (newValue >= 0 && newValue < themeData.themes.Length)
        {
            string theme = themeData.themes[newValue];
            Debug.Log($"[SessionThemeManager] Thème synchronisé : {theme}");

            if (themeTextUI != null)
            {
                themeTextUI.text = $"Thème sélectionné : {theme}";
                themeTextUI.gameObject.SetActive(true);
            }
        }
        else
        {
            if (themeTextUI != null)
            {
                Debug.Log("[SessionThemeManager] Aucune sélection active. Cacher le texte.");
                themeTextUI.gameObject.SetActive(false);
            }
        }
    }

    public void AssignThemeIfNone()
    {
        if (!IsOwner)
        {
            Debug.LogWarning("[SessionThemeManager] Tentative de sélection par un non-owner.");
            return;
        }

        if (selectedThemeIndex.Value != -1)
        {
            Debug.Log("[SessionThemeManager] Le thème a déjà été défini.");
            return;
        }

        if (themeData.themes.Length == 0)
        {
            Debug.LogError("[SessionThemeManager] Aucun thème défini dans ThemeData.");
            return;
        }

        int randomIndex = Random.Range(0, themeData.themes.Length);
        selectedThemeIndex.Value = randomIndex;
        Debug.Log($"[SessionThemeManager] Thème sélectionné localement : {themeData.themes[randomIndex]}");
    }
}
