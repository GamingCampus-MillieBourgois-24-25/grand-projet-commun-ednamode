using UnityEngine;
using Unity.Netcode; // Pour Netcode for GameObjects
using TMPro; // Pour TextMeshPro
using System.Collections.Generic;

// Structure personnalisée pour encapsuler le thème et le rendre sérialisable
public struct ThemeData : INetworkSerializable
{
    public string theme;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref theme);
    }
}

public class ThemeSelector : NetworkBehaviour
{
    // Liste des thèmes possibles
    private List<string> themes = new List<string>()
    {
        "Soirée Chic",
        "Streetwear",
        "Années 80",
        "Plage",
        "Futuriste",
        "Steampunk",
        "Gothique",
        "Kawaii",
        "Sport",
        "Haute Couture"
    };

    // NetworkVariable avec notre type personnalisé
    private NetworkVariable<ThemeData> currentTheme = new NetworkVariable<ThemeData>(
        new ThemeData { theme = "" },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private TMP_Text themeText; // Référence au texte UI
    private bool themeRequested = false; // Pour éviter les doublons

    // Appelé quand l'objet est spawn en réseau
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("OnNetworkSpawn appelé - IsServer: " + IsServer + ", IsClient: " + IsClient);

        // Si c'est un client et le thème n'a pas encore été demandé, demander au serveur
        if (IsClient && !themeRequested)
        {
            RequestThemeServerRpc();
            themeRequested = true;
        }

        // Écouter les changements de thème
        currentTheme.OnValueChanged += UpdateThemeUI;

        // Mettre à jour l'UI immédiatement avec la valeur actuelle
        UpdateThemeUI(new ThemeData(), currentTheme.Value);
    }

    // Appelé quand l'objet est despawn
    public override void OnNetworkDespawn()
    {
        currentTheme.OnValueChanged -= UpdateThemeUI;
        base.OnNetworkDespawn();
    }

    // ServerRpc : Un client demande au serveur de définir le thème
    [ServerRpc(RequireOwnership = false)]
    private void RequestThemeServerRpc()
    {
        if (string.IsNullOrEmpty(currentTheme.Value.theme)) // S'assurer qu'un thème n'est pas déjà défini
        {
            Random.InitState((int)(Time.time * 1000));
            ThemeData newTheme = new ThemeData { theme = themes[Random.Range(0, themes.Count)] };
            currentTheme.Value = newTheme;
            Debug.Log("Serveur - Thème choisi via ServerRpc : " + newTheme.theme);
            SyncThemeClientRpc(newTheme); // Propager à tous les clients
        }
    }

    // ClientRpc : Propager le thème à tous les clients
    [ClientRpc]
    private void SyncThemeClientRpc(ThemeData theme)
    {
        currentTheme.Value = theme; // Mettre à jour la NetworkVariable pour cohérence
        UpdateThemeUI(new ThemeData(), theme);
        Debug.Log("Client - Thème synchronisé via ClientRpc : " + theme.theme);
    }

    // Met à jour l'affichage dans l'UI
    private void UpdateThemeUI(ThemeData oldValue, ThemeData newValue)
    {
        if (themeText != null)
        {
            if (string.IsNullOrEmpty(newValue.theme))
            {
                themeText.text = "Thème : Aucun thème sélectionné";
                Debug.LogWarning("Aucun thème assigné à currentTheme.Value.theme");
            }
            else
            {
                themeText.text = "Thème : " + newValue.theme;
                Debug.Log("UI mise à jour - Thème affiché : " + newValue.theme);
            }
        }
        else
        {
            Debug.LogWarning("ThemeText n'est pas assigné dans l'inspecteur !");
        }
    }

    // Méthode publique pour obtenir le thème actuel
    public string GetCurrentTheme()
    {
        return currentTheme.Value.theme;
    }

    // Pour tester un nouveau thème avec une touche (optionnel)
    void Update()
    {
        if (IsClient && Input.GetKeyDown(KeyCode.Space))
        {
            RequestThemeServerRpc();
        }
    }

    // Méthode pour forcer la sélection d'un thème (optionnel)
    public void ForceSelectTheme()
    {
        RequestThemeServerRpc();
    }
}