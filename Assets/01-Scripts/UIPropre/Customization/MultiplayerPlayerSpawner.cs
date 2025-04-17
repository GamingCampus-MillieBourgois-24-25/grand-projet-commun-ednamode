using Unity.Netcode;
using UnityEngine;
using CharacterCustomization;

public class MultiplayerPlayerSpawner : NetworkBehaviour
{
    [Header("🧍 Prefab joueur customisé")]
    [SerializeField] private GameObject characterPrefab;

    [Header("📚 SlotLibrary")]
    [SerializeField] private SlotLibrary slotLibrary;

    [Header("🎥 Caméra principale")]
    [SerializeField] private Camera mainCamera;

    private GameObject localCharacterInstance;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Vector3 spawnPosition = GetSpawnPosition(NetworkManager.Singleton.LocalClientId);
        Quaternion spawnRotation = Quaternion.Euler(0f, 150f, 0f);

        // 🔍 Recherche du parent "Environment/CharacterPlayer"
        Transform parent = GameObject.Find("Environment")?.transform.Find("CharacterPlayer");

        if (parent == null)
        {
            Debug.LogError("[Spawner] Le parent 'Environment/CharacterPlayer' est introuvable.");
            return;
        }

        // 🧍 Instanciation du personnage en enfant du bon parent
        var character = new CharacterCustomization.CharacterCustomization(characterPrefab, slotLibrary);
        localCharacterInstance = character.CharacterInstance;
        localCharacterInstance.transform.SetParent(parent);
        localCharacterInstance.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        // 🎥 Caméra
        SetupCameraFollow(localCharacterInstance);
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        float x = 9.64f + (clientId * 2.5f);
        return new Vector3(x, 5.03f, -4f);
    }

    private void SetupCameraFollow(GameObject character)
    {
        if (mainCamera == null) return;

        mainCamera.transform.position = character.transform.position + new Vector3(0, 2, -5);
        mainCamera.transform.LookAt(character.transform);
    }
}
