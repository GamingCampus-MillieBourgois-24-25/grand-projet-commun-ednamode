using UnityEngine;
using DG.Tweening;

public class SpriteAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetSprite;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 endScale = Vector3.one;
    [SerializeField] private float fadeStart = 0f;
    [SerializeField] private float fadeEnd = 1f;

    void Start()
    {
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        if (targetSprite == null) return;

        // Initialisation de l'état du sprite
        targetSprite.transform.localScale = startScale;
        targetSprite.color = new Color(targetSprite.color.r, targetSprite.color.g, targetSprite.color.b, fadeStart);

        // Animation combinée (scaling + fade)
        Sequence sequence = DOTween.Sequence();
        sequence.Append(targetSprite.transform.DOScale(endScale, animationDuration).SetEase(Ease.OutBounce));
        sequence.Join(targetSprite.DOFade(fadeEnd, animationDuration));
    }
}