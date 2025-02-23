using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScrollViewManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonScrollViewPair
    {
        public Button button; // Le bouton qui active/d�sactive le ScrollView
        public ScrollRect scrollView; // Le ScrollView associ� � ce bouton
    }

    public List<ButtonScrollViewPair> buttonScrollViewPairs; // Liste des boutons et ScrollView associ�s

    private void Start()
    {
        // D�sactiver tous les ScrollView au d�marrage
        foreach (var pair in buttonScrollViewPairs)
        {
            pair.scrollView.gameObject.SetActive(false);
        }

        // Assigner les �v�nements aux boutons
        foreach (var pair in buttonScrollViewPairs)
        {
            pair.button.onClick.AddListener(() => OnButtonClicked(pair));
        }
    }

    private void OnButtonClicked(ButtonScrollViewPair clickedPair)
    {
        // D�sactiver tous les ScrollView
        foreach (var pair in buttonScrollViewPairs)
        {
            pair.scrollView.gameObject.SetActive(false);
        }

        // Activer le ScrollView associ� au bouton cliqu�
        clickedPair.scrollView.gameObject.SetActive(true);
        Debug.Log($"ScrollView activ� pour le bouton : {clickedPair.button.name}");
    }
}