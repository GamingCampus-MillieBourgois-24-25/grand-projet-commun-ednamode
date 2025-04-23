using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferButton : MonoBehaviour
{
    private TextMeshProUGUI buttonText;
    private DataSaver dataSaver;
    private int coinsToObtain;
    private int jewelsToSpend;
    private ScriptableObject offer;
    private Button button;
    void Awake()
    {
        dataSaver = DataSaver.Instance;
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();

        if (buttonText == null)
        {
            Debug.LogError("TextMeshPro component not found in children of this GameObject.");
        }

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
        if (buttonText != null)
        {
            buttonText.text = jewels+" jewels -> "+coins+" coins";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.");
        }
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

