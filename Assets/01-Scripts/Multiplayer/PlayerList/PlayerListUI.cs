using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

/// <summary>
/// Gère l’affichage de tous les joueurs dans le lobby.
/// </summary>
public class PlayerListUI : MonoBehaviour
{
    [Header("Références")]

    [Tooltip("Prefab représentant un joueur.")]
    [SerializeField] private PlayerListItemUI playerItemPrefab;

    [Tooltip("Conteneur où seront instanciés les items.")]
    [SerializeField] private Transform contentRoot;

    [Tooltip("Texte affichant le nom du lobby.")]
    [SerializeField] private TMP_Text lobbyNameText;

    private readonly List<PlayerListItemUI> playerItems = new();

    private void OnEnable()
    {
        InvokeRepeating(nameof(Refresh), 0.5f, 2f); // polling toutes les 2s
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Refresh));
    }

    private void Refresh()
    {
        var lobby = SessionStore.Instance.CurrentLobby;
        if (lobby == null) return;

        // Affiche le nom du lobby
        if (lobbyNameText != null)
            lobbyNameText.text = lobby.Name;

        // Supprime les anciens items
        foreach (var item in playerItems)
        {
            Destroy(item.gameObject);
        }
        playerItems.Clear();

        // Ajoute les nouveaux joueurs
        foreach (var player in lobby.Players)
        {
            var item = Instantiate(playerItemPrefab, contentRoot);
            string playerName = player.Data != null && player.Data.ContainsKey("name")
                ? player.Data["name"].Value
                : "Joueur inconnu";
            item.SetPlayerName(playerName);
            playerItems.Add(item);
        }
    }
}
