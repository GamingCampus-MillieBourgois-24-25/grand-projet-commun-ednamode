using CharacterCustomization;
using System.Collections.Generic;
using UnityEngine;

public class EquippedVisualsHandler : MonoBehaviour
{
    private readonly Dictionary<SlotType, GameObject> equippedVisuals = new();
    [SerializeField] private bool copyAnimatorFromParent = true;

    private Transform bodyRoot;
    private Animator referenceAnimator;

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


    public void Equip(SlotType slotType, GameObject prefab)
    {
        Unequip(slotType);

        if (prefab == null)
        {
            Debug.LogWarning($"[EquippedVisualsHandler] Prefab null pour {slotType}");
            return;
        }

        GameObject instance = Instantiate(prefab, bodyRoot);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

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

    public void Unequip(SlotType slotType)
    {
        if (equippedVisuals.TryGetValue(slotType, out var obj) && obj != null)
        {
            Destroy(obj);
            equippedVisuals.Remove(slotType);
        }
    }

    public void ClearAll()
    {
        foreach (var obj in equippedVisuals.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        equippedVisuals.Clear();
    }
}