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
        coinsText.text = "Coins: " + _dataSaver.GetCoins().ToString();
        jewelsText.text = "Jewels: " + _dataSaver.GetJewels().ToString();
    }

    public void SetSelectedItemButton(ItemButton itemButton)
    {
        if (itemButton == null)
        {
            Debug.LogError("SetSelectedItem a reçu un item null !");
            return;
        }

        selectedItemButton = itemButton;
    }


    private void ProcessPurchase(int currentBalance, int price, System.Action<int> removeCurrency)
    {
        if (currentBalance >= price)
        {
            removeCurrency(price);
            _dataSaver.AddItem(selectedItem);
            Destroy(selectedItemButton.gameObject);
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
