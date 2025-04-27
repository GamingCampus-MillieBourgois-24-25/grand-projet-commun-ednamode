using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ThemeManager : NetworkBehaviour
{
    public static ThemeManager Instance { get; private set; }

    private List<ThemeData> availableThemes = new List<ThemeData>();
    private readonly Dictionary<ThemeData.ThemeCategory, List<ThemeData>> themesByCategory = new Dictionary<ThemeData.ThemeCategory, List<ThemeData>>();

    private NetworkVariable<int> selectedThemeIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<ulong> impostorClientId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private ThemeData lastChosenTheme;
    private ThemeData.ThemeCategory? lastCategory;
    private readonly List<ThemeData> recentThemes = new List<ThemeData>();
    private const int MaxRecentThemes = 5; // Avoid repeating last 5 themes

    public ThemeData CurrentTheme => selectedThemeIndex.Value >= 0 && selectedThemeIndex.Value < availableThemes.Count
        ? availableThemes[selectedThemeIndex.Value]
        : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Lightweight seed for mobile
        int seed = System.DateTime.Now.Millisecond + Time.frameCount;
        Random.InitState(seed);

        LoadAllThemes();
    }

    private void OnEnable()
    {
        selectedThemeIndex.OnValueChanged += OnSelectedThemeIndexChanged;
    }

    private void OnDisable()
    {
        selectedThemeIndex.OnValueChanged -= OnSelectedThemeIndexChanged;
    }

    private void LoadAllThemes()
    {
        availableThemes = Resources.LoadAll<ThemeData>("Themes").ToList();
        themesByCategory.Clear();

        // Cache themes by category for faster lookup
        foreach (var theme in availableThemes)
        {
            if (theme.hideFlags == HideFlags.DontSave) continue;
            if (!themesByCategory.ContainsKey(theme.category))
                themesByCategory[theme.category] = new List<ThemeData>();
            themesByCategory[theme.category].Add(theme);
        }

        // Log theme and category counts
        int themeCount = availableThemes.Count;
        string categories = string.Join(", ", themesByCategory.Keys);
        Debug.Log($"[ThemeManager] {themeCount} thèmes chargés. Categories: {categories}");
        foreach (var kvp in themesByCategory)
        {
            Debug.Log($"[ThemeManager] Category {kvp.Key}: {kvp.Value.Count} themes");
        }
    }

    public IEnumerator LaunchThemeDisplaySequence()
    {
        if (IsServer)
            SelectThemeWithImpostorLogic();

        yield return new WaitForSeconds(ThemeUIManager.Instance.TotalDisplayTime);
    }

    private void SelectThemeWithImpostorLogic()
    {
        // Get available categories
        List<ThemeData.ThemeCategory> categories = new List<ThemeData.ThemeCategory>(themesByCategory.Keys);
        if (categories.Count == 0)
        {
            Debug.LogError("[ThemeManager] No valid categories found!");
            return;
        }

        // Select a random category, preferring different from lastCategory
        ThemeData.ThemeCategory randomCategory;
        if (categories.Count > 1 && lastCategory.HasValue)
        {
            List<ThemeData.ThemeCategory> validCategories = new List<ThemeData.ThemeCategory>(categories.Count);
            foreach (var cat in categories)
            {
                if (cat != lastCategory.Value)
                    validCategories.Add(cat);
            }
            randomCategory = validCategories[Random.Range(0, validCategories.Count)];
        }
        else
        {
            randomCategory = categories[Random.Range(0, categories.Count)];
        }
        lastCategory = randomCategory;

        // Handle impostor mode
        int selectedMode = MultiplayerNetwork.Instance.SelectedGameMode.Value;
        bool isImpostorMode = selectedMode == 1;
        if (isImpostorMode)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            impostorClientId.Value = clients[Random.Range(0, clients.Count)].ClientId;
        }
        else
        {
            impostorClientId.Value = ulong.MaxValue;
        }

        // Get themes in category
        if (!themesByCategory.TryGetValue(randomCategory, out var themesInCategory) || themesInCategory.Count == 0)
        {
            Debug.LogError($"[ThemeManager] No themes found in category {randomCategory}!");
            return;
        }

        // Select a theme, avoiding recent ones
        ThemeData chosenTheme = SelectNonRecentTheme(themesInCategory);
        recentThemes.Add(chosenTheme);
        if (recentThemes.Count > MaxRecentThemes)
            recentThemes.RemoveAt(0);
        lastChosenTheme = chosenTheme;

        selectedThemeIndex.Value = availableThemes.IndexOf(chosenTheme);

        // Notify clients
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            bool isImpostor = isImpostorMode && client.ClientId == impostorClientId.Value;
            ShowThemeClientRpc(client.ClientId, randomCategory.ToString(), isImpostor ? "Imposteur !" : chosenTheme.themeName);
        }
    }

    private ThemeData SelectNonRecentTheme(List<ThemeData> themes)
    {
        List<ThemeData> validThemes = new List<ThemeData>(themes.Count);
        foreach (var theme in themes)
        {
            if (!recentThemes.Contains(theme))
                validThemes.Add(theme);
        }

        if (validThemes.Count == 0)
        {
            validThemes = themes;
        }

        int index = Random.Range(0, validThemes.Count);
        return validThemes[index];
    }

    [ClientRpc]
    private void ShowThemeClientRpc(ulong targetClientId, string categoryName, string themeName)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        ThemeUIManager.Instance.DisplayThemeSequence(categoryName, themeName);
    }

    private void OnSelectedThemeIndexChanged(int oldValue, int newValue)
    {
    }

    public string GetCurrentThemeName()
    {
        return CurrentTheme?.themeName ?? "";
    }

    public void ResetThemeHistory()
    {
        recentThemes.Clear();
        lastChosenTheme = null;
        lastCategory = null;
    }
}