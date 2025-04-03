// ?? SESSION STORE – Centralise les infos multijoueur (Lobby, JoinCode)
// ?????????????????????????????????????????????????????????????
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

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
} //
