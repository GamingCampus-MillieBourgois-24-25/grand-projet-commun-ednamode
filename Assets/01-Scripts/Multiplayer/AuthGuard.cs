// ?? AUTH GUARD � G�re l'authentification Unity pour �viter les conflits
// Emp�che les appels simultan�s � SignInAnonymouslyAsync()

using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthGuard
{
    private static bool isAuthenticating = false;

    /// <summary>
    /// Initialise Unity Services et se connecte anonymement si n�cessaire (une seule fois).
    /// </summary>
    public static async Task EnsureSignedInAsync()
    {
        if (isAuthenticating)
            return;

        isAuthenticating = true;

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("AuthGuard: Connect� en tant que " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("AuthGuard: Erreur d'authentification � " + e.Message);
        }
        finally
        {
            isAuthenticating = false;
        }
    }

}
