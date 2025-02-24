using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Netcode;

public class UIMultiplayerControls : NetworkBehaviour
{
    [Header("Panel contenant les boutons après connexion")]
    [SerializeField] private CanvasGroup multiplayerPanel; // Panel contenant les boutons "Joueurs" et "Chat"

    [Header("Boutons d'interaction")]
    [SerializeField] private Button playerListButton; // Bouton pour ouvrir la liste des joueurs
    [SerializeField] private Button chatButton; // Bouton pour ouvrir le chat

    [Header("Panels liés (Locaux)")]
    [SerializeField] private GameObject playerListPanel; // Panel Liste des joueurs (Local)
    [SerializeField] private GameObject chatPanel; // Panel Chat (Local)

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private Ease animationEase = Ease.OutBack;

    private bool isConnected = false;

    private void Awake()
    {
        if (multiplayerPanel == null)
        {
            Debug.LogError("[UIMultiplayerControls] Panel non assigné dans l’Inspector !");
            return;
        }

        // Cache le panel au démarrage
        multiplayerPanel.alpha = 0;
        multiplayerPanel.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Ajout des listeners sur les boutons
            if (playerListButton != null)
                playerListButton.onClick.AddListener(TogglePlayerList);
            else
                Debug.LogError("[UIMultiplayerControls] playerListButton non assigné !");

            if (chatButton != null)
                chatButton.onClick.AddListener(ToggleChat);
            else
                Debug.LogError("[UIMultiplayerControls] chatButton non assigné !");

            RequestUIActivationServerRpc();
        }
    }


    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    /// <summary>
    /// Appelé lorsqu'un joueur se connecte.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            isConnected = true;
            ShowMultiplayerPanel();
            RequestUIActivationServerRpc();
        }
    }

    /// <summary>
    /// Appelé lorsqu'un joueur se déconnecte.
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            isConnected = false;
            HideMultiplayerPanel();
        }
    }

    /// <summary>
    /// Demande au serveur d'activer l'UI sur tous les clients.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestUIActivationServerRpc()
    {
        ActivateUIClientRpc();
    }

    /// <summary>
    /// Active l'UI sur tous les clients.
    /// </summary>
    [ClientRpc]
    private void ActivateUIClientRpc()
    {
        if (IsOwner)
        {
            ShowMultiplayerPanel();
        }
    }

    /// <summary>
    /// Affiche le panel des boutons (avec animation).
    /// </summary>
    private void ShowMultiplayerPanel()
    {
        if (multiplayerPanel == null) return;

        multiplayerPanel.gameObject.SetActive(true);
        multiplayerPanel.DOFade(1, fadeDuration).SetEase(animationEase);
    }

    /// <summary>
    /// Cache le panel des boutons (avec animation).
    /// </summary>
    private void HideMultiplayerPanel()
    {
        if (multiplayerPanel == null) return;

        multiplayerPanel.DOFade(0, fadeDuration).OnComplete(() =>
        {
            multiplayerPanel.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// Ouvre le panel de la liste des joueurs et ferme le chat si ouvert.
    /// </summary>
    public void TogglePlayerList()
    {
        if (!isConnected) return;

        Debug.Log("[UIMultiplayerControls] TogglePlayerList() appelé.");

        bool isActive = playerListPanel.activeSelf;
        if (!isActive)
        {
            ClosePanel(chatPanel);
        }

        TogglePanel(playerListPanel);
    }

    /// <summary>
    /// Ouvre le panel du chat et ferme la liste des joueurs si ouverte.
    /// </summary>
    public void ToggleChat()
    {
        if (!isConnected) return;

        Debug.Log("[UIMultiplayerControls] ToggleChat() appelé.");

        bool isActive = chatPanel.activeSelf;
        if (!isActive)
        {
            ClosePanel(playerListPanel);
        }

        TogglePanel(chatPanel);
    }

    /// <summary>
    /// Gère l'affichage et l'animation d'un panel.
    /// </summary>
    private void TogglePanel(GameObject panel)
    {
        if (panel == null) return;

        bool isActive = panel.activeSelf;
        panel.SetActive(!isActive);

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        if (!isActive)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        canvasGroup.alpha = isActive ? 1 : 0;
        canvasGroup.DOFade(isActive ? 0 : 1, fadeDuration).SetEase(animationEase);
    }

    /// <summary>
    /// Ferme un panel avec une animation (utile pour fermer l'autre panel).
    /// </summary>
    private void ClosePanel(GameObject panel)
    {
        if (panel == null || !panel.activeSelf) return;

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroup.DOFade(0, fadeDuration).SetEase(animationEase).OnComplete(() =>
        {
            panel.SetActive(false);
        });
    }
}
