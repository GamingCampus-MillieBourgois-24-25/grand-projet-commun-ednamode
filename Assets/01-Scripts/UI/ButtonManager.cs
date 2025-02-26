using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScrollViewManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonScrollViewPair
    {
        public Button button; 
        public ScrollRect scrollView; 
    }

    public ScrollRect mainScrollView; 
    public List<ButtonScrollViewPair> buttonScrollViewPairs;
    public Button backButton; 

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
        }

        // Assigner l'�v�nement au bouton "Retour"
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(false); // D�sactiver le bouton "Retour" au d�marrage
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