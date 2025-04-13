using Unity.Netcode;
using UnityEngine;
using CharacterCustomization;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("📦 Slot Library")]
    [SerializeField] private SlotLibrary slotLibrary;

    [Header("🧍 Character Prefab")]
    [SerializeField] private GameObject characterPrefab;

    public GameObject CharacterInstance { get; private set; }
    public CharacterCustomization.CharacterCustomization CharacterLogic { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            SpawnCharacterInstance(); // centralisé
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        float x = 9.64f + (clientId * 2.5f);
        return new Vector3(x, 5.03f, -4f);
    }

    public void SpawnCharacterInstance()
    {
        if (CharacterInstance != null) return; // déjà instancié

        Transform parent = GameObject.Find("Environment")?.transform.Find("CharacterPlayer");
        if (parent == null)
        {
            Debug.LogError("[NetworkPlayer] Environment/CharacterPlayer introuvable.");
            return;
        }

        Vector3 pos = GetSpawnPosition(OwnerClientId);
        Quaternion rot = Quaternion.Euler(0f, 150f, 0f);

        CharacterLogic = new CharacterCustomization.CharacterCustomization(characterPrefab, slotLibrary);
        CharacterInstance = CharacterLogic.CharacterInstance;
        CharacterInstance.transform.SetParent(parent);
        CharacterInstance.transform.SetPositionAndRotation(pos, rot);

        if (CharacterInstance.GetComponent<EquippedVisualsHandler>() == null)
            CharacterInstance.AddComponent<EquippedVisualsHandler>();

        Debug.Log("[NetworkPlayer] Personnage instancié à la fin du décompte.");
    }
}
