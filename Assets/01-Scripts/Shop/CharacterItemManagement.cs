using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterItemManager : MonoBehaviour
    {
        [System.Serializable]
        public class SlotAttachment
        {
            public SlotType slotType; // Type de l'emplacement (ex: Hat, Glasses, etc.)
            public Transform attachmentPoint; // Point d'attache sur le personnage
        }

        [Header("Points d'attache")]
        public List<SlotAttachment> slotAttachments; // Liste des points d'attache pour chaque type d'item

        private Dictionary<SlotType, GameObject> equippedItems = new Dictionary<SlotType, GameObject>();

        /// <summary>
        /// Équipe un nouvel item sur le personnage.
        /// </summary>
        /// <param name="item">L'item à équiper.</param>
        public void EquipItem(Item item)
        {
            if (item == null || item.prefab == null)
            {
                Debug.LogError("L'item ou son prefab est null !");
                return;
            }

            // Trouver le point d'attache correspondant au type de l'item
            SlotAttachment attachment = slotAttachments.Find(a => a.slotType == item.category);
            if (attachment == null || attachment.attachmentPoint == null)
            {
                Debug.LogError($"Aucun point d'attache trouvé pour le type {item.category} !");
                return;
            }

            // Détruire l'item précédent s'il existe
            if (equippedItems.ContainsKey(item.category))
            {
                Destroy(equippedItems[item.category]);
                equippedItems.Remove(item.category);
            }

            // Instancier le nouvel item et l'attacher au point d'attache
            GameObject newItem = Instantiate(item.prefab, attachment.attachmentPoint);
            newItem.transform.localPosition = Vector3.zero;
            newItem.transform.localRotation = Quaternion.identity;
            newItem.transform.localScale = Vector3.one;

            // Ajouter l'item à la liste des items équipés
            equippedItems[item.category] = newItem;

            // Synchroniser les animations si nécessaire
            AnimationSync animationSync = newItem.GetComponent<AnimationSync>();
            if (animationSync != null)
            {
                animationSync.Initialize(this.gameObject);
            }

            Debug.Log($"Item {item.itemName} équipé sur {attachment.attachmentPoint.name}.");
        }
    }
}
