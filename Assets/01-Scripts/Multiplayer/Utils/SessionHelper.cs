using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public static class SessionHelper
{
    /// <summary>
    /// Retourne true si le joueur local est le premier joueur du lobby.
    /// </summary>
    public static bool IsLocalPlayerHost()
    {
        var lobby = SessionStore.Instance?.CurrentLobby;
        if (lobby == null || lobby.Players == null || !AuthenticationService.Instance.IsSignedIn)
            return false;

        string localPlayerId = AuthenticationService.Instance.PlayerId;
        string firstPlayerId = lobby.Players.FirstOrDefault()?.Id;

        return localPlayerId == firstPlayerId;
    }

    /// <summary>
    /// Retourne l'ID du joueur local.
    /// </summary>
    public static string GetLocalPlayerId()
    {
        return AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : null;
    }

    /// <summary>
    /// Retourne l'ID du premier joueur dans le lobby (host logique).
    /// </summary>
    public static string GetHostPlayerId()
    {
        return SessionStore.Instance?.CurrentLobby?.Players?.FirstOrDefault()?.Id;
    }

    /// <summary>
    /// Retourne le Lobby en cours.
    /// </summary>
    public static Lobby GetLobby()
    {
        return SessionStore.Instance?.CurrentLobby;
    }
}
