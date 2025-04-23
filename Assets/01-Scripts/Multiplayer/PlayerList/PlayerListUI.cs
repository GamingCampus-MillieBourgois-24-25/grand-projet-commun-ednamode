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
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);

        playerEntries.Clear();

        if (SessionStore.Instance.CurrentLobby == null)
        {
            Debug.LogWarning("[PlayerListUI] Impossible de rafraîchir la liste des joueurs.");
            return;
        }

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

            // Animation d’entrée
            Transform animRoot = entry.transform.Find("AnimatedContainer");
            if (animRoot != null)
            {
                RectTransform anim = animRoot.GetComponent<RectTransform>();
                Vector2 start = new Vector2(-Screen.width, 0);
                anim.anchoredPosition = start;

                anim.DOAnchorPosX(0, 0.4f).SetEase(Ease.OutBack);
            }

            // Texte du pseudo
            TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = name;

            // Icône de host / bouton Kick
            Transform hostPanel = entry.transform.Find("Host Panel");
            if (hostPanel != null)
            {
                Transform crown = hostPanel.Find("Crown Image");
                if (crown != null)
                    crown.gameObject.SetActive(isHost);

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

            // ✅ ➕ ICI : applique l'état ready réel du joueur
            bool isReady = MultiplayerManager.Instance.IsPlayerReady(playerId);
            MarkPlayerReady(playerId, isReady);
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

    public void MarkPlayerReady(string playerId, bool isReady)
    {
        if (!playerEntries.TryGetValue(playerId, out var entry))
        {
            Debug.LogWarning($"[UI] Aucun entry trouvé pour playerId: {playerId}");
            return;
        }

        // 🔍 Cherche la gélule d’état
        var fill = entry.transform.Find("AnimatedContainer/ReadyPill/Fill")?.GetComponent<Image>();
        if (fill == null)
        {
            Debug.LogWarning("[UI] Aucun composant 'Fill' trouvé pour ReadyPill.");
            return;
        }

        fill.DOKill(); // stoppe les animations en cours

        if (isReady)
        {
            fill.fillAmount = 0f;
            fill.DOFillAmount(1f, 0.4f).SetEase(Ease.OutQuad);
        }
        else
        {
            fill.DOFillAmount(0f, 0.4f).SetEase(Ease.InQuad);
        }
    }
}
