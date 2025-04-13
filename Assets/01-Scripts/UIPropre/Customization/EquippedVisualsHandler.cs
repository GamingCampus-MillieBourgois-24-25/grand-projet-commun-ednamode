using CharacterCustomization;
using System.Collections.Generic;
using UnityEngine;

public class EquippedVisualsHandler : MonoBehaviour
{
    private readonly Dictionary<SlotType, GameObject> equippedVisuals = new();
    [SerializeField] private bool copyAnimatorFromParent = true;

    public void Equip(SlotType slotType, GameObject prefab)
    {
        Unequip(slotType);

        if (prefab == null)
        {
            Debug.LogWarning($"[EquippedVisualsHandler] Prefab null pour {slotType}");
            return;
        }

        GameObject instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        if (copyAnimatorFromParent)
        {
            var parentAnimator = GetComponent<Animator>();
            var instanceAnimator = instance.GetComponent<Animator>();
            if (parentAnimator != null && instanceAnimator != null)
            {
                instanceAnimator.runtimeAnimatorController = parentAnimator.runtimeAnimatorController;
                instanceAnimator.avatar = parentAnimator.avatar;
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