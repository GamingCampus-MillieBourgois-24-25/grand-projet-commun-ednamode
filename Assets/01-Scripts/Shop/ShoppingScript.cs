using CharacterCustomization;
using TMPro;
using UnityEngine;

public class ShoppingScript : MonoBehaviour
{
    private DataSaver _dataSaver;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI jewelsText;
    private Item _selectedItem;
    void Start()
    {
        _dataSaver = DataSaver.Instance;
        coinsText.text = "Coins: " + _dataSaver.GetCoins().ToString();
        jewelsText.text = "Jewels: " + _dataSaver.GetJewels().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectedItem(CustomizationItem item)
    {
        
    }
}
