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


    public void Buy()
    {
        if (selectedItem == null)
        {
            Debug.LogError("Item is null");
            return;
        }
        if (_dataSaver.GetCoins() >= selectedItem.price)
        {
            _dataSaver.removeCoins(selectedItem.price);
            coinsText.text = "Coins: " + _dataSaver.GetCoins().ToString();
            _dataSaver.AddItem(selectedItem);
            Debug.Log("Item bought: " + selectedItem.itemName);
            Debug.Log("Coins left: " + _dataSaver.GetCoins());
            _dataSaver.ShowItems();
        }
        else
        {
            Debug.Log("Not enough coins to buy the item.");
        }
    }
}
