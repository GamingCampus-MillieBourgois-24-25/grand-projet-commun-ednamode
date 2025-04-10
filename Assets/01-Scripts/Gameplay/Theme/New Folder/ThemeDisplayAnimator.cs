using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Affiche et anime le th�me avec apparition/disparition.
/// </summary>
public class ThemeDisplayAnimator : NetworkBehaviour
{
    [Header("UI")]
    [Tooltip("R�f�rence au TMP_Text � animer.")]
    [SerializeField] private TMP_Text themeText;

    [Tooltip("Position de d�part (hors �cran).")]
    [SerializeField] private Vector3 startPosition = new Vector3(0, 600, 0);

    [Tooltip("Position finale (�cran visible).")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;

    [Tooltip("Dur�e de l'affichage � l'�cran.")]
    [SerializeField] private float displayDuration = 5f;

    [Tooltip("Dur�e de l�animation entr�e/sortie.")]
    [SerializeField] private float animationDuration = 1f;

    private void Start()
    {
        if (!IsClient) return;

        themeText.rectTransform.anchoredPosition = startPosition;
        string theme = ThemeSyncManager.Instance.CurrentTheme;
        themeText.text = $"Th�me : {theme}";
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
