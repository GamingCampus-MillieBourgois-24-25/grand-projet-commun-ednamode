
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StartingScreenController : MonoBehaviour
{
    public CanvasGroup screenGroup;
    public TextMeshProUGUI pressText;
    public float fadeSpeed = 2f;
    public Animator animator;

    private bool isTransitioning = false;

    private void Start()
    {
        screenGroup.alpha = 0;
        screenGroup.gameObject.SetActive(true);
        animator.SetTrigger("FadeIn");
    }

    private void Update()
    {
        if (!isTransitioning && Input.anyKeyDown)
        {
            isTransitioning = true;
            animator.SetTrigger("Exit");
            Invoke(nameof(TriggerLogin), 0.7f);
        }
    }

    private void TriggerLogin()
    {
        screenGroup.gameObject.SetActive(false);
        UIManagerLogin.Instance.OpenGameLogin();
    }
}
