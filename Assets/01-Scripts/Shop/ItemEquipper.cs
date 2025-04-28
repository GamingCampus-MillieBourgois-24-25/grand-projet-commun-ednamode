using System.Collections.Generic;
using CharacterCustomization;
using UnityEngine;

public class ItemEquipper : MonoBehaviour
{
    [System.Serializable]
    public class EquipPoint
    {
        public SlotType slotType; // Type de l'item (ex: Hat, Glasses, etc.)
        public Transform attachPoint; // Point d'attache sur le personnage
    }

    [Header("Configuration")]
    public List<EquipPoint> equipPoints; // Liste des points d'attache pour chaque type d'item
    public GameObject character; // Le personnage sur lequel les items seront ?quip?s

    private Dictionary<SlotType, GameObject> equippedItems = new Dictionary<SlotType, GameObject>();

    /// <summary>
    /// ?quipe un item sur le personnage.
    /// </summary>
    /// <param name="itemPrefab">Le prefab de l'item ? ?quiper.</param>
    /// <param name="slotType">Le type de l'item (ex: Hat, Glasses).</param>
    public void EquipItem(GameObject itemPrefab, SlotType slotType)
    {
        // Trouver le point d'attache correspondant au type d'item
        EquipPoint equipPoint = equipPoints.Find(point => point.slotType == slotType);
        if (equipPoint == null)
        {
            return;
        }

        // Si un item est d?j? ?quip? ? ce slot, le retirer
        if (equippedItems.ContainsKey(slotType))
        {
            Destroy(equippedItems[slotType]);
            equippedItems.Remove(slotType);
        }

        // Instancier le nouvel item et l'attacher au point d'attache
        GameObject newItem = Instantiate(itemPrefab, equipPoint.attachPoint);
        newItem.transform.localPosition = Vector3.zero;
        newItem.transform.localRotation = Quaternion.identity;
        newItem.transform.localScale = Vector3.one;

        // Ajouter l'item au dictionnaire des items ?quip?s
        equippedItems[slotType] = newItem;
    }

    /// <summary>
    /// M?thode ? appeler lorsqu'un bouton est cliqu?.
    /// </summary>
    /// <param name="item">L'item ? ?quiper.</param>
    public void OnItemButtonClicked(Item item)
    {
        if (item == null || item.prefab == null)
        {
            return;
        }

        EquipItem(item.prefab, item.category);
    }
}