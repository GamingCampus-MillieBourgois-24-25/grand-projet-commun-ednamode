using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PanelBounceEffect : MonoBehaviour
{
    [SerializeField] private RectTransform rectPanel;
    [SerializeField] private GameObject gameobjectPanel;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float bounceHeight = 150f; // Hauteur du rebond
    [SerializeField] private float bounceDelay = 1.5f; // Délai entre les rebonds

    private Vector2 originalAnchorPosition;
    private bool isSlidingOut = false;

    void Start()
    {
        gameobjectPanel.SetActive(true);
        rectPanel.gameObject.SetActive(true);

        // Sauvegarde de la position de base (ancre)
        originalAnchorPosition = rectPanel.anchoredPosition;

        // Démarrage du rebond après 2 secondes
        Invoke(nameof(StartBouncingEffect), 2f);
    }

    void Update()
    {
        if (Input.anyKeyDown && !isSlidingOut)
        {
            SlideOut();
        }
    }

    private void SlideOut()
    {
        isSlidingOut = true;
        CancelInvoke(nameof(StartBouncingEffect));
        DOTween.Kill("BounceEffect");

        rectPanel.DOAnchorPosY(Screen.height, slideDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                gameobjectPanel.SetActive(false); // Désactiver après animation
            });
    }

    private void StartBouncingEffect()
    {
        if (isSlidingOut) return; // Ne pas rebondir si le panel sort

        float targetY = originalAnchorPosition.y + bounceHeight; // Monte à partir de l’ancre

        // 1?? Animation de montée
        rectPanel.DOAnchorPosY(targetY, 0.4f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 2?? Animation de descente (retour à l’ancre d'origine)
                rectPanel.DOAnchorPosY(originalAnchorPosition.y, 0.4f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        if (!isSlidingOut)
                        {
                            Invoke(nameof(StartBouncingEffect), bounceDelay); // Relance après 1.5s
                        }
                    });
            })
            .SetId("BounceEffect");
    }
}
