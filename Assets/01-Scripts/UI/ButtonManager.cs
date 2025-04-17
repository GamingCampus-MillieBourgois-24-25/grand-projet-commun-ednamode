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
    public Button buttonBackFromTexture;

    private CustomizableCharacterUI _characterUI;

    public void Initialize(CustomizableCharacterUI characterUI)
    {
        _characterUI = characterUI;

        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
            }
        }

        if (mainScrollView != null)
        {
            mainScrollView.gameObject.SetActive(true);
        }

        if (buttonTags != null)
        {
            buttonTags.gameObject.SetActive(true);
            buttonTags.onClick.RemoveAllListeners();
            buttonTags.onClick.AddListener(() =>
            {
                _characterUI.ShowTagsPanel();
            });
            Debug.Log("Bouton Tags configuré et activé.");
        }
        else
        {
            Debug.LogWarning("ButtonTags n'est pas assigné dans l'inspecteur !");
        }

        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(backButton, false);

        AssignButtonEvents();
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
        if (buttonBackFromTexture != null)
        {
            buttonBackFromTexture.onClick.RemoveAllListeners();
            buttonBackFromTexture.onClick.AddListener(() => _characterUI.OnBackFromTextureClicked());
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.button != null)
            {
                pair.button.onClick.RemoveAllListeners();
                pair.button.onClick.AddListener(() => OnButtonClicked(pair));
            }
        }
    }

    public void ShowTextureOptions()
    {
        SetButtonActive(buttonBackFromTexture, true);
        SetButtonActive(backButton, false);
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(false);
        if (buttonTags != null) buttonTags.gameObject.SetActive(false);
    }

    public void ReturnToMainView()
    {
        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(backButton, false);
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(true);
        if (buttonTags != null) buttonTags.gameObject.SetActive(true);
    }

    private void OnButtonClicked(ButtonScrollViewPair clickedPair)
    {
        if (mainScrollView != null) mainScrollView.gameObject.SetActive(false);

        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                bool shouldBeActive = (pair == clickedPair);
                pair.scrollView.gameObject.SetActive(shouldBeActive);
                Debug.Log($"ScrollView {pair.scrollView.name} définie à l'état: {shouldBeActive}");
            }
        }

        if (backButton != null) backButton.gameObject.SetActive(true);
        // Ne pas désactiver buttonTags ici
        // if (buttonTags != null) buttonTags.gameObject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(false);
                Debug.Log($"ScrollView {pair.scrollView.name} désactivée lors du retour.");
            }
        }

        if (mainScrollView != null)
        {
            mainScrollView.gameObject.SetActive(true);
            Debug.Log($"Main ScrollView {mainScrollView.name} activée lors du retour.");
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (buttonTags != null) buttonTags.gameObject.SetActive(true);
    }
}