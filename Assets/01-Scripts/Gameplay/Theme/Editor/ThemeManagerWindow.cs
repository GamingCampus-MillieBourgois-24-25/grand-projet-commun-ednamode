using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Fenêtre d'édition et de gestion des thèmes du jeu.
/// Permet la création, modification, suppression, import/export des ThemeData.
/// </summary>
public class ThemeManagerWindow : EditorWindow
{
    #region Variables

    private ThemeData[] themes;
    private Vector2 scrollPos;

    private string searchKeyword = "";
    private bool showExcluded = false;
    private bool showOnlyFavorites = false;

    private string[] categoryOptions;
    private int selectedCategoryIndex = 0;

    private GUIStyle headerStyle;
    private const int gridColumns = 2;
    private const float cardSize = 250f;

    private Dictionary<ThemeData, string> tempNames = new Dictionary<ThemeData, string>();

    #endregion

    #region Initialisation

    [MenuItem("Outils/Theme Manager 🎨")]
    public static void ShowWindow() => GetWindow<ThemeManagerWindow>("Gestionnaire de Thèmes");

    private void OnEnable()
    {
        RefreshThemes();
        UpdateCategoryOptions();
        headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, normal = { textColor = Color.white } };
    }

    #endregion

    #region Rafraichissement & Filtres

    private void RefreshThemes()
    {
        themes = Resources.LoadAll<ThemeData>("Themes");
    }

    private void UpdateCategoryOptions()
    {
        var enumNames = System.Enum.GetNames(typeof(ThemeData.ThemeCategory));
        categoryOptions = new string[] { "Tous" }.Concat(enumNames).ToArray();
        if (selectedCategoryIndex >= categoryOptions.Length)
            selectedCategoryIndex = 0;
    }

    #endregion

    #region OnGUI Principal

    private void OnGUI()
    {
        GUILayout.Label("🎨 Gestionnaire de Thèmes", headerStyle);
        EditorGUILayout.Space();

        DrawFilters();
        DrawToolbar();
        ThemeExporter.HandleDragAndDrop();

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        var filteredThemes = ApplyFilters();
        if (filteredThemes.Count == 0)
            EditorGUILayout.HelpBox("Aucun thème ne correspond aux filtres.", MessageType.Info);

        DrawGridView(filteredThemes);

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region UI - Filtres & Toolbar

    /// <summary>
    /// Affiche les filtres de recherche et de tri.
    /// </summary>
    private void DrawFilters()
    {
        GUILayout.Label("🔎 Filtres", EditorStyles.boldLabel);
        searchKeyword = EditorGUILayout.TextField(new GUIContent("Recherche", "Filtrer par nom"), searchKeyword);

        if (categoryOptions == null || categoryOptions.Length == 0)
            UpdateCategoryOptions();

        selectedCategoryIndex = EditorGUILayout.Popup(new GUIContent("Catégorie", "Filtrer par catégorie"), selectedCategoryIndex, categoryOptions);
        showOnlyFavorites = EditorGUILayout.Toggle(new GUIContent("Afficher les favoris", "Afficher uniquement les thèmes favoris"), showOnlyFavorites);
        showExcluded = EditorGUILayout.Toggle(new GUIContent("Afficher les exclus", "Afficher les thèmes exclus du tirage"), showExcluded);

        if (GUILayout.Button("Réinitialiser les filtres"))
        {
            searchKeyword = "";
            selectedCategoryIndex = 0;
            showOnlyFavorites = false;
            showExcluded = false;
        }
    }

    /// <summary>
    /// Affiche les boutons d'action principaux : rafraîchir, créer, importer, exporter.
    /// </summary>
    private void DrawToolbar()
    {
        EditorGUILayout.Space();
        GUILayout.Label("⚙️ Outils", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("🔄 Rafraîchir"))
        {
            RefreshThemes();
            UpdateCategoryOptions();
        }

        if (GUILayout.Button("➕ Nouveau Thème"))
            CreateNewTheme();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("📤 Exporter"))
            ThemeExporter.Export(themes);

        if (GUILayout.Button("📥 Importer"))
        {
            ThemeExporter.Import();
            RefreshThemes();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(); EditorGUILayout.Space();
    }

    #endregion

    #region Filtrage & Affichage Grid

    private List<ThemeData> ApplyFilters()
    {
        IEnumerable<ThemeData> filtered = themes;
        if (!string.IsNullOrEmpty(searchKeyword))
            filtered = filtered.Where(t => t.themeName.ToLower().Contains(searchKeyword.ToLower()));

        if (selectedCategoryIndex > 0)
        {
            var selectedCategory = (ThemeData.ThemeCategory)(selectedCategoryIndex - 1);
            filtered = filtered.Where(t => t.category == selectedCategory);
        }

        if (showOnlyFavorites)
            filtered = filtered.Where(t => t.isFavorite);

        if (!showExcluded)
            filtered = filtered.Where(t => t.hideFlags != HideFlags.DontSave);

        return filtered.OrderBy(t => t.themeName).ToList();
    }

    private void DrawGridView(List<ThemeData> themesToDisplay)
    {
        int rowCount = Mathf.CeilToInt((float)themesToDisplay.Count / gridColumns);
        for (int row = 0; row < rowCount; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < gridColumns; col++)
            {
                int index = row * gridColumns + col;
                if (index >= themesToDisplay.Count)
                    break;

                DrawThemeCard(themesToDisplay[index]);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }

    #endregion

    #region UI - Card Thème

    private void DrawThemeCard(ThemeData theme)
    {
        Color bgColor = GetCategoryColor(theme.category);
        GUI.backgroundColor = bgColor;

        EditorGUILayout.BeginVertical("box", GUILayout.Width(cardSize), GUILayout.Height(cardSize));

        if (!tempNames.ContainsKey(theme))
            tempNames[theme] = theme.themeName;

        EditorGUI.BeginChangeCheck();
        tempNames[theme] = EditorGUILayout.TextField("Nom", tempNames[theme]);
        if (!EditorGUIUtility.editingTextField && tempNames[theme] != theme.themeName && !string.IsNullOrWhiteSpace(tempNames[theme]))
        {
            RenameThemeAsset(theme, theme.themeName, tempNames[theme]);
            theme.themeName = tempNames[theme];
        }

        theme.description = EditorGUILayout.TextField("Description", theme.description);
        theme.category = (ThemeData.ThemeCategory)EditorGUILayout.EnumPopup("Catégorie", theme.category);
        theme.themeIcon = (Sprite)EditorGUILayout.ObjectField("Icône", theme.themeIcon, typeof(Sprite), false);

        if (GUILayout.Button(theme.isFavorite ? "⭐ Retirer Favori" : "☆ Ajouter Favori"))
            ToggleFavorite(theme);

        if (GUILayout.Button(theme.hideFlags == HideFlags.DontSave ? "Inclure" : "Exclure"))
            ToggleExclude(theme);

        if (GUILayout.Button("🗑️ Supprimer"))
            DeleteTheme(theme);

        if (!theme.themeIcon)
            EditorGUILayout.HelpBox("Icône manquante", MessageType.Warning);

        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;

        if (GUI.changed)
            EditorUtility.SetDirty(theme);
    }

    private Color GetCategoryColor(ThemeData.ThemeCategory category)
    {
        return category switch
        {
            ThemeData.ThemeCategory.Futuristic => Color.cyan,
            ThemeData.ThemeCategory.Fantasy => new Color(0.6f, 0.4f, 0.8f),
            ThemeData.ThemeCategory.Retro => new Color(1f, 0.65f, 0f),
            ThemeData.ThemeCategory.Casual => Color.green,
            ThemeData.ThemeCategory.Formal => Color.gray,
            _ => Color.white,
        };
    }

    #endregion

    #region Actions Thèmes

    private void ToggleFavorite(ThemeData theme)
    {
        theme.isFavorite = !theme.isFavorite;
        EditorUtility.SetDirty(theme);
    }

    private void ToggleExclude(ThemeData theme)
    {
        theme.hideFlags = theme.hideFlags == HideFlags.DontSave ? HideFlags.None : HideFlags.DontSave;
        EditorUtility.SetDirty(theme);
    }

    private void DeleteTheme(ThemeData theme)
    {
        if (EditorUtility.DisplayDialog("Supprimer le Thème", $"Voulez-vous vraiment supprimer '{theme.themeName}' ?", "Oui", "Non"))
        {
            string path = AssetDatabase.GetAssetPath(theme);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            RefreshThemes();
            UpdateCategoryOptions();
        }
    }

    private void CreateNewTheme()
    {
        var theme = ScriptableObject.CreateInstance<ThemeData>();
        theme.themeName = "Nouveau Thème";
        theme.description = "Description...";
        theme.category = ThemeData.ThemeCategory.Casual;
        AssetDatabase.CreateAsset(theme, $"Assets/Resources/Themes/{theme.themeName}.asset");
        AssetDatabase.SaveAssets();
        RefreshThemes();
    }

    private void RenameThemeAsset(ThemeData theme, string oldName, string newName)
    {
        string path = AssetDatabase.GetAssetPath(theme);
        string validName = MakeFilenameSafe(newName);

        string basePath = $"Assets/Resources/Themes/{validName}.asset";
        string finalPath = basePath;
        int counter = 1;

        while (AssetDatabase.LoadAssetAtPath<ThemeData>(finalPath) != null && finalPath != path)
        {
            finalPath = $"Assets/Resources/Themes/{validName}_{counter}.asset";
            counter++;
        }

        if (path != finalPath)
        {
            AssetDatabase.RenameAsset(path, System.IO.Path.GetFileNameWithoutExtension(finalPath));
            AssetDatabase.SaveAssets();
        }
    }

    private string MakeFilenameSafe(string name)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }

    #endregion
}