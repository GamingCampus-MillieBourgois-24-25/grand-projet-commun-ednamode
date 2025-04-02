using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Affiche et anime le thème avec apparition/disparition.
/// </summary>
public class ThemeDisplayAnimator : NetworkBehaviour
{
    [Header("UI")]
    [Tooltip("Référence au TMP_Text à animer.")]
    [SerializeField] private TMP_Text themeText;

    [Tooltip("Position de départ (hors écran).")]
    [SerializeField] private Vector3 startPosition = new Vector3(0, 600, 0);

    [Tooltip("Position finale (écran visible).")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;

    [Tooltip("Durée de l'affichage à l'écran.")]
    [SerializeField] private float displayDuration = 5f;

    [Tooltip("Durée de l’animation entrée/sortie.")]
    [SerializeField] private float animationDuration = 1f;

    private void Start()
    {
        if (!IsClient) return;

        themeText.rectTransform.anchoredPosition = startPosition;
        string theme = ThemeSyncManager.Instance.CurrentTheme;
        themeText.text = $"Thème : {theme}";
        AnimateThemeDisplay();
    }

    private void AnimateThemeDisplay()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(themeText.rectTransform.DOAnchorPos(targetPosition, animationDuration).SetEase(Ease.OutBack));
        sequence.AppendInterval(displayDuration);
        sequence.Append(themeText.rectTransform.DOAnchorPos(startPosition, animationDuration).SetEase(Ease.InBack));
    }
}
