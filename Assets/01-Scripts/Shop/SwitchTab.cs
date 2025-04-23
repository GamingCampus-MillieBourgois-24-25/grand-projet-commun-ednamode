using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class SwitchTab : MonoBehaviour
{
    private List<Button> buttons;
    private string clickedButtonName;
    private GameObject selectedCategory;
    void Start()
    {
        // Get all buttons in the parent object
        buttons = new List<Button>(GetComponentsInChildren<Button>(true));
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => OnButtonClick(button));
        }
    }

    private void OnButtonClick(Button clickedButton)
    {
        if(selectedCategory != null)
        {
            selectedCategory.SetActive(false);
        }
        // Disable the clicked button and enable others
        IsClickedButtonInteractable(clickedButton);
        // Get the name of the clicked button
        clickedButtonName = clickedButton.name.Replace("Button", "");
        selectedCategory = transform.parent.Find(clickedButtonName+"ScrollBack").gameObject;
        selectedCategory.SetActive(true);
    }
    private void IsClickedButtonInteractable(Button clickedButton)
    {
        foreach (Button button in buttons)
        {
            if (button == clickedButton)
            {
                button.interactable = false;
                button.GetComponent<Image>().color = Color.gray;
            }
            else
            {
                button.interactable = true;
                button.GetComponent<Image>().color = Color.white;
            }
        }
    }
}
