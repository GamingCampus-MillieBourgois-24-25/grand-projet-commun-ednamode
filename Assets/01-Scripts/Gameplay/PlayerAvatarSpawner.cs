using CharacterCustomization;
using Unity.Netcode;
using UnityEngine;

public class PlayerAvatarSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private SlotLibrary slotLibrary;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Transform parent = GameObject.Find("Environment")?.transform.Find("CharacterPlayer");
        if (parent == null)
        {
            Debug.LogError("Parent not found!");
            return;
        }

        GameObject instance = Instantiate(characterPrefab, parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0, 150, 0);
        instance.transform.localScale = Vector3.one;

        var logic = new CharacterCustomization.CharacterCustomization(characterPrefab, slotLibrary);
        instance.name = "PlayerAvatar_" + OwnerClientId;

        var visuals = instance.GetComponent<EquippedVisualsHandler>() ?? instance.AddComponent<EquippedVisualsHandler>();
    }
}