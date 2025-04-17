using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using CharacterCustomization;
using System.Linq;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Fenêtre d’éditeur pour visualiser et interagir avec les tenues des joueurs en jeu.
/// </summary>
public class CustomizationDebuggerWindow : EditorWindow
{
    [MenuItem("Tools/👗 Customization Debugger")]
    public static void ShowWindow()
    {
        var window = GetWindow<CustomizationDebuggerWindow>();
        window.titleContent = new GUIContent("🧵 Customization Debugger");
        window.minSize = new Vector2(420, 300);
    }

    private ScrollView scrollView;
    private EnumField filterField;
    private PlayerFilter currentFilter = PlayerFilter.All;

    private enum PlayerFilter { All, LocalOnly, RemoteOnly }

    private void CreateGUI()
    {
        rootVisualElement.Clear();

        Label title = new Label("👥 Données des joueurs réseau");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.fontSize = 16;
        title.style.marginBottom = 8;

        filterField = new EnumField("Filtrer :", PlayerFilter.All);
        filterField.RegisterValueChangedCallback(evt =>
        {
            currentFilter = (PlayerFilter)evt.newValue;
            RefreshPanel();
        });

        scrollView = new ScrollView();
        scrollView.style.flexGrow = 1;

        rootVisualElement.Add(title);
        rootVisualElement.Add(filterField);
        rootVisualElement.Add(scrollView);

        EditorApplication.update += RefreshLoop;
    }

    private void OnDisable()
    {
        EditorApplication.update -= RefreshLoop;
    }

    private void RefreshLoop()
    {
        if (!EditorApplication.isPlaying) return;
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        scrollView.Clear();

        var allPlayers = GameObject.FindObjectsOfType<PlayerCustomizationData>();

        if (allPlayers.Length == 0)
        {
            scrollView.Add(new Label("❌ Aucun joueur trouvé."));
            return;
        }

        foreach (var player in allPlayers.OrderBy(p => p.OwnerClientId))
        {
            bool isLocal = player.IsOwner;
            if ((currentFilter == PlayerFilter.LocalOnly && !isLocal) ||
                (currentFilter == PlayerFilter.RemoteOnly && isLocal))
            {
                continue;
            }

            var box = new Box();
            box.style.marginBottom = 6;
            box.style.paddingBottom = 4;
            box.style.borderBottomWidth = 1;
            box.style.borderBottomColor = new Color(0, 0, 0, 0.2f);

            var header = new Label($"👤 Joueur {player.OwnerClientId} {(isLocal ? "— LOCAL" : "")}");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            box.Add(header);

            var data = player.Data.Value;

            if (data.equippedItemIds == null || data.equippedItemIds.Count == 0)
            {
                box.Add(new Label("Aucune donnée de tenue."));
            }
            else
            {
                foreach (var kvp in data.equippedItemIds)
                {
                    var slot = kvp.Key;
                    var itemId = kvp.Value;

                    data.TryGetColor(slot, out var color);
                    data.TryGetTexture(slot, out var textureName);
                    string colorHex = ColorUtility.ToHtmlStringRGBA(color);

                    var row = new Label($"→ {slot} : {itemId} | 🎨 #{colorHex} | 🧵 {textureName}");
                    box.Add(row);
                }
            }

            // 🔁 Réappliquer
            var applyBtn = new Button(() =>
            {
                var visuals = player.GetComponentInChildren<EquippedVisualsHandler>(true);
                if (visuals == null)
                {
                    Debug.LogWarning($"[Debugger] Aucun visuals pour {player.OwnerClientId}");
                    return;
                }

                var allItems = Resources.LoadAll<Item>("Items").ToList();
                player.ApplyToVisuals(visuals, allItems);
                Debug.Log($"[Debugger] ✅ Tenue réappliquée pour {player.OwnerClientId}");
            })
            { text = "🔁 Réappliquer la tenue" };
            box.Add(applyBtn);

            // 📋 Log console
            var logBtn = new Button(player.LogTenue)
            {
                text = "📋 Log Console"
            };
            box.Add(logBtn);

            // 💾 Export JSON
            var exportBtn = new Button(() => ExportTenueAsJson(player))
            {
                text = "💾 Export JSON"
            };
            box.Add(exportBtn);

            scrollView.Add(box);
        }
    }

    private void ExportTenueAsJson(PlayerCustomizationData player)
    {
        string json = JsonUtility.ToJson(player.Data.Value, true);
        string folderPath = Application.dataPath + "Resources/TenueExports";
        Directory.CreateDirectory(folderPath);

        string filePath = $"{folderPath}/Player_{player.OwnerClientId}_Tenue.json";
        File.WriteAllText(filePath, json);

        Debug.Log($"[Debugger] 💾 Tenue exportée : {filePath}");
        EditorUtility.RevealInFinder(filePath);
    }
}
