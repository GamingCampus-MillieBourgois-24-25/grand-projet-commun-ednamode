using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScrollViewManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonScrollViewPair
    {
        public Button button; // Le bouton qui active/désactive le ScrollView
        public ScrollRect scrollView; // Le ScrollView associé à ce bouton
    }

    public ScrollRect mainScrollView; // Référence au ScrollView principal
    public List<ButtonScrollViewPair> buttonScrollViewPairs; // Liste des boutons et ScrollView associés
    public Button backButton; // Référence au bouton "Retour"

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
            else
            {
                Debug.LogWarning($"Bouton non assigné pour le ScrollView : {pair.scrollView?.name ?? "NULL"}");
            }
        }

        // Assigner l'événement au bouton "Retour"
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            backButton.gameObject.SetActive(false); // Désactiver le bouton "Retour" au démarrage
        }
        else
        {
            Debug.LogWarning("Bouton 'Retour' non assigné !");
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
            Debug.Log($"ScrollView activé pour le bouton : {clickedPair.button.name}");
        }
        else
        {
            Debug.LogWarning($"Aucun ScrollView associé au bouton : {clickedPair.button.name}");
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