using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class NotificationUI : MonoBehaviour
{
    [Header("Durée & Animation")]
    [Tooltip("Durée avant disparition automatique.")]
    [SerializeField] private float displayDuration = 3f;

    [Tooltip("Décalage d'apparition (en haut de l'écran).")]
    [SerializeField] private Vector2 startOffset = new(0, 80f);

    [Tooltip("Espacement vertical entre notifications.")]
    [SerializeField] private float verticalSpacing = 60f;

    private TMP_Text messageText;
    private Image iconImage;
    private Image background;
    private RectTransform rectTransform;
    private GameObject originalPrefab;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Hypothèse : structure = Image (parent) > Icon (Image), Message (TMP_Text)
        background = GetComponent<Image>();
        if (!background)
            Debug.LogError("[NotificationUI] Aucun fond Image trouvé sur l'objet principal.");

        // Recherche dans les enfants directs uniquement
        foreach (Transform child in transform)
        {
            var img = child.GetComponent<Image>();
            if (img != null && iconImage == null)
                iconImage = img;

            var text = child.GetComponent<TMP_Text>();
            if (text != null && messageText == null)
                messageText = text;
        }

        if (!messageText || !background)
        {
            Debug.LogError("[NotificationUI] Structure du prefab invalide. Assurez-vous que le fond est sur le parent, et que le texte et l’icône sont bien enfants directs.");
        }
    }

    public void Initialize(NotificationData data, GameObject prefabRef, int index)
    {
        originalPrefab = prefabRef;

        // Position initiale hors-écran (vers le haut)
        Vector2 targetPos = -Vector2.up * index * verticalSpacing;
        rectTransform.anchoredPosition = startOffset;

        if (messageText) messageText.text = data.message;
        if (iconImage) iconImage.sprite = data.icon;
        if (background) background.color = data.color;

        // Animation d'entrée vers le bas
        rectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutCubic);

        // Suppression après durée
        DOVirtual.DelayedCall(displayDuration, HideAnimated);
    }

    public void ShiftToIndex(int index)
    {
        Vector2 targetPos = -Vector2.up * index * verticalSpacing;
        rectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutCubic);
    }

    public void HideAnimated()
    {
        transform.SetAsFirstSibling(); // Se met en arrière-plan visuel

        rectTransform.DOAnchorPos(startOffset, 0.3f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            ObjectPool.Instance.Despawn(gameObject, originalPrefab);
            NotificationManager.Instance.NotifyDespawn(this);
        });
    }
}
