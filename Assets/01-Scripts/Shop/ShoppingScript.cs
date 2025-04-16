using CharacterCustomization;
using TMPro;
using UnityEngine;

public class ShoppingScript : MonoBehaviour
{
    private DataSaver _dataSaver;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI jewelsText;
    [SerializeField] private Item selectedItem;
    void Start()
    {
        _dataSaver = DataSaver.Instance;
        coinsText.text = "Coins: " + _dataSaver.GetCoins().ToString();
        jewelsText.text = "Jewels: " + _dataSaver.GetJewels().ToString();
    }

    public void SetSelectedItem(Item item)
    {
        if (item == null)
        {
            Debug.LogError("SetSelectedItem a reçu un item null !");
            return;
        }

        selectedItem = item;
        Debug.Log($"Item sélectionné : {item.name}");
    }


    private void ProcessPurchase(int currentBalance, int price, System.Action<int> removeCurrency)
    {
        if (currentBalance >= price)
        {
            removeCurrency(price);
            _dataSaver.AddItem(selectedItem);
            _dataSaver.ShowItems();
        }
        else
        {
            Debug.Log($"Not enough to buy the item.");
        }
    }

    public void Buy()
    {
        if (selectedItem == null)
        {
            Debug.LogError("Item is null");
            return;
        }

        if (selectedItem.tags.Contains("premium"))
        {
            ProcessPurchase(
                _dataSaver.GetJewels(),
                selectedItem.price,
                _dataSaver.removeJewels
            );
        }
        else
        {
            ProcessPurchase(
                _dataSaver.GetCoins(),
                selectedItem.price,
                _dataSaver.removeCoins
            );
        }
    }

}
