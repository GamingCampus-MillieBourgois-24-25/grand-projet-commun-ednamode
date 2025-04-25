using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CharacterCustomization;
public class OfferButton : MonoBehaviour
{
    private DataSaver dataSaver;
    private int coinsToObtain;
    private int jewelsToSpend;
    private ScriptableObject offer;
    private Button button;
    private List<TextMeshProUGUI> textMeshProUGUIs;
    [SerializeField] private Sprite coinIcon;
    [SerializeField] private Sprite jewelIcon;
    private List<Image> icons;

    private ShoppingScript shopping;
    void Awake()
    {
        shopping = Object.FindFirstObjectByType<ShoppingScript>();
        dataSaver = DataSaver.Instance;
        textMeshProUGUIs = new List<TextMeshProUGUI>();
        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>())
        {
            textMeshProUGUIs.Add(text);
        }
        icons = new List<Image>();
        foreach (var icon in GetComponentsInChildren<Image>())
        {
            if (icon.gameObject.name == "coinIcon")
            {
                icon.sprite = coinIcon;
            }
            else if (icon.gameObject.name == "jewelIcon")
            {
                icon.sprite = jewelIcon;
            }
            icons.Add(icon);
        }
        button = GetComponent<Button>();


        // Ajouter un listener au bouton
        if (button != null)
        {
            button.onClick.AddListener(Convert); // Passe une référence à la méthode Convert
        }
        else
        {
            Debug.LogError("Button component not found on this GameObject.");
        }
    }


    public void SetText(int jewels, int coins)
    {
        textMeshProUGUIs[0].text = jewels.ToString();
        textMeshProUGUIs[1].text = coins.ToString();
    }

    public void SetScriptable(ScriptableObject offer)
    {
        this.offer = offer;
        if (offer is ConvertCurrencyOffer convertCurrencyOffer)
        {
            jewelsToSpend = convertCurrencyOffer.jewelsToSpend;
            coinsToObtain = convertCurrencyOffer.coinsToObtain;
            SetText(jewelsToSpend, coinsToObtain);
        }
        else
        {
            Debug.LogError("The offer is not of type ConvertCurrencyOffer.");
        }
    }

    public void Convert()
    {
        if (offer is ConvertCurrencyOffer convertCurrencyOffer)
        {
            if (dataSaver.dts.totalJewels >= jewelsToSpend)
            {
                dataSaver.addCoins(coinsToObtain);
                dataSaver.removeJewels(jewelsToSpend);
                // Mettre à jour l'affichage des pièces et des joyaux
                shopping.SetTexts();
                Debug.Log("Conversion successful: " + jewelsToSpend + " jewels -> " + coinsToObtain + " coins");
            }
            else
            {
                Debug.LogError("Not enough jewels to convert.");
            }
        }
        else
        {
            Debug.LogError("The offer is not of type ConvertCurrencyOffer.");
        }
    }
}

