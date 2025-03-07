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
        // Désactiver tous les ScrollView au démarrage
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Assigner les événements aux boutons
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.button != null)
            {
                pair.button.onClick.AddListener(() => OnButtonClicked(pair));
            }
        }

        // Assigner l'événement au bouton "Retour"
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(false); // Désactiver le bouton "Retour" au démarrage
        }
    }

    private void OnButtonClicked(ButtonScrollViewPair clickedPair)
    {
        // Désactiver le ScrollView principal
        if (mainScrollView != null)
        {
            mainScrollView.gameObject.SetActive(false);
        }

        // Désactiver tous les autres ScrollView
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Activer le ScrollView associé au bouton cliqué
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
        // Désactiver tous les ScrollView
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Réactiver le ScrollView principal
        if (mainScrollView != null)
        {
            mainScrollView.gameObject.SetActive(true);
        }

        // Désactiver le bouton "Retour"
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }
    }
}