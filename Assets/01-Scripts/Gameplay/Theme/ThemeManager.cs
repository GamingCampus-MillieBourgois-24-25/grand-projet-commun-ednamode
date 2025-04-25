using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ThemeManager : NetworkBehaviour
{
    public static ThemeManager Instance { get; private set; }

    private List<ThemeData> availableThemes = new List<ThemeData>();

    private NetworkVariable<int> selectedThemeIndex = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<ulong> impostorClientId = new(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private ThemeData lastChosenTheme = null;

    public ThemeData CurrentTheme => (selectedThemeIndex.Value >= 0 && selectedThemeIndex.Value < availableThemes.Count)
                                        ? availableThemes[selectedThemeIndex.Value]
                                        : null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadAllThemes();
    }

    private void LoadAllThemes()
    {
        availableThemes = Resources.LoadAll<ThemeData>("Themes")
                                   .Where(t => t.hideFlags != HideFlags.DontSave)
                                   .ToList();
        Debug.Log($"[ThemeManager] {availableThemes.Count} thèmes chargés.");
    }

    public IEnumerator LaunchThemeDisplaySequence()
    {
        if (IsServer)
            SelectThemeWithImpostorLogic();

        yield return new WaitForSeconds(ThemeUIManager.Instance.TotalDisplayTime);
    }

    private void SelectThemeWithImpostorLogic()
    {
        var categories = System.Enum.GetValues(typeof(ThemeData.ThemeCategory)).Cast<ThemeData.ThemeCategory>().ToList();
        var randomCategory = categories[Random.Range(0, categories.Count)];

        int selectedMode = MultiplayerNetwork.Instance.SelectedGameMode.Value;
        bool isImpostorMode = (selectedMode == 1);

        if (isImpostorMode)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            impostorClientId.Value = clients[Random.Range(0, clients.Count)].ClientId;
            Debug.Log($"[ThemeManager] 🎭 Imposteur désigné : Client {impostorClientId.Value}");
        }
        else
        {
            impostorClientId.Value = ulong.MaxValue;
        }

        var themesInCategory = availableThemes.Where(t => t.category == randomCategory).ToList();
        var chosenTheme = GetNonRepeatingRandomTheme(themesInCategory);
        selectedThemeIndex.Value = availableThemes.IndexOf(chosenTheme);

        Debug.Log($"[ThemeManager] Catégorie : {randomCategory} | Thème sélectionné : {chosenTheme.themeName}");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            bool isImpostor = isImpostorMode && client.ClientId == impostorClientId.Value;
            ShowThemeClientRpc(client.ClientId, randomCategory.ToString(), isImpostor ? "Imposteur !" : chosenTheme.themeName);
        }
    }

    private ThemeData GetNonRepeatingRandomTheme(List<ThemeData> themes)
    {
        if (themes.Count <= 1)
            return themes[0];

        ThemeData chosen;
        int attempts = 0;
        do
        {
            chosen = themes[Random.Range(0, themes.Count)];
            attempts++;
        } while (chosen == lastChosenTheme && attempts < 10);

        lastChosenTheme = chosen;
        return chosen;
    }

    [ClientRpc]
    private void ShowThemeClientRpc(ulong targetClientId, string categoryName, string themeName)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        ThemeUIManager.Instance.DisplayThemeSequence(categoryName, themeName);
    }
}
