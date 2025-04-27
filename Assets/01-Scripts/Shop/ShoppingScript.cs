using CharacterCustomization;
using TMPro;
using UnityEngine;

public class ShoppingScript : MonoBehaviour
{
    private DataSaver _dataSaver;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI jewelsText;
    [SerializeField] private ItemButton selectedItemButton;
    private Item selectedItem;
    void Start()
    {
        _dataSaver = DataSaver.Instance;
        SetTexts();
    }

    public void SetSelectedItemButton(ItemButton itemButton)
    {
        if (itemButton == null)
        {
            Debug.LogError("SetSelectedItem a re�u un item null !");
            return;
        }

        selectedItemButton = itemButton;
    }

    public void SetTexts()
    {
        coinsText.text = _dataSaver.GetCoins().ToString();
        jewelsText.text = _dataSaver.GetJewels().ToString();
    }
    private void ProcessPurchase(int currentBalance, int price, System.Action<int> removeCurrency)
    {
        if (currentBalance >= price)
        {
            removeCurrency(price);

            // Ajout de l'item � la liste des v�tements d�bloqu�s
            _dataSaver.AddItem(selectedItem);

            Debug.Log($"Item ajout� : {selectedItem.itemName}");
        }
        else
        {
            Debug.Log($"Not enough to buy the item.");
        }
    }

    public void Buy()
    {
        selectedItem = selectedItemButton.GetItem();
        if (selectedItem == null)
        {
            Debug.LogError("Item is null");
            return;
        }

        if (selectedItem.tags.Contains("premium"))
        {
            ProcessPurchase(
                _dataSaver.dts.totalJewels, // Acc�s direct � totalJewels
                selectedItem.price,
                jewelsToRemove => _dataSaver.dts.totalJewels -= jewelsToRemove // Modification directe
            );
        }
        else
        {
            ProcessPurchase(
                _dataSaver.dts.totalCoins, // Acc�s direct � totalCoins
                selectedItem.price,
                coinsToRemove => _dataSaver.dts.totalCoins -= coinsToRemove // Modification directe
            );
        }
    }

}
