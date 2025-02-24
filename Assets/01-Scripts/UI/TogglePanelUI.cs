using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TogglePanelUI : MonoBehaviour
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
    [SerializeField] private Image borderImage; // L'image du border
    [SerializeField] private float blinkDuration = 0.5f; // Durée du clignotement
    [SerializeField] private float blinkAlpha = 0f; // Intensité du clignotement

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

        // Cache le panel au début
        SetPanelState(false, true);
    }

    public void TogglePanel()
    {
        isVisible = !isVisible;
        SetPanelState(isVisible, false);
    }

    private void SetPanelState(bool show, bool instant)
    {
        float targetAlpha = show ? 1 : 0;
        Vector3 targetScale = show ? visibleScale : hiddenScale;
        Vector2 targetPosition = show ? originalPosition : originalPosition + hiddenPositionOffset;

        if (instant)
        {
            panelCanvasGroup.alpha = targetAlpha;
            panelRect.localScale = targetScale;
            panelRect.anchoredPosition = targetPosition;
        }
        else
        {
            panelCanvasGroup.DOFade(targetAlpha, fadeDuration);
            panelRect.DOScale(targetScale, scaleDuration).SetEase(animationEase);
            panelRect.DOAnchorPos(targetPosition, scaleDuration).SetEase(animationEase);
        }

        panelCanvasGroup.blocksRaycasts = show;
        panelCanvasGroup.interactable = show;

        if (buttonText != null)
            buttonText.text = show ? "HIDE" : "SHOW";

        // Active ou désactive le clignotement du border
        if (!show)
            StartBlinkingBorder();
        else
            StopBlinkingBorder();
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
        borderImage.DOFade(1, 0.2f); // Rétablit l'alpha
        borderImage.gameObject.SetActive(false);
    }
}
