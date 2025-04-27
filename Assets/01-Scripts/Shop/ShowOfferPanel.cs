using UnityEngine;

public class ShowOfferPanel : MonoBehaviour
{
    [SerializeField] private GameObject offerPanel;
    private Animator animator;

    public void ShowPanel()
    {
        animator = offerPanel.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Show",true);
        }
        else
        {
            Debug.LogError("Animator component is missing on the offerPanel GameObject.");
        }
    }

    public void HidePanel()
    {
        if (animator != null)
        {
            animator.SetBool("Show", false);    
        }
        else
        {
            Debug.LogError("Animator component is missing or not initialized.");
        }
    }
}
