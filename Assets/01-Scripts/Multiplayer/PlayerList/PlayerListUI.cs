using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class PlayerListUI : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private Transform container;
    [SerializeField] private GameObject playerNamePrefab;

    private Dictionary<string, GameObject> playerEntries = new();

    private void OnEnable()
    {
        RefreshPlayerList();
    }

    public void RefreshPlayerList()
    {
        // Nettoyage
        foreach (Transform child in container)
            Destroy(child.gameObject);

        playerEntries.Clear();

        var lobby = MultiplayerManager.Instance?.CurrentLobby;
        if (lobby == null) return;

        string hostId = lobby.HostId;

        // Trie : Host en haut, puis noms alphabétiques
        var sortedPlayers = new List<Player>(lobby.Players);
        sortedPlayers.Sort((a, b) =>
        {
            if (a.Id == hostId) return -1;
            if (b.Id == hostId) return 1;

            string nameA = a.Data.TryGetValue("name", out var dA) ? dA.Value : a.Id;
            string nameB = b.Data.TryGetValue("name", out var dB) ? dB.Value : b.Id;
            return nameA.CompareTo(nameB);
        });

        foreach (var player in sortedPlayers)
        {
            string playerId = player.Id;
            string name = player.Data.TryGetValue("name", out var data) ? data.Value : $"Player_{player.Id}";

            GameObject entry = Instantiate(playerNamePrefab, container);

            TMP_Text[] texts = entry.GetComponentsInChildren<TMP_Text>();
            TMP_Text nameText = texts[0];
            TMP_Text statusText = texts.Length > 1 ? texts[1] : null;

            bool isHost = playerId == hostId;
            nameText.text = isHost ? $"👑 {name}" : name;
            nameText.fontStyle = isHost ? FontStyles.Bold : FontStyles.Normal;

            if (statusText != null)
            {
                bool isReady = MultiplayerManager.Instance.IsPlayerReady(playerId);

                statusText.text = isReady
                    ? "<color=#4CAF50>✅ Ready</color>"
                    : "<color=#F44336>❌ Not Ready</color>";

                statusText.fontStyle = isReady ? FontStyles.Bold : FontStyles.Italic;
            }

            playerEntries[playerId] = entry;

            // 💫 Animation d’apparition
            CanvasGroup cg = entry.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            entry.transform.localScale = Vector3.zero;

            cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
            entry.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
    }
}
