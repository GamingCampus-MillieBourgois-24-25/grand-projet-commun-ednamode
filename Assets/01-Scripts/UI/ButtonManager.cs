using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CharacterCustomization;

public class ButtonScrollViewManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonScrollViewPair
    {
        public Button button;
        public ScrollRect scrollView;
    }

    [Header("ScrollView Configuration")]
    public ScrollRect mainScrollView;
    public List<ButtonScrollViewPair> buttonScrollViewPairs;
    public Button backButton;
    public Button buttonTags;

    [Header("Customization UI Buttons")]
    public Button buttonEdit;
    public Button buttonDelete;
    public Button buttonBackFromTexture;
    public Button buttonBackFromEdit;
    public Button buttonBackFromInitial; // Nouveau bouton

    private CustomizableCharacterUI _characterUI;

    public void Initialize(CustomizableCharacterUI characterUI)
    {
        _characterUI = characterUI;

        // Désactiver tous les ScrollView au démarrage
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Activer mainScrollView et buttonTags au démarrage
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(true);
        if (buttonTags != null)
        {
            buttonTags.gameObject.SetActive(true);
            buttonTags.onClick.AddListener(() => _characterUI.ShowTagsPanel());
        }

        // Désactiver les boutons au démarrage
        SetButtonActive(buttonEdit, false);
        SetButtonActive(buttonDelete, false);
        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(buttonBackFromEdit, false);
        SetButtonActive(backButton, false);
        SetButtonActive(buttonBackFromInitial, false);

        // Assigner les événements
        AssignButtonEvents();

        // Réactiver tous les ScrollView après l’initialisation (pour s’assurer qu’ils sont visibles)
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(true);
                Debug.Log($"ScrollView {pair.scrollView.name} réactivé après initialisation");
            }
        }
    }

    private void SetButtonActive(Button button, bool active)
    {
        if (button != null)
        {
            button.gameObject.SetActive(active);
        }
    }

    private void AssignButtonEvents()
    {
        if (buttonEdit != null) buttonEdit.onClick.AddListener(() => _characterUI.OnEditClicked());
        if (buttonDelete != null) buttonDelete.onClick.AddListener(() => _characterUI.OnDeleteClicked());
        if (buttonBackFromTexture != null) buttonBackFromTexture.onClick.AddListener(() => _characterUI.OnBackFromTextureClicked());
        if (buttonBackFromEdit != null) buttonBackFromEdit.onClick.AddListener(() => _characterUI.OnBackFromEditClicked());
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        if (buttonBackFromInitial != null)
        {
            buttonBackFromInitial.onClick.AddListener(OnBackFromInitialClicked);
            Debug.Log("buttonBackFromInitial assigné"); // Vérifie si le bouton est détecté
        }
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.button != null)
            {
                pair.button.onClick.AddListener(() => OnButtonClicked(pair));
            }
        }
    }

    public void ShowInitialButtons()
    {
        SetButtonActive(buttonEdit, true);
        SetButtonActive(buttonDelete, true);
        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(buttonBackFromEdit, false);
        SetButtonActive(backButton, true); // Garde le comportement initial
        SetButtonActive(buttonBackFromInitial, true); // Affiche le nouveau bouton
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(false);
        if (buttonTags != null) buttonTags.gameObject.SetActive(false);
    }

    public void ShowEditOptions()
    {
        SetButtonActive(buttonEdit, false);
        SetButtonActive(buttonDelete, false);
        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(buttonBackFromEdit, true);
        SetButtonActive(backButton, false);
        SetButtonActive(buttonBackFromInitial, false); // Désactive dans ce mode
        // Les boutons ChangeTexture et ChangeColor sont gérés par CustomizableCharacterUI
    }

    public void ShowTextureOptions()
    {
        SetButtonActive(buttonEdit, false);
        SetButtonActive(buttonDelete, false);
        SetButtonActive(buttonBackFromTexture, true);
        SetButtonActive(buttonBackFromEdit, false);
        SetButtonActive(backButton, false);
        SetButtonActive(buttonBackFromInitial, false); // Désactive dans ce mode
    }

    private void OnButtonClicked(ButtonScrollViewPair clickedPair)
    {
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(false);
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(pair == clickedPair);
            }
        }
        if (backButton != null) backButton.gameObject.SetActive(true);
        if (buttonTags != null) buttonTags.gameObject.SetActive(false);
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

    private void OnBackFromInitialClicked()
    {
        Debug.Log("OnBackFromInitialClicked appelé"); // Doit apparaître au clic
                                                      // Désactiver tous les ScrollView dans buttonScrollViewPairs
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        // Désactiver tous les ScrollRect dans la scène
        ScrollRect[] allScrollViews = FindObjectsOfType<ScrollRect>();
        foreach (var scrollView in allScrollViews)
        {
            scrollView.gameObject.SetActive(false);
        }

        // Désactiver tous les boutons
        SetButtonActive(buttonEdit, false);
        SetButtonActive(buttonDelete, false);
        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(buttonBackFromEdit, false);
        SetButtonActive(backButton, false);
        SetButtonActive(buttonBackFromInitial, false);

        // Réactiver l’état initial
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(true);
        if (buttonTags != null) buttonTags.gameObject.SetActive(true);

        _characterUI.ResetCamera();
    }
}