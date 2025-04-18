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

    [Tooltip("Nom de l'objet enfant à équiper (ex: RootBody, MeshBody, etc.)")]
    [SerializeField] private string targetMeshName = "RootBody";

    [Tooltip("Copier l'Animator du parent sur les habits instanciés")]
    [SerializeField] private bool copyAnimatorFromParent = true;

    private Transform bodyTarget;
    private Animator referenceAnimator;

    public string GetTargetMeshName() => targetMeshName;
    #endregion

    #region 🚀 Initialisation

    private void Awake()
    {
        // Recherche dynamique du mesh cible pour équiper les habits
        bodyTarget = transform.Find(targetMeshName);

        if (bodyTarget == null)
        {
            Debug.LogError($"[EquippedVisualsHandler] ❌ Aucun enfant nommé '{targetMeshName}' trouvé dans {gameObject.name}. L'équipement ne sera pas visible.");
        }

        referenceAnimator = GetComponentInParent<Animator>();

        if (referenceAnimator == null)
        {
            Debug.LogError("[EquippedVisualsHandler] Aucun Animator trouvé dans le parent !");
            return;
        }
        else
        {
            Debug.Log($"[EquippedVisualsHandler] Animator trouvé : {referenceAnimator.name}");
        }
            bodyTarget = referenceAnimator.transform;
    }

    #endregion

    #region 🧥 Gestion des habits

    public void Equip(SlotType slotType, GameObject prefab)
    {
        Equip(slotType, prefab, Color.white, null); // Appel de la version complète
    }

    /// <summary>
    /// Équipe un prefab (habit) dans un slot spécifique. Instancié et synchronisé si possible.
    /// </summary>
    /// <summary>
    /// Équipe un prefab (habit) dans un slot spécifique. Instancié et synchronisé si possible, sans duplication.
    /// </summary>
    /// <summary>
    /// Équipe un prefab (habit) dans un slot spécifique, avec couleur et texture.
    /// </summary>
    public void Equip(SlotType slotType, GameObject prefab, Color color, string textureName)
    {
        // 🔁 Supprime l'existant
        Unequip(slotType);

        if (prefab == null)
        {
            Debug.LogWarning($"[EquippedVisualsHandler] ❌ Prefab null pour {slotType}");
            return;
        }

        // 🔧 Instanciation
        GameObject instance = Instantiate(prefab);

        // 🎯 Placement hiérarchique
        instance.transform.SetParent(bodyTarget, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        // 🎭 Animation
        if (copyAnimatorFromParent && referenceAnimator != null)
        {
            var animator = instance.GetComponent<Animator>();
            if (animator != null)
                animator.runtimeAnimatorController = referenceAnimator.runtimeAnimatorController;
        }

        // 🎨 Appliquer couleur
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat != null)
                    mat.color = color;
            }
        }
        var skinned = instance.GetComponentInChildren<SkinnedMeshRenderer>();
        var bodySkinned = GetComponentInChildren<SkinnedMeshRenderer>();

        if (skinned != null && bodySkinned != null)
        {
            skinned.bones = bodySkinned.bones;
            skinned.rootBone = bodySkinned.rootBone;
        }
        else
        {
            Debug.LogWarning($"[EquippedVisualsHandler] ⚠️ SkinnedMeshRenderer non trouvé pour {slotType}");
        }

        // 🧵 Appliquer texture
        if (!string.IsNullOrEmpty(textureName))
        {
            Texture tex = Resources.Load<Texture>($"Textures/{textureName}");
            if (tex != null)
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (mat != null)
                            mat.mainTexture = tex;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[EquippedVisualsHandler] ❌ Texture non trouvée : {textureName}");
            }
        }

        // 💾 Sauvegarde
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