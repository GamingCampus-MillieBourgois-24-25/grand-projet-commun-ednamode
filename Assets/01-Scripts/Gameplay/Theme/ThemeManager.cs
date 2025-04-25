using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Gère la sélection aléatoire des catégories et thèmes pour chaque partie.
/// Chargement automatique des ThemeData depuis Resources.
/// Synchronisation du thème sélectionné avec tous les clients.
/// </summary>
public class ThemeManager : NetworkBehaviour
{
    #region Singleton

    public static ThemeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadAllThemes();
    }

    #endregion

    #region Variables

    private List<ThemeData> availableThemes = new List<ThemeData>();

    private NetworkVariable<int> selectedThemeIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public ThemeData CurrentTheme => (selectedThemeIndex.Value >= 0 && selectedThemeIndex.Value < availableThemes.Count)
                                        ? availableThemes[selectedThemeIndex.Value]
                                        : null;

    #endregion

    #region Chargement Automatique

    /// <summary>
    /// Charge tous les ThemeData présents dans Resources/Themes, en excluant ceux marqués.
    /// </summary>
    private void LoadAllThemes()
    {
        availableThemes = Resources.LoadAll<ThemeData>("Themes")
                                   .Where(t => t.hideFlags != HideFlags.DontSave)
                                   .ToList();

        Debug.Log($"[ThemeManager] {availableThemes.Count} thèmes chargés automatiquement.");
    }

    #endregion

    #region Sélection Aléatoire

    /// <summary>
    /// Lance la sélection d'une catégorie puis d'un thème correspondant.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectRandomThemeServerRpc()
    {
        if (availableThemes == null || availableThemes.Count == 0)
        {
            Debug.LogWarning("[ThemeManager] Aucun thème disponible pour la sélection.");
            return;
        }

        var categories = System.Enum.GetValues(typeof(ThemeData.ThemeCategory)).Cast<ThemeData.ThemeCategory>().ToList();
        ThemeData.ThemeCategory randomCategory = categories[Random.Range(0, categories.Count)];

        ShowCategoryToClientsClientRpc(randomCategory.ToString());

        List<ThemeData> themesInCategory = availableThemes.Where(t => t.category == randomCategory).ToList();

        if (themesInCategory.Count == 0)
        {
            Debug.LogWarning($"[ThemeManager] Aucun thème trouvé pour la catégorie : {randomCategory}");
            return;
        }

        ThemeData chosenTheme = themesInCategory[Random.Range(0, themesInCategory.Count)];
        selectedThemeIndex.Value = availableThemes.IndexOf(chosenTheme);

        ShowThemeToClientsClientRpc(chosenTheme.themeName);
    }

    #endregion

    #region RPC - Synchronisation UI

    [ClientRpc]
    private void ShowCategoryToClientsClientRpc(string categoryName)
    {
        Debug.Log($"[ThemeManager] Catégorie sélectionnée : {categoryName}");
        ThemeUIManager.Instance.ShowCategory(categoryName);
    }

    [ClientRpc]
    private void ShowThemeToClientsClientRpc(string themeName)
    {
        Debug.Log($"[ThemeManager] Thème sélectionné : {themeName}");
        ThemeUIManager.Instance.ShowTheme(themeName);
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    [ContextMenu("Afficher les thèmes chargés")]
    private void LogThemes()
    {
        foreach (var theme in availableThemes)
            Debug.Log($"- {theme.themeName} ({theme.category})");
    }
#endif

    #endregion
}