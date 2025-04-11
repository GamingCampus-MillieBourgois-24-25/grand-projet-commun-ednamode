using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;
using DG.Tweening;
using TMPro;

using Type = NotificationData.NotificationType;

/// <summary>
/// MULTIPLAYER UI – Connecte les boutons à MultiplayerManager
/// Gère la création, la connexion et la déconnexion de sessions.
/// </summary>
public class MultiplayerUI : MonoBehaviour
{
    [Header("Entrées utilisateur")]
    [SerializeField] private TMP_InputField inputLobbyName;
    [SerializeField] private TMP_InputField inputJoinCode;

    [Header("Boutons")]
    [SerializeField] private Button buttonCreate;
    [SerializeField] private Button buttonJoin;
    [SerializeField] private Button buttonQuickJoin;
    [SerializeField] private Button buttonLeave;

    [Header("GameMode")]
    [Header("Style Ready")]
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.red;
    [SerializeField] private string readyText = "Ready";
    [SerializeField] private string notReadyText = "Not Ready";
    [SerializeField] private TMP_Text readyCountText;
    [SerializeField] private Button buttonReady;
    [SerializeField] private Button[] gameModeButtons; // 3 boutons

    [Header("Feedback UI")]
    [Tooltip("Affiche le code de session en cours")]
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Activer quand connecté")]
    [SerializeField] private GameObject[] showOnConnected;

    [Header("Masquer quand connecté")]
    [SerializeField] private GameObject[] hideOnConnected;

    private bool isReady = false;

    private void Awake()
    {
        buttonCreate.onClick.AddListener(OnCreateClicked);
        buttonJoin.onClick.AddListener(OnJoinClicked);
        buttonQuickJoin.onClick.AddListener(OnQuickJoinClicked);
        buttonLeave.onClick.AddListener(OnLeaveClicked);

        inputLobbyName.onValueChanged.AddListener(OnInputChanged);
        inputJoinCode.onValueChanged.AddListener(OnInputChanged);

        buttonCreate.interactable = false;
        buttonJoin.interactable = false;
    }

    private void Start()
    {
        StartCoroutine(WaitForMultiplayerReady());

        for (int i = 0; i < gameModeButtons.Length; i++)
        {
            int mode = i;
            gameModeButtons[i].onClick.AddListener(() =>
            {
                MultiplayerManager.Instance.SelectGameMode(mode);
                UpdateGameModeButtonVisuals(mode);
                NotificationManager.Instance.ShowNotification($"GameMode {mode + 1} selected", Type.Normal);
            });
        }

        UpdateHostUI();

        buttonReady.onClick.AddListener(() =>
        {
            isReady = !isReady;
            MultiplayerManager.Instance.SetReady(isReady);
            UpdateReadyButtonUI();
        });

        // 🔁 Synchronise également à l’ouverture si data déjà dispo
        if (MultiplayerNetwork.Instance != null)
        {
            UpdateReadyCount(
                MultiplayerNetwork.Instance.ReadyCount.Value,
                MultiplayerNetwork.Instance.PlayerCount.Value
            );
        }

        UpdateReadyButtonUI();
    }

    private IEnumerator WaitForMultiplayerReady()
    {
        while (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsReady)
        {
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("MultiplayerUI: MultiplayerManager prêt, activation des boutons.");

        buttonQuickJoin.interactable = true;
        buttonLeave.interactable = true;

        OnInputChanged("");
    }

    private void OnInputChanged(string _)
    {
        buttonCreate.interactable = !string.IsNullOrWhiteSpace(inputLobbyName.text);
        buttonJoin.interactable = !string.IsNullOrWhiteSpace(inputJoinCode.text);
    }

    private void OnCreateClicked()
    {
        string lobbyName = inputLobbyName.text.Trim();
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
        NotificationManager.Instance.ShowNotification("Joining : " + code, Type.Info);
    }

    private void OnQuickJoinClicked()
    {
        MultiplayerManager.Instance.QuickJoin();
        NotificationManager.Instance.ShowNotification("Searching for Lobby...", Type.Info);
    }

    private void OnLeaveClicked()
    {
        MultiplayerManager.Instance.LeaveLobby();
        NotificationManager.Instance.ShowNotification("Disconnect...", Type.Info);
        UpdateConnectionUI(false);
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }

        Debug.Log("[UI] " + message);
    }

    public void NotifyCreateResult(bool success)
    {
        if (success)
        {
            NotificationManager.Instance.ShowNotification("Create Success", Type.Info);
            UpdateConnectionUI(true);
            UpdateHostUI();
        }
        else
        {
            NotificationManager.Instance.ShowNotification("Create Failed", Type.Important);
            UpdateConnectionUI(false);
        }
    }


    public void NotifyJoinResult(bool success)
    {
        if (success)
        {
            NotificationManager.Instance.ShowNotification("Join Success", Type.Info);
            UpdateConnectionUI(true);
            UpdateHostUI();
        }
        else
        {
            NotificationManager.Instance.ShowNotification("Join Failed", Type.Important);
            UpdateConnectionUI(false);
        }
    }

    public void NotifyNoLobbyFound()
    {
        NotificationManager.Instance.ShowNotification("No Lobby found", Type.Important);
        UpdateConnectionUI(false);
    }

    public void UpdateJoinCode(string code)
    {
        if (joinCodeText != null)
        {
            joinCodeText.text = string.IsNullOrEmpty(code) ? "" : $"Code : {code}";
        }
    }

    public void UpdateConnectionUI(bool connected)
    {
        foreach (var go in showOnConnected)
        {
            if (!go) continue;

            if (connected)
            {
                AnimateShow(go);
            }
            else
            {
                AnimateHide(go);
            }
        }

        foreach (var go in hideOnConnected)
        {
            if (!go) continue;

            if (connected)
            {
                AnimateHide(go);
            }
            else
            {
                AnimateShow(go);
            }
        }
    }

    private void AnimateShow(GameObject go)
    {
        go.SetActive(true);

        if (go.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.DOKill();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
        }
        else
        {
            var t = go.transform;
            t.DOKill();
            t.localScale = Vector3.zero;
            t.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }
        UIManager.Instance?.HideAllPanels();
    }

    private void AnimateHide(GameObject go)
    {
        if (!go.activeInHierarchy) return;

        if (go.TryGetComponent<CanvasGroup>(out var cg))
        {
            cg.DOKill();
            cg.DOFade(0f, 0.3f).SetEase(Ease.InQuad)
              .OnComplete(() => go.SetActive(false));
        }
        else
        {
            var t = go.transform;
            t.DOKill();
            t.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
              .OnComplete(() => go.SetActive(false));
        }
    }

    private void UpdateReadyButtonUI()
    {
        TMP_Text label = buttonReady.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = isReady ? readyText : notReadyText;

        // 🎨 Couleur dynamique
        Color targetColor = isReady ? readyColor : notReadyColor;

        Image bg = buttonReady.GetComponent<Image>();
        if (bg != null)
        {
            bg.DOKill();
            bg.DOColor(targetColor, 0.3f).SetEase(Ease.OutQuad);
        }

        // 🌟 Feedback visuel immédiat (punch)
        buttonReady.transform.DOKill(); // Arrête tout (pulse ou punch)
        buttonReady.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f).OnComplete(() =>
        {
            // 🔁 Lance la pulsation idle **après** le punch
            if (isReady)
            {
                buttonReady.transform
                    .DOScale(1.05f, 0.8f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetId("ReadyPulse");
            }
            else
            {
                buttonReady.transform
                    .DOScale(1f, 0.2f)
                    .SetEase(Ease.OutBack)
                    .SetId("ReadyPulse");
            }
        });

        // ColorBlock UI pour interaction native
        ColorBlock colors = buttonReady.colors;
        colors.normalColor = targetColor;
        colors.highlightedColor = targetColor * 1.1f;
        colors.pressedColor = targetColor * 0.8f;
        colors.selectedColor = targetColor;
        buttonReady.colors = colors;
    }

    public void UpdateHostUI()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;

        foreach (var btn in gameModeButtons)
            btn.interactable = isHost;
    }

    public void UpdateReadyCount(int current, int total)
    {
        Debug.Log($"[UI] UpdateReadyCount: {current} / {total}");
        if (readyCountText == null) return;

        current = Mathf.Clamp(current, 0, total);
        readyCountText.text = $"{current} / {total}";
    }

    private void UpdateGameModeButtonVisuals(int selectedIndex)
    {
        for (int i = 0; i < gameModeButtons.Length; i++)
        {
            var btn = gameModeButtons[i];
            if (btn == null) continue;

            // Cible l'image du GameObject parent (Game 1 Button)
            Image backgroundImage = btn.GetComponent<Image>();
            if (backgroundImage == null) continue;

            // Garde la couleur (r, g, b) intacte, modifie seulement l'alpha
            Color color = backgroundImage.color;
            color.a = (i == selectedIndex) ? 1f : 0.2f;
            backgroundImage.color = color;
        }
    }

    #region NETWORKING
    public void OnClientConnected()
    {
        if (NetworkManager.Singleton.IsClient && MultiplayerNetwork.Instance != null)
        {
            UpdateReadyCount(
                MultiplayerNetwork.Instance.ReadyCount.Value,
                MultiplayerNetwork.Instance.PlayerCount.Value
            );
        }
    }
    #endregion
}