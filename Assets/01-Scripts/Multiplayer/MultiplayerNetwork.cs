using Unity.Netcode;
using UnityEngine;

public class MultiplayerNetwork : NetworkBehaviour
{
    public static MultiplayerNetwork Instance { get; private set; }

    public NetworkVariable<int> ReadyCount = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> PlayerCount = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> SelectedGameMode = new(writePerm: NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            PlayerCount.OnValueChanged += (oldVal, newVal) =>
            {
                var ui = FindAnyObjectByType<MultiplayerUI>();
                if (ui != null)
                    ui.UpdateReadyCount(ReadyCount.Value, newVal); // 🔁 avec ready actuel
            };

            ReadyCount.OnValueChanged += (oldVal, newVal) =>
            {
                var ui = FindAnyObjectByType<MultiplayerUI>();
                if (ui != null)
                    ui.UpdateReadyCount(newVal, PlayerCount.Value); // 🔁 avec total actuel
            };
        }
    }

}
