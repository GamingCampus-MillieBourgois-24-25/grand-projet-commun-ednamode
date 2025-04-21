using UnityEngine;
using System.Collections.Generic;
using CharacterCustomization;

public class OwnedItems : MonoBehaviour
{
    [SerializeField] private GameObject itemButton;
    private DataSaver dataSaver;
    private List<Item> ownedItems;
    private int i=0;
    void Start()
    {
        dataSaver = DataSaver.Instance;
        ownedItems = new List<Item>();
        ownedItems = dataSaver.dts.unlockedClothes;
        Debug.Log($"Owned items count: {ownedItems.Count}");
        LoadOwnedItems();
    }

    private void LoadOwnedItems()
    {
        foreach (Item item in ownedItems)
        {
            i++;
            Debug.Log($"i : {i}");
            GameObject itemObject = Instantiate(itemButton, this.transform);
            itemObject.GetComponent<ItemButton>().SetItem(item);
        }
    }
}
