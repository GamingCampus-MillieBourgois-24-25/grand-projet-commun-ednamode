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

        // R�cup�rer le nom du bouton cliqu� et activer la cat�gorie correspondante
        camPointName = clickedButton.name.Replace("Button", "CamPoint");
        camPoint = chooseCamPoint.transform.Find(camPointName)?.gameObject;

        if (camPoint != null)
        {
            // Appeler la m�thode pour changer le point de cam�ra
            chooseCamPoint.SwitchToCamPoint(camPointName);
            Debug.Log($"Changement de la cam�ra vers le point : {camPointName}");
        }
        else
        {
            Debug.LogWarning($"Le GameObject avec le nom '{camPointName}' n'a pas �t� trouv� dans la sc�ne.");
        }
    }

    private void UpdateButtonInteractability(Button clickedButton)
    {
        if (button == clickedButton)
            {
                // D�sactiver le bouton cliqu� et changer sa couleur
                button.interactable = false;
                button.GetComponent<Image>().color = Color.gray;
            }
        else
            {
                // R�activer les autres boutons et r�initialiser leur couleur
                button.interactable = true;
                button.GetComponent<Image>().color = Color.white;
            }
    }

}
