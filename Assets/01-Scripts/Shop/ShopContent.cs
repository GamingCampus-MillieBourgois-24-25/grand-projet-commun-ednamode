using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
namespace CharacterCustomization
{
    public class ShopContent : MonoBehaviour
    {
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

            loadedItems = Resources.LoadAll<Item>("Items").ToList();
            savedItems = dataSaver.GetItems();
            loadedItems = loadedItems.Except(savedItems).ToList();

            Dictionary<SlotType, List<Item>> itemsByCategory = loadedItems
            .GroupBy(item => item.category)
            .ToDictionary(group => group.Key, group => group.ToList());

            // Récupérer la liste des items possédés
            HashSet<Item> ownedItemsSet = new HashSet<Item>(savedItems); // Utiliser un HashSet pour une recherche rapide

            // Associer les items aux tabs correspondants
            foreach (Tab tab in tabs)
            {
                if (itemsByCategory.TryGetValue(tab.category, out List<Item> itemsInCategory))
                {
                    foreach (Item item in itemsInCategory)
                    {
                        // Vérifier si l'item est déjà possédé
                        if (ownedItemsSet.Contains(item))
                        {
                            continue; // Passer cet item
                        }

                        tab.items.Add(item); // Ajouter l'item au tab

                        GameObject itemButton = Instantiate(itemButtonPrefab, gridLayoutGroup.transform);
                        itemButton.GetComponent<ShopButton>().SetScriptable(item);
                    }
                }
            }

        }
    }
}
