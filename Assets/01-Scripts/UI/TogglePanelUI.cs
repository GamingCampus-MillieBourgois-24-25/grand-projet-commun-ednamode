using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.Netcode; // Ajout du Netcode

public class TogglePanelUI : NetworkBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Button toggleButton;
    [SerializeField] private TMP_Text buttonText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float scaleDuration = 0.3f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;
    [SerializeField] private Vector2 hiddenPositionOffset = new Vector2(0, -200f);
    [SerializeField] private Ease animationEase = Ease.OutBack;

    [Header("Border Blink Settings")]
    [SerializeField] private Image borderImage;
    [SerializeField] private float blinkDuration = 0.5f;
    [SerializeField] private float blinkAlpha = 0f;

    private bool isVisible = false;
    private Vector2 originalPosition;
    private Tween borderTween;

    private void Awake()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();

        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        originalPosition = panelRect.anchoredPosition;

        // Désactive complètement le panel au démarrage
        panelCanvasGroup.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[TogglePanelUI] Connexion détectée. Fermeture du panel.");
            OnSessionJoined();
        }
    }

    public void TogglePanel()
    {
        isVisible = !isVisible;
        Debug.Log($"[TogglePanelUI] TogglePanel() appelé. Nouvel état : {isVisible}");
        SetPanelState(isVisible, false);
    }

    private void SetPanelState(bool show, bool instant)
    {
        float targetAlpha = show ? 1 : 0;
        Vector3 targetScale = show ? visibleScale : hiddenScale;
        Vector2 targetPosition = show ? originalPosition : originalPosition + hiddenPositionOffset;

        Debug.Log($"[TogglePanelUI] Changement d’état du panel : {show}");

        if (show)
        {
            panelCanvasGroup.gameObject.SetActive(true);
        }

        if (instant)
        {
            panelCanvasGroup.alpha = targetAlpha;
            panelRect.localScale = targetScale;
            panelRect.anchoredPosition = targetPosition;
        }
        else
        {
            panelCanvasGroup.DOFade(targetAlpha, fadeDuration).OnComplete(() =>
            {
                if (!show)
                {
                    HidePanel();
                }
            });

            panelRect.DOScale(targetScale, scaleDuration).SetEase(animationEase);
            panelRect.DOAnchorPos(targetPosition, scaleDuration).SetEase(animationEase);
        }

        panelCanvasGroup.blocksRaycasts = show;
        panelCanvasGroup.interactable = show;

        if (buttonText != null)
            buttonText.text = show ? "HIDE" : "SHOW";

        if (!show)
            StartBlinkingBorder();
        else
            StopBlinkingBorder();
    }

    private void HidePanel()
    {
        Debug.Log("[TogglePanelUI] Désactivation complète du panel.");
        panelCanvasGroup.gameObject.SetActive(false);
    }

    public void OnSessionJoined()
    {
        Debug.Log("[TogglePanelUI] OnSessionJoined() appelé. Masquage du panel.");

        if (isVisible)
        {
            SetPanelState(false, false);
        }
    }

    private void StartBlinkingBorder()
    {
        if (borderImage == null)
            return;

        borderImage.gameObject.SetActive(true);
        borderTween?.Kill();
        borderTween = borderImage.DOFade(blinkAlpha, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopBlinkingBorder()
    {
        if (borderImage == null)
            return;

        borderTween?.Kill();
        borderImage.DOFade(1, 0.2f);
        borderImage.gameObject.SetActive(false);
    }
}
