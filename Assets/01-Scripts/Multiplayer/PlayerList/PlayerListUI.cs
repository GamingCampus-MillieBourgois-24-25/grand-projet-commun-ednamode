using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.Services.Authentication;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player;
using UnityEngine.UI;



public class PlayerListUI : MonoBehaviour
{
    [Header("Références UI")]
    [Tooltip("Transform du conteneur de la liste des joueurs (doit être un RectTransform)")]
    [SerializeField] private Transform container;
    [Tooltip("Prefab d’un nom de joueur (doit contenir un TMP_Text)")]
    [SerializeField] private GameObject playerNamePrefab;

    private Dictionary<string, GameObject> playerEntries = new();

    private void OnEnable()
    {
        RefreshPlayerList();
    }

    public void RefreshPlayerList()
    {
        // Supprime tous les anciens
        foreach (Transform child in container)
            Destroy(child.gameObject);

        playerEntries.Clear();

        if (SessionStore.Instance.CurrentLobby == null) return;

        List<LobbyPlayer> players = new(SessionStore.Instance.CurrentLobby.Players);

        // Tri avec host en premier
        players.Sort((a, b) =>
        {
            bool aIsHost = a.Id == SessionStore.Instance.CurrentLobby.HostId;
            bool bIsHost = b.Id == SessionStore.Instance.CurrentLobby.HostId;
            return bIsHost.CompareTo(aIsHost);
        });

        foreach (var player in players)
        {
            string playerId = player.Id;
            string name = SessionStore.Instance.GetPlayerName(player.Id);
            bool isHost = player.Id == SessionStore.Instance.CurrentLobby.HostId;
            bool isMe = player.Id == SessionHelper.GetLocalPlayerId();
            bool isLocalHost = SessionHelper.IsLocalPlayerHost();

            GameObject entry = Instantiate(playerNamePrefab, container);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
            playerEntries[player.Id] = entry;

            // Animation
            Transform animRoot = entry.transform.Find("AnimatedContainer");
            if (animRoot != null)
            {
                RectTransform anim = animRoot.GetComponent<RectTransform>();
                Vector2 start = new Vector2(-Screen.width, 0);
                anim.anchoredPosition = start;

                anim.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutBack);
            }

            // Texte
            TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = name;

            Transform hostPanel = entry.transform.Find("Host Panel");
            if (hostPanel != null)
            {
                // 👑 Affiche la couronne uniquement si CE joueur est le host (vu par tous)
                Transform crown = hostPanel.Find("Crown Image");
                if (crown != null)
                    crown.gameObject.SetActive(isHost);

                // 🔴 Affiche le bouton kick seulement si : 
                // le joueur est AUTRE que le host, et je suis le host local
                Button kickBtn = hostPanel.Find("Kick Button")?.GetComponent<Button>();
                if (kickBtn != null)
                {
                    bool shouldShowKick = isLocalHost && !isHost;

                    kickBtn.gameObject.SetActive(shouldShowKick);

                    if (shouldShowKick)
                    {
                        kickBtn.onClick.RemoveAllListeners();
                        kickBtn.onClick.AddListener(() =>
                        {
                            MultiplayerManager.Instance.KickPlayerById(playerId);
                        });
                    }
                }
            }
        }
    }

    public void AnimatePlayerLeave(string playerId)
    {
        if (!playerEntries.TryGetValue(playerId, out var entry)) return;

        RectTransform rect = entry.GetComponent<RectTransform>();

        // 🎞 Animation de sortie par la gauche
        rect.DOAnchorPosX(-Screen.width, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Destroy(entry);
                playerEntries.Remove(playerId);
            });
    }

    public void OnKickButtonClicked(string playerId)
    {
        MultiplayerManager.Instance?.KickPlayerById(playerId);
    }

}
