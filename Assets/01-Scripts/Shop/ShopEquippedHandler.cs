using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    public class ShopEquippedHandler : MonoBehaviour
    {
        [Tooltip("Le Transform sur lequel les équipements doivent être attachés.")]
        [SerializeField] private Transform bodyTarget;

        private Dictionary<SlotType, GameObject> equippedVisuals = new Dictionary<SlotType, GameObject>();

        private void Awake()
        {
            if (bodyTarget == null)
            {
                Debug.LogError($"[ShopEquippedHandler] ? Aucun bodyTarget assigné sur {gameObject.name}.");
            }
        }

        public void EquipItem(Item item)
        {
            if (item == null || item.prefab == null || bodyTarget == null)
            {
                Debug.LogWarning("[ShopEquippedHandler] Equipement impossible : item, prefab ou bodyTarget manquant.");
                return;
            }

            // Détruire l'ancien objet du même slot
            Unequip(item.category);

            // Instancier le nouveau prefab
            GameObject instance = Instantiate(item.prefab, bodyTarget);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            equippedVisuals[item.category] = instance;

            Debug.Log($"[ShopEquippedHandler] ? {item.itemName} équipé sur le slot {item.category}.");
        }

        public void Unequip(SlotType slotType)
        {
            if (equippedVisuals.TryGetValue(slotType, out GameObject oldObj) && oldObj != null)
            {
                Destroy(oldObj);
                equippedVisuals.Remove(slotType);

                Debug.Log($"[ShopEquippedHandler] ?? Dééquipé slot {slotType}.");
            }
        }

        public void ClearAll()
        {
            foreach (var obj in equippedVisuals.Values)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            equippedVisuals.Clear();
        }
    }
}
