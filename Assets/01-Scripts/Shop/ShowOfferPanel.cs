using UnityEngine;

public class ShowOfferPanel : MonoBehaviour
{
    [SerializeField] private GameObject offerPanel;
    public void ShowPanel(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

}
