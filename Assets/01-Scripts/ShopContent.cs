using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ShopContent : MonoBehaviour
{
    public List<Tab> tabs = new List<Tab>();
    [SerializeField] private GameObject tabButtonPrefab;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private List<GameObject> itemButtons = new List<GameObject>();
    private void Start()
    {
        itemButtons= new List<GameObject>();
        // Trouver l'enfant avec un LayoutGroup
        LayoutGroup layoutGroup = GetComponentInChildren<LayoutGroup>();
        GridLayoutGroup gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
        if (layoutGroup == null)
        {
            Debug.LogError("Aucun LayoutGroup trouvé dans les enfants !");
            return;
        }

        // Charger tous les tabs depuis le dossier Resources
        Tab[] loadedTabs = Resources.LoadAll<Tab>("Tabs");
        foreach (Tab tab in loadedTabs)
        {
            tabs.Add(tab);

            // Instancier le bouton en tant qu'enfant du LayoutGroup
            GameObject tabButton = Instantiate(tabButtonPrefab, layoutGroup.transform);
            tabButton.GetComponent<ShopButton>().SetScriptable(tab);
            
        }
        Item[] loadedItems = Resources.LoadAll<Item>("Items");
        // Sort items into their respective tabs
        foreach (Tab tab in tabs)
        {
            tab.items.Clear(); // Clear existing items
            foreach (Item item in loadedItems)
            {
                if ((int)item.category == (int)tab.category)
                {
                    tab.items.Add(item);
                    // Instancier le bouton de l'item en tant qu'enfant du LayoutGroup
                    GameObject itemButton = Instantiate(itemButtonPrefab, gridLayoutGroup.transform);
                    itemButton.GetComponent<ShopButton>().SetScriptable(item);
                }
            }
        }
        
    }
}
