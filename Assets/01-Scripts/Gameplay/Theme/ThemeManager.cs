using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// G�re la s�lection al�atoire des cat�gories et th�mes pour chaque partie.
/// Chargement automatique des ThemeData depuis Resources.
/// Synchronisation du th�me s�lectionn� avec tous les clients.
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
    /// Charge tous les ThemeData pr�sents dans Resources/Themes, en excluant ceux marqu�s.
    /// </summary>
    private void LoadAllThemes()
    {
        availableThemes = Resources.LoadAll<ThemeData>("Themes")
                                   .Where(t => t.hideFlags != HideFlags.DontSave)
                                   .ToList();

        Debug.Log($"[ThemeManager] {availableThemes.Count} th�mes charg�s automatiquement.");
    }

    #endregion

    #region S�lection Al�atoire

    /// <summary>
    /// Lance la s�lection d'une cat�gorie puis d'un th�me correspondant.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectRandomThemeServerRpc()
    {
        if (availableThemes == null || availableThemes.Count == 0)
        {
            Debug.LogWarning("[ThemeManager] Aucun th�me disponible pour la s�lection.");
            return;
        }

        var categories = System.Enum.GetValues(typeof(ThemeData.ThemeCategory)).Cast<ThemeData.ThemeCategory>().ToList();
        ThemeData.ThemeCategory randomCategory = categories[Random.Range(0, categories.Count)];

        ShowCategoryToClientsClientRpc(randomCategory.ToString());

        List<ThemeData> themesInCategory = availableThemes.Where(t => t.category == randomCategory).ToList();

        if (themesInCategory.Count == 0)
        {
            Debug.LogWarning($"[ThemeManager] Aucun th�me trouv� pour la cat�gorie : {randomCategory}");
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
        Debug.Log($"[ThemeManager] Cat�gorie s�lectionn�e : {categoryName}");
        ThemeUIManager.Instance.ShowCategory(categoryName);
    }

    [ClientRpc]
    private void ShowThemeToClientsClientRpc(string themeName)
    {
        Debug.Log($"[ThemeManager] Th�me s�lectionn� : {themeName}");
        ThemeUIManager.Instance.ShowTheme(themeName);
    }

    #endregion

    #region Debug

#if UNITY_EDITOR
    [ContextMenu("Afficher les th�mes charg�s")]
    private void LogThemes()
    {
        foreach (var theme in availableThemes)
            Debug.Log($"- {theme.themeName} ({theme.category})");
    }
#endif

    #endregion
}