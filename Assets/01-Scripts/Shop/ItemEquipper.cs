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
    public GameObject character; // Le personnage sur lequel les items seront équipés

    private Dictionary<SlotType, GameObject> equippedItems = new Dictionary<SlotType, GameObject>();

    /// <summary>
    /// Équipe un item sur le personnage.
    /// </summary>
    /// <param name="itemPrefab">Le prefab de l'item à équiper.</param>
    /// <param name="slotType">Le type de l'item (ex: Hat, Glasses).</param>
    public void EquipItem(GameObject itemPrefab, SlotType slotType)
    {
        // Trouver le point d'attache correspondant au type d'item
        EquipPoint equipPoint = equipPoints.Find(point => point.slotType == slotType);
        if (equipPoint == null)
        {
            Debug.LogWarning($"Aucun point d'attache trouvé pour le type {slotType}");
            return;
        }

        // Si un item est déjà équipé à ce slot, le retirer
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

        // Ajouter l'item au dictionnaire des items équipés
        equippedItems[slotType] = newItem;
    }

    /// <summary>
    /// Méthode à appeler lorsqu'un bouton est cliqué.
    /// </summary>
    /// <param name="item">L'item à équiper.</param>
    public void OnItemButtonClicked(Item item)
    {
        if (item == null || item.prefab == null)
        {
            Debug.LogWarning("L'item ou son prefab est null.");
            return;
        }

        EquipItem(item.prefab, item.category);
    }
}
