using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CharacterCustomization
{
    public class ItemSorter : MonoBehaviour
    {
        public void SortItemsByCategory(string category)
        {
            foreach (Transform child in transform)
            {
                Debug.Log("Child: " + child.gameObject.name);
                if (child.gameObject.GetComponent<ShopButton>().GetCategory() == category)
                {
                    child.gameObject.SetActive(true);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        public void ShowAllItems()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}
