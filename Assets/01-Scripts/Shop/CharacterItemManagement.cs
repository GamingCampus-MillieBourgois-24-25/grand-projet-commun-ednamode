using System.Collections.Generic;
using UnityEngine;

namespace CharacterCustomization
{
    public class CharacterItemManager : MonoBehaviour
    {
        [System.Serializable]
        public class SlotAttachment
        {
            public SlotType slotType;
            public Transform attachmentPoint;
        }

        [Header("Points d'attache (optionnel)")]
        public List<SlotAttachment> slotAttachments;

        [Header("Cible pour équipement")]
        [Tooltip("Transform du body du joueur (ex: RootBody)")]
        [SerializeField] private Transform bodyTarget;

        [Tooltip("Copier l'Animator du parent")]
        [SerializeField] private bool copyAnimatorFromParent = true;

        private Dictionary<SlotType, GameObject> equippedItems = new Dictionary<SlotType, GameObject>();
        private Animator referenceAnimator;
        private SkinnedMeshRenderer bodySkinned;

        private void Awake()
        {
            if (bodyTarget == null)
            {
                Debug.LogError("[CharacterItemManager] bodyTarget non assigné dans l'Inspector !");
            }

            referenceAnimator = GetComponentInParent<Animator>();
            if (referenceAnimator == null)
            {
                Debug.LogWarning("[CharacterItemManager] Aucun Animator trouvé dans le parent.");
            }

            bodySkinned = bodyTarget?.GetComponentInChildren<SkinnedMeshRenderer>();
            if (bodySkinned == null)
            {
                Debug.LogWarning("[CharacterItemManager] Aucun SkinnedMeshRenderer trouvé sur bodyTarget.");
            }
        }

        public void EquipSingleItemForShop(Item item)
        {
            if (item == null || item.prefab == null)
            {
                Debug.LogError($"[CharacterItemManager] L'item ou son prefab est null ! Item: {item?.name}");
                return;
            }

            if (bodyTarget == null)
            {
                Debug.LogError("[CharacterItemManager] bodyTarget non assigné !");
                return;
            }

            // Déséquiper tous les items
            ClearAllEquippedItems();

            // Instancier l'item
            Debug.Log($"[CharacterItemManager] Instanciation de {item.itemName} sur {bodyTarget.name}");
            GameObject instance = Instantiate(item.prefab, bodyTarget);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Synchroniser l'Animator
            if (copyAnimatorFromParent && referenceAnimator != null)
            {
                var animator = instance.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = referenceAnimator.runtimeAnimatorController;
                    Debug.Log($"[CharacterItemManager] Animator synchronisé pour {item.itemName}");
                }
            }

            // Synchroniser les os (SkinnedMeshRenderer)
            var skinned = instance.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinned != null && bodySkinned != null)
            {
                skinned.bones = bodySkinned.bones;
                skinned.rootBone = bodySkinned.rootBone;
                Debug.Log($"[CharacterItemManager] SkinnedMeshRenderer synchronisé pour {item.itemName}");
            }
            else
            {
                Debug.LogWarning($"[CharacterItemManager] SkinnedMeshRenderer non trouvé pour {item.itemName} ou bodyTarget.");
            }

            // Stocker l'item
            equippedItems[item.category] = instance;

            Debug.Log($"[CharacterItemManager] Item {item.itemName} équipé sur {bodyTarget.name}.");
        }

        public void ClearAllEquippedItems()
        {
            Debug.Log("[CharacterItemManager] Déséquipement de tous les items...");
            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    Debug.Log($"[CharacterItemManager] Destruction de {item.name}");
                    Destroy(item);
                }
            }
            equippedItems.Clear();
        }
    }
}