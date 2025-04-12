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

        if (mainScrollView != null) mainScrollView.gameObject.SetActive(true);
        if (buttonTags != null)
        {
            buttonTags.gameObject.SetActive(true);
            buttonTags.onClick.AddListener(() => _characterUI.ShowTagsPanel());
        }

        SetButtonActive(buttonBackFromTexture, false);
        SetButtonActive(backButton, false);

        AssignButtonEvents();

        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.scrollView != null)
            {
                pair.scrollView.gameObject.SetActive(true);
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
        if (buttonBackFromTexture != null) buttonBackFromTexture.onClick.AddListener(() => _characterUI.OnBackFromTextureClicked());
        if (backButton != null) backButton.onClick.AddListener(OnBackButtonClicked);
        foreach (var pair in buttonScrollViewPairs)
        {
            if (pair.button != null)
            {
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
                pair.scrollView.gameObject.SetActive(pair == clickedPair);
            }
        }
        if (backButton != null) backButton.gameObject.SetActive(true);
        if (buttonTags != null) buttonTags.gameObject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
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

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (buttonTags != null) buttonTags.gameObject.SetActive(true);
    }
}