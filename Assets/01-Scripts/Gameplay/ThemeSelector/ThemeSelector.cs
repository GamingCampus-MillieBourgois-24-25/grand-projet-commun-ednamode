using UnityEngine;
using Unity.Netcode; // Pour Netcode for GameObjects
using TMPro; // Pour TextMeshPro
using System.Collections.Generic;

// Structure personnalis�e pour encapsuler le th�me et le rendre s�rialisable
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
    // Liste des th�mes possibles
    private List<string> themes = new List<string>()
    {
        "Soir�e Chic",
        "Streetwear",
        "Ann�es 80",
        "Plage",
        "Futuriste",
        "Steampunk",
        "Gothique",
        "Kawaii",
        "Sport",
        "Haute Couture"
    };

    // NetworkVariable avec notre type personnalis�
    private NetworkVariable<ThemeData> currentTheme = new NetworkVariable<ThemeData>(
        new ThemeData { theme = "" },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private TMP_Text themeText; // R�f�rence au texte UI
    private bool themeRequested = false; // Pour �viter les doublons

    // Appel� quand l'objet est spawn en r�seau
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("OnNetworkSpawn appel� - IsServer: " + IsServer + ", IsClient: " + IsClient);

        // Si c'est un client et le th�me n'a pas encore �t� demand�, demander au serveur
        if (IsClient && !themeRequested)
        {
            RequestThemeServerRpc();
            themeRequested = true;
        }

        // �couter les changements de th�me
        currentTheme.OnValueChanged += UpdateThemeUI;

        // Mettre � jour l'UI imm�diatement avec la valeur actuelle
        UpdateThemeUI(new ThemeData(), currentTheme.Value);
    }

    // Appel� quand l'objet est despawn
    public override void OnNetworkDespawn()
    {
        currentTheme.OnValueChanged -= UpdateThemeUI;
        base.OnNetworkDespawn();
    }

    // ServerRpc : Un client demande au serveur de d�finir le th�me
    [ServerRpc(RequireOwnership = false)]
    private void RequestThemeServerRpc()
    {
        if (string.IsNullOrEmpty(currentTheme.Value.theme)) // S'assurer qu'un th�me n'est pas d�j� d�fini
        {
            Random.InitState((int)(Time.time * 1000));
            ThemeData newTheme = new ThemeData { theme = themes[Random.Range(0, themes.Count)] };
            currentTheme.Value = newTheme;
            Debug.Log("Serveur - Th�me choisi via ServerRpc : " + newTheme.theme);
            SyncThemeClientRpc(newTheme); // Propager � tous les clients
        }
    }

    // ClientRpc : Propager le th�me � tous les clients
    [ClientRpc]
    private void SyncThemeClientRpc(ThemeData theme)
    {
        currentTheme.Value = theme; // Mettre � jour la NetworkVariable pour coh�rence
        UpdateThemeUI(new ThemeData(), theme);
        Debug.Log("Client - Th�me synchronis� via ClientRpc : " + theme.theme);
    }

    // Met � jour l'affichage dans l'UI
    private void UpdateThemeUI(ThemeData oldValue, ThemeData newValue)
    {
        if (themeText != null)
        {
            if (string.IsNullOrEmpty(newValue.theme))
            {
                themeText.text = "Th�me : Aucun th�me s�lectionn�";
                Debug.LogWarning("Aucun th�me assign� � currentTheme.Value.theme");
            }
            else
            {
                themeText.text = "Th�me : " + newValue.theme;
                Debug.Log("UI mise � jour - Th�me affich� : " + newValue.theme);
            }
        }
        else
        {
            Debug.LogWarning("ThemeText n'est pas assign� dans l'inspecteur !");
        }
    }

    // M�thode publique pour obtenir le th�me actuel
    public string GetCurrentTheme()
    {
        return currentTheme.Value.theme;
    }

    // Pour tester un nouveau th�me avec une touche (optionnel)
    void Update()
    {
        if (IsClient && Input.GetKeyDown(KeyCode.Space))
        {
            RequestThemeServerRpc();
        }
    }

    // M�thode pour forcer la s�lection d'un th�me (optionnel)
    public void ForceSelectTheme()
    {
        RequestThemeServerRpc();
    }
}