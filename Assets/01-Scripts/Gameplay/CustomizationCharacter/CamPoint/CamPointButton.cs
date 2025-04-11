using UnityEngine;
using UnityEngine.UI;
public class CamPointButton : MonoBehaviour
{
    [SerializeField] private ChooseCamPoint chooseCamPoint;
    private string camPointName;
    private GameObject camPoint;
    private Button button;
    void Start()
    {
        button= GetComponent<Button>();
       button.onClick.AddListener(() => HandleButtonClick(button));
    }

    private void HandleButtonClick(Button clickedButton)
    {
        UpdateButtonInteractability(clickedButton);

        // Récupérer le nom du bouton cliqué et activer la catégorie correspondante
        camPointName = clickedButton.name.Replace("Button", "CamPoint");
        camPoint = chooseCamPoint.transform.Find(camPointName)?.gameObject;

        if (camPoint != null)
        {
            // Appeler la méthode pour changer le point de caméra
            chooseCamPoint.SwitchToCamPoint(camPointName);
            Debug.Log($"Changement de la caméra vers le point : {camPointName}");
        }
        else
        {
            Debug.LogWarning($"Le GameObject avec le nom '{camPointName}' n'a pas été trouvé dans la scène.");
        }
    }

    private void UpdateButtonInteractability(Button clickedButton)
    {
        if (button == clickedButton)
            {
                // Désactiver le bouton cliqué et changer sa couleur
                button.interactable = false;
                button.GetComponent<Image>().color = Color.gray;
            }
        else
            {
                // Réactiver les autres boutons et réinitialiser leur couleur
                button.interactable = true;
                button.GetComponent<Image>().color = Color.white;
            }
    }

}
