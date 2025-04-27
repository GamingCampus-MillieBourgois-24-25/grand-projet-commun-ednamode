using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CharacterCustomization
{
    public class ShopContent : MonoBehaviour
    {
        [SerializeField] private CharacterItemManager characterItemManager;
        public List<Tab> tabs = new List<Tab>();
        [SerializeField] private GameObject tabButtonPrefab;
        [SerializeField] private GameObject itemButtonPrefab;
        [SerializeField] private List<GameObject> itemButtons = new List<GameObject>();
        private DataSaver dataSaver;
        private List<Item> loadedItems;
        private List<Item> savedItems;

        private void Start()
        {
            dataSaver = DataSaver.Instance;
            itemButtons = new List<GameObject>();

            if (dataSaver == null || characterItemManager == null || tabButtonPrefab == null || itemButtonPrefab == null)
            {
                Debug.LogError("[ShopContent] Composant manquant (DataSaver, CharacterItemManager, tabButtonPrefab ou itemButtonPrefab) !");
                return;
            }

            LayoutGroup layoutGroup = GetComponentInChildren<LayoutGroup>();
            GridLayoutGroup gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
            if (layoutGroup == null || gridLayoutGroup == null)
            {
                Debug.LogError("[ShopContent] Composant manquant (LayoutGroup ou GridLayoutGroup) !");
                return;
            }

            Tab[] loadedTabs = Resources.LoadAll<Tab>("Tabs");
            Debug.Log($"[ShopContent] {loadedTabs.Length} onglets chargés.");
            foreach (Tab tab in loadedTabs)
            {
                tabs.Add(tab);
                GameObject tabButton = Instantiate(tabButtonPrefab, layoutGroup.transform);
                ShopButton shopButton = tabButton.GetComponent<ShopButton>();
                if (shopButton == null)
                {
                    Debug.LogError($"[ShopContent] Aucun ShopButton sur {tabButton.name} !");
                    continue;
                }
                shopButton.SetScriptable(tab);
                Debug.Log($"[ShopContent] Onglet {tab.name} configuré sur {tabButton.name}.");
            }

            loadedItems = Resources.LoadAll<Item>("Items").ToList();
            savedItems = dataSaver.GetItems();
            loadedItems = loadedItems.Except(savedItems).ToList();
            Debug.Log($"[ShopContent] {loadedItems.Count} items chargés (non possédés).");

            Dictionary<SlotType, List<Item>> itemsByCategory = loadedItems
                .GroupBy(item => item.category)
                .ToDictionary(group => group.Key, group => group.ToList());

            HashSet<Item> ownedItemsSet = new HashSet<Item>(savedItems);

            foreach (Tab tab in tabs)
            {
                if (itemsByCategory.TryGetValue(tab.category, out List<Item> itemsInCategory))
                {
                    foreach (Item item in itemsInCategory)
                    {
                        if (ownedItemsSet.Contains(item))
                        {
                            continue;
                        }

                        tab.items.Add(item);
                        GameObject itemButton = Instantiate(itemButtonPrefab, gridLayoutGroup.transform);
                        ItemButton itemButtonScript = itemButton.GetComponent<ItemButton>();
                        if (itemButtonScript == null)
                        {
                            Debug.LogError($"[ShopContent] Aucun ItemButton sur {itemButton.name} !");
                            continue;
                        }
                        itemButtonScript.SetScriptable(item, () =>
                        {
                            Debug.Log($"[ShopContent] Clic sur item {item.itemName}");
                            characterItemManager.EquipSingleItemForShop(item);
                        });
                        itemButtons.Add(itemButton);
                        Debug.Log($"[ShopContent] Item {item.name} configuré sur {itemButton.name}.");
                    }
                }
            }
        }
    }
}