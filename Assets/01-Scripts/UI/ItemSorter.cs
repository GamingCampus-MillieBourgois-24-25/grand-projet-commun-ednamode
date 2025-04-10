using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
}
