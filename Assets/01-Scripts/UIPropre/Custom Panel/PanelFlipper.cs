using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PanelFlipper : MonoBehaviour, IPointerClickHandler
{
    [Header("🃏 Faces")]
    [SerializeField] private GameObject faceA;
    [SerializeField] private GameObject faceB;

    [Header("🎬 Animation")]
    [SerializeField] private float flipDuration = 0.4f;
    [SerializeField] private Ease flipEase = Ease.OutBack;

    private bool isFlipped = false;
    private bool isAnimating = false;

    public GameObject GetFrontFace() => faceB;


    private void Start()
    {
        // Affiche face A par défaut
        SetFace(false, instant: true);
    }

    public void OnFlipButtonClicked()
    {
        if (isAnimating) return;

        isFlipped = !isFlipped;
        Flip(isFlipped);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Clique en dehors → revient à FaceA si retourné
        if (isFlipped && !isAnimating)
        {
            isFlipped = false;
            Flip(false);
        }
    }

    private void Flip(bool toBack)
    {
        isAnimating = true;

        transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2).SetEase(Ease.InBack).OnComplete(() =>
        {
            SetFace(toBack, instant: false);
            transform.rotation = Quaternion.Euler(0, -90, 0);

            transform.DORotate(Vector3.zero, flipDuration / 2).SetEase(Ease.OutBack).OnComplete(() =>
            {
                isAnimating = false;
            });
        });
    }

    private void SetFace(bool showBack, bool instant)
    {
        if (faceA != null) faceA.SetActive(!showBack);
        if (faceB != null) faceB.SetActive(showBack);

        if (instant)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    public void SetupFaceB(string label, Color color, Sprite icon)
    {
        TMP_Text text = faceB.GetComponentInChildren<TMP_Text>();
        Image image = faceB.GetComponentInChildren<Image>();

        text.text = label;
        image.color = color;
        if (icon != null) image.sprite = icon;
    }

    public void TriggerFlip()
    {
        Flip(true);  // Si Flip(bool) est la méthode interne
    }

}
