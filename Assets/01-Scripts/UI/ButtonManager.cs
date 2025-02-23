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

    public List<ButtonScrollViewPair> buttonScrollViewPairs; // Liste des boutons et ScrollView associés

    private void Start()
    {
        // Désactiver tous les ScrollView au démarrage
        foreach (var pair in buttonScrollViewPairs)
        {
            pair.scrollView.gameObject.SetActive(false);
        }

        // Assigner les événements aux boutons
        foreach (var pair in buttonScrollViewPairs)
        {
            pair.button.onClick.AddListener(() => OnButtonClicked(pair));
        }
    }

    private void OnButtonClicked(ButtonScrollViewPair clickedPair)
    {
        // Désactiver tous les ScrollView
        foreach (var pair in buttonScrollViewPairs)
        {
            pair.scrollView.gameObject.SetActive(false);
        }

        // Activer le ScrollView associé au bouton cliqué
        clickedPair.scrollView.gameObject.SetActive(true);
        Debug.Log($"ScrollView activé pour le bouton : {clickedPair.button.name}");
    }
}