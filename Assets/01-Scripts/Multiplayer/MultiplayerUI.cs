using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Type = NotificationData.NotificationType;

/// <summary>
/// MULTIPLAYER UI – Connecte les boutons à MultiplayerManager
/// Gère la création, la connexion et la déconnexion de sessions.
/// </summary>
public class MultiplayerUI : MonoBehaviour
{
    [Header("Entrées utilisateur")]

    [Tooltip("Champ pour entrer le nom du lobby à créer.")]
    [SerializeField] private TMP_InputField inputLobbyName;

    [Tooltip("Champ pour entrer un code pour rejoindre un lobby.")]
    [SerializeField] private TMP_InputField inputJoinCode;

    [Header("Boutons")]

    [Tooltip("Bouton pour créer un lobby avec le nom spécifié.")]
    [SerializeField] private Button buttonCreate;

    [Tooltip("Bouton pour rejoindre un lobby via un code.")]
    [SerializeField] private Button buttonJoin;

    [Tooltip("Bouton pour rejoindre automatiquement un lobby ouvert.")]
    [SerializeField] private Button buttonQuickJoin;

    [Tooltip("Bouton pour quitter le lobby actuel.")]
    [SerializeField] private Button buttonLeave;

    [Header("Feedback UI")]

    [Tooltip("Texte affichant les retours d'état ou erreurs à l'utilisateur.")]
    [SerializeField] private TMP_Text feedbackText;

    private void Awake()
    {
        buttonCreate.onClick.AddListener(OnCreateClicked);
        buttonJoin.onClick.AddListener(OnJoinClicked);
        buttonQuickJoin.onClick.AddListener(OnQuickJoinClicked);
        buttonLeave.onClick.AddListener(OnLeaveClicked);
    }

    private void Start()
    {
        StartCoroutine(WaitForMultiplayerReady());
    }

    /// <summary>
    /// Attend que le MultiplayerManager soit prêt avant d’activer les boutons.
    /// </summary>
    private System.Collections.IEnumerator WaitForMultiplayerReady()
    {
        while (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsReady)
        {
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("MultiplayerUI: MultiplayerManager prêt, activation des boutons.");

        buttonCreate.interactable = true;
        buttonJoin.interactable = true;
        buttonQuickJoin.interactable = true;
    }

    private void OnCreateClicked()
    {
        string lobbyName = string.IsNullOrEmpty(inputLobbyName.text) ? "FashionSession" : inputLobbyName.text;
        MultiplayerManager.Instance.CreateLobby(lobbyName);
        NotificationManager.Instance.ShowNotification("Create Lobby...", Type.Normal);
    }

    private void OnJoinClicked()
    {
        string code = inputJoinCode.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            NotificationManager.Instance.ShowNotification("Invalid Code", Type.Important);
            return;
        }

        MultiplayerManager.Instance.JoinLobbyByCode(code);
        NotificationManager.Instance.ShowNotification("Joining : " + code, Type.Normal);
    }

    private void OnQuickJoinClicked()
    {
        MultiplayerManager.Instance.QuickJoin();
        NotificationManager.Instance.ShowNotification("Search Lobby...", Type.Normal);
    }

    private void OnLeaveClicked()
    {
        MultiplayerManager.Instance.LeaveLobby();
        NotificationManager.Instance.ShowNotification("Disconnect...", Type.Normal);
    }

    /// <summary>
    /// Affiche un message dans l'interface et la console.
    /// </summary>
    /// <param name="message">Message à afficher</param>
    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }

        Debug.Log("[UI] " + message);
    }
}
