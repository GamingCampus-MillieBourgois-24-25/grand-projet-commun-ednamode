
using UnityEngine;

public class PanelAnimator : MonoBehaviour
{
    private Animator animator;
    private CanvasGroup group;

    void Awake()
    {
        animator = GetComponent<Animator>();
        group = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (animator != null) animator.SetTrigger("Show");
    }

    public void Hide()
    {
        if (animator != null) animator.SetTrigger("Hide");
    }
}
