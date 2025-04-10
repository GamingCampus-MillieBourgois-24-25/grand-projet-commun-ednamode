using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    public delegate void MessageReceivedDelegate(string sender, string message);
    public static event MessageReceivedDelegate OnMessageReceived;

    private void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void SendChat(string message)
    {
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        var playerId = AuthenticationService.Instance?.PlayerId;
        var sender = SessionStore.Instance?.GetPlayerName(playerId) ?? playerId;
        SendChatMessageServerRpc(sender, message);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string sender, string message, ServerRpcParams rpcParams = default)
    {
        BroadcastMessageClientRpc(sender, message);
    }

    [ClientRpc]
    private void BroadcastMessageClientRpc(string sender, string message)
    {
        Debug.Log($"[Chat] {sender}: {message}");
        OnMessageReceived?.Invoke(sender, message);
    }
}
