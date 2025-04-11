// SESSION STORE – Centralise les infos multijoueur (Lobby, JoinCode)
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using Unity.Services.Authentication;

public class SessionStore : MonoBehaviour
{
    public static SessionStore Instance { get; private set; }

    [Header("Session Info")]
    public Lobby CurrentLobby { get; private set; }
    public string JoinCode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLobby(Lobby lobby)
    {
        CurrentLobby = lobby;
        JoinCode = lobby?.Data != null && lobby.Data.ContainsKey("joinCode")
            ? lobby.Data["joinCode"].Value
            : null;
    }

    public void Clear()
    {
        CurrentLobby = null;
        JoinCode = null;
    }

    private Dictionary<ulong, string> clientIdToPlayerId = new();
    private Dictionary<string, ulong> playerIdToClientId = new();

    public void RegisterClient(ulong clientId, string playerId)
    {
        if (!clientIdToPlayerId.ContainsKey(clientId))
            clientIdToPlayerId.Add(clientId, playerId);

        if (!playerIdToClientId.ContainsKey(playerId))
            playerIdToClientId.Add(playerId, clientId);
    }

    public string GetPlayerId(ulong clientId)
    {
        return clientIdToPlayerId.TryGetValue(clientId, out var id) ? id : null;
    }

    public ulong GetClientId(string playerId)
    {
        return playerIdToClientId.TryGetValue(playerId, out var cid) ? cid : ulong.MaxValue;
    }

    public string GetPlayerName(string playerId)
    {
        if (CurrentLobby == null) return playerId;

        foreach (var player in CurrentLobby.Players)
        {
            if (player.Id == playerId)
            {
                if (player.Data.TryGetValue("name", out var data))
                    return data.Value;
            }
        }

        return playerId;
    }


    public string GetLocalPlayerId()
    {
        return AuthenticationService.Instance?.PlayerId;
    }
}
