using CharacterCustomization;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gère l'affichage et la synchronisation réseau des objets équipés sur un personnage.
/// </summary>
public class EquippedVisualsHandler : NetworkBehaviour
{
    #region 🔧 Données

    private readonly Dictionary<SlotType, GameObject> equippedVisuals = new();

    [Tooltip("Copier l'Animator du parent sur les habits instanciés")]
    [SerializeField] private bool copyAnimatorFromParent = true;

    private Transform bodyRoot;
    private Animator referenceAnimator;

    #endregion

    #region 🚀 Initialisation

    private void Awake()
    {
        referenceAnimator = GetComponentInParent<Animator>();

        if (referenceAnimator == null)
        {
            Debug.LogError("[EquippedVisualsHandler] Aucun Animator trouvé dans le parent !");
            return;
        }

        bodyRoot = referenceAnimator.transform;
    }

    #endregion

    #region 🧥 Gestion des habits

    /// <summary>
    /// Équipe un prefab (habit) dans un slot spécifique. Instancié et synchronisé si possible.
    /// </summary>
    public void Equip(SlotType slotType, GameObject prefab)
    {
        Unequip(slotType);

        if (prefab == null)
        {
            Debug.LogWarning($"[EquippedVisualsHandler] Prefab null pour {slotType}");
            return;
        }

        GameObject instance;

        // ✅ Si serveur et prefab a un NetworkObject, on le spawn pour tous
        if (NetworkManager.Singleton.IsServer && prefab.TryGetComponent(out NetworkObject _))
        {
            instance = Instantiate(prefab, bodyRoot);

            var netObj = instance.GetComponent<NetworkObject>();
            if (!netObj.IsSpawned)
                netObj.Spawn(true); // true = ownership sur le serveur uniquement

            Debug.Log($"[EquippedVisualsHandler] 🔁 Habit {prefab.name} spawné via NetObj pour {slotType}");
        }
        else
        {
            // Fallback local uniquement (client / pas NetworkObject)
            instance = Instantiate(prefab, bodyRoot);
        }

        // ⚙️ Réinitialise la position relative
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        // 🎭 Copie de l'Animator si nécessaire
        if (copyAnimatorFromParent && referenceAnimator != null)
        {
            var instanceAnimator = instance.GetComponent<Animator>();
            if (instanceAnimator != null)
            {
                instanceAnimator.runtimeAnimatorController = referenceAnimator.runtimeAnimatorController;
            }
        }

        equippedVisuals[slotType] = instance;
    }

    /// <summary>
    /// Supprime un objet visuel d’un slot si déjà équipé.
    /// </summary>
    public void Unequip(SlotType slotType)
    {
        if (equippedVisuals.TryGetValue(slotType, out var obj) && obj != null)
        {
            if (NetworkManager.Singleton.IsServer && obj.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(obj);
            }

            equippedVisuals.Remove(slotType);
        }
    }

    /// <summary>
    /// Supprime tous les objets équipés actuels.
    /// </summary>
    public void ClearAll()
    {
        foreach (var obj in equippedVisuals.Values)
        {
            if (obj == null) continue;

            if (NetworkManager.Singleton.IsServer && obj.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
            {
                netObj.Despawn(true);
            }
            else
            {
                Destroy(obj);
            }
        }
        equippedVisuals.Clear();
    }

    #endregion
}