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

    public ScrollRect mainScrollView; // R�f�rence au ScrollView principal
    public List<ButtonScrollViewPair> buttonScrollViewPairs; // Liste des boutons et ScrollView associ�s
    public Button backButton; // R�f�rence au bouton "Retour"

    private void Start()
    {
        // D�sactiver tous les ScrollView au d�marrage
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Assigner les �v�nements aux boutons
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.button != null)
            {
                pair.button.onClick.AddListener(() => OnButtonClicked(pair));
            }
            else
            {
                Debug.LogWarning($"Bouton non assign� pour le ScrollView : {pair.scrollView?.name ?? "NULL"}");
            }
        }

        // Assigner l'�v�nement au bouton "Retour"
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(false); // D�sactiver le bouton "Retour" au d�marrage
        }
        else
        {
            Debug.LogWarning("Bouton 'Retour' non assign� !");
        }
    }

    private void OnButtonClicked(ButtonScrollViewPair clickedPair)
    {
        // D�sactiver le ScrollView principal
        if (mainScrollView != null)
        {
            mainScrollView.gameObject.SetActive(false);
        }

        // D�sactiver tous les autres ScrollView
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Activer le ScrollView associ� au bouton cliqu�
        if (clickedPair.scrollView != null)
        {
            clickedPair.scrollView.gameObject.SetActive(true);
            Debug.Log($"ScrollView activ� pour le bouton : {clickedPair.button.name}");
        }
        else
        {
            Debug.LogWarning($"Aucun ScrollView associ� au bouton : {clickedPair.button.name}");
        }

        // Activer le bouton "Retour"
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }
    }

    private void OnBackButtonClicked()
    {
        // D�sactiver tous les ScrollView
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // R�activer le ScrollView principal
        if (mainScrollView != null)
        {
            mainScrollView.gameObject.SetActive(true);
        }

        // D�sactiver le bouton "Retour"
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
    }
}