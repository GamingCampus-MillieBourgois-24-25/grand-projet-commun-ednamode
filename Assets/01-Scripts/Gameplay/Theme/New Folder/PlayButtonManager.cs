// ?? PLAY BUTTON MANAGER � Version compatible Lobby + Relay (Unity 6)
// Affiche le bouton Play uniquement pour le joueur "host" (1er du lobby)
// Et lance le jeu avec un th�me al�atoire synchronis�

using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class PlayButtonManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Bouton Play visible uniquement pour le host (1er joueur du lobby).")]
    [SerializeField] private Button playButton;

    [Tooltip("Nom de la sc�ne � charger lors du lancement de la partie.")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Tooltip("Zone de debug facultative.")]
    [SerializeField] private TMP_Text debugText;

    [Tooltip("Liste des th�mes disponibles pour la partie.")]
    [SerializeField] private ThemeData themeData;

    private void Start()
    {
        playButton.gameObject.SetActive(false);
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayClicked);

        TryEnablePlayButton();
    }

    private void TryEnablePlayButton()
    {
        var lobby = SessionStore.Instance?.CurrentLobby;
        if (lobby == null || lobby.Players == null || !AuthenticationService.Instance.IsSignedIn)
        {
            debugText?.SetText("Lobby non initialis�.");
            return;
        }

        string localPlayerId = AuthenticationService.Instance.PlayerId;
        string firstPlayerId = lobby.Players.FirstOrDefault()?.Id;

        if (firstPlayerId == localPlayerId)
        {
            playButton.gameObject.SetActive(true);
            debugText?.SetText("Tu es le host. Bouton Play activ�.");
        }
        else
        {
            playButton.gameObject.SetActive(false);
            debugText?.SetText("En attente que le host lance la partie...");
        }
    }

    private void OnPlayClicked()
    {
        // S�lection du th�me al�atoire
        string selectedTheme = themeData.themes[Random.Range(0, themeData.themes.Length)];

        // Synchronisation globale via un singleton ou RPC si besoin
        ThemeSyncManager.Instance.SetTheme(selectedTheme);

        // Changement de sc�ne pour tous
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
}