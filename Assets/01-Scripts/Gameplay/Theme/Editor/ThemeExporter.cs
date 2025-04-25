using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Gère l'exportation et l'importation des thèmes (ThemeData) via des fichiers JSON.
/// Supporte l'import par sélection de fichier ou drag & drop.
/// </summary>
public static class ThemeExporter
{
    private const string defaultExportPath = "Assets/Resources/Themes/ThemeExport.json";

    #region Export

    public static void Export(ThemeData[] themes)
    {
        var exportList = themes.Select(t => new ThemeExportData(t)).ToList();
        string json = JsonUtility.ToJson(new ThemeExportWrapper { themes = exportList }, true);
        File.WriteAllText(defaultExportPath, json);
        AssetDatabase.Refresh();
        Debug.Log($"[ThemeExporter] {themes.Length} thèmes exportés vers {defaultExportPath}");
    }

    #endregion

    #region Import avec File Picker

    public static void Import()
    {
        string path = EditorUtility.OpenFilePanel("Importer un fichier de thèmes", "", "json");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[ThemeImporter] Import annulé.");
            return;
        }
        ImportFromPath(path);
    }

    #endregion

    #region Import Principal

    public static void ImportFromPath(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("[ThemeImporter] Fichier introuvable : " + path);
            return;
        }

        string json = File.ReadAllText(path);
        var wrapper = JsonUtility.FromJson<ThemeExportWrapper>(json);

        int created = 0;
        int ignored = 0;

        foreach (var data in wrapper.themes)
        {
            string assetPath = $"Assets/Resources/Themes/{data.themeName}.asset";
            if (AssetDatabase.LoadAssetAtPath<ThemeData>(assetPath) != null)
            {
                ignored++;
                continue;
            }

            var theme = ScriptableObject.CreateInstance<ThemeData>();
            theme.themeName = data.themeName;
            theme.description = data.description;

            if (System.Enum.TryParse(data.category, true, out ThemeData.ThemeCategory parsedCategory))
            {
                theme.category = parsedCategory;
            }
            else
            {
                theme.category = ThemeData.ThemeCategory.Casual;  // Par défaut si erreur
                Debug.LogWarning($"[ThemeImporter] Catégorie inconnue pour le thème '{data.themeName}', assigné à Casual.");
            }

            AssetDatabase.CreateAsset(theme, assetPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ThemeImporter] Import terminé : {created} créés, {ignored} ignorés.");
    }

    #endregion

    #region Drag & Drop Handler

    public static void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "📂 Glissez-déposez ici votre fichier JSON");

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (!dropArea.Contains(evt.mousePosition))
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var draggedPath in DragAndDrop.paths)
                {
                    if (draggedPath.EndsWith(".json"))
                    {
                        ImportFromPath(draggedPath);
                    }
                    else
                    {
                        Debug.LogWarning($"[ThemeImporter] Fichier ignoré : {draggedPath}");
                    }
                }
            }
            Event.current.Use();
        }
    }

    #endregion

    #region Structures

    [System.Serializable]
    private class ThemeExportWrapper
    {
        public List<ThemeExportData> themes;
    }

    [System.Serializable]
    private class ThemeExportData
    {
        public string themeName;
        public string description;
        public string category;

        public ThemeExportData(ThemeData theme)
        {
            themeName = theme.themeName;
            description = theme.description;
            category = theme.category.ToString();
        }
    }

    #endregion
}
