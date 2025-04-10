using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Type = NotificationData.NotificationType;

public class BannedWordsEditorWindow : EditorWindow
{
    private Vector2 scrollPos;
    private BannedWordsData data;
    private string newWord = "";
    private string searchTerm = "";

    public static void ShowWindow(BannedWordsData target)
    {
        var window = GetWindow<BannedWordsEditorWindow>("Mots bannis");
        window.data = target;
        window.minSize = new Vector2(300, 300);
    }

    private void OnGUI()
    {
        if (data == null)
        {
            EditorGUILayout.HelpBox("Aucune donnée chargée.", MessageType.Warning);
            return;
        }

        GUILayout.Label("Liste des mots bannis", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        searchTerm = EditorGUILayout.TextField("🔍 Rechercher", searchTerm);

        GUILayout.Space(5);
        if (GUILayout.Button("🔤 Trier alphabétiquement", GUILayout.Height(25)))
        {
            data.bannedWords.Sort();
            NotificationManager.Instance?.ShowNotification("📑 Liste triée", Type.Normal);
        }
        if (GUILayout.Button("📥 Importer depuis un fichier .txt", GUILayout.Height(25)))
        {
            string path = EditorUtility.OpenFilePanel("Importer mots bannis", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(path))
            {
                int added = 0;
                string[] lines = System.IO.File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string clean = line.Trim().ToLower();
                    if (!string.IsNullOrWhiteSpace(clean) && !data.bannedWords.Contains(clean))
                    {
                        data.bannedWords.Add(clean);
                        added++;
                        data.bannedWords = data.bannedWords.Distinct().ToList();
                    }
                }
                data.bannedWords.Sort(); // 📑 Trie la liste après import
                EditorUtility.SetDirty(data);
                Debug.Log($"[BannedWords] ✅ {added} mot(s) importé(s) depuis : {path}");
                NotificationManager.Instance?.ShowNotification($"✅ {added} mot(s) importé(s)", Type.Info);
            }
            else
            {
                Debug.LogWarning("[BannedWords] ❌ Aucune donnée importée.");
            }
        }
        if (GUILayout.Button("📤 Exporter vers un fichier .txt", GUILayout.Height(25)))
        {
            string path = EditorUtility.SaveFilePanel("Exporter mots bannis", Application.dataPath, "MotsBannis.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllLines(path, data.bannedWords);
                Debug.Log($"[BannedWords] ✅ Liste exportée vers : {path}");
                NotificationManager.Instance?.ShowNotification($"✅ Liste exportée vers : {path}", Type.Info);
            }
            else
            {
                Debug.LogWarning("[BannedWords] ❌ Aucune donnée exportée.");
            }
        }
        if (GUILayout.Button("🧹 Supprimer les doublons", GUILayout.Height(25)))
        {
            int before = data.bannedWords.Count;
            data.bannedWords = new List<string>(new HashSet<string>(data.bannedWords));
            int removed = before - data.bannedWords.Count;

            NotificationManager.Instance?.ShowNotification($"🧼 {removed} doublon(s) supprimé(s)", Type.Info);
            Debug.Log($"[BannedWords] 🔄 {removed} doublon(s) retiré(s).");

            GUI.FocusControl(null); // désélectionne le champ actif
            EditorUtility.SetDirty(data);
        }

        GUILayout.Space(5);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < data.bannedWords.Count; i++)
        {
            string word = data.bannedWords[i];

            if (!string.IsNullOrWhiteSpace(searchTerm) && !word.ToLower().Contains(searchTerm.ToLower()))
                continue;

            EditorGUILayout.BeginHorizontal();

            // 🌟 surbrillance du mot s’il match la recherche
            if (!string.IsNullOrWhiteSpace(searchTerm) && word.ToLower().Contains(searchTerm.ToLower()))
            {
                GUIStyle highlightStyle = new(GUI.skin.textField)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.yellow }
                };
                data.bannedWords[i] = EditorGUILayout.TextField(word, highlightStyle);
            }
            else
            {
                data.bannedWords[i] = EditorGUILayout.TextField(word);
            }

            if (GUILayout.Button("❌", GUILayout.Width(30)))
            {
                data.bannedWords.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);
        GUILayout.Label("➕ Ajouter un mot");
        newWord = EditorGUILayout.TextField(newWord);

        if (GUILayout.Button("Ajouter"))
        {
            if (!string.IsNullOrWhiteSpace(newWord) && !data.bannedWords.Contains(newWord.ToLower()))
            {
                data.bannedWords.Add(newWord.ToLower());
                data.bannedWords.Sort(); // trie à l'ajout
                data.bannedWords = data.bannedWords.Distinct().ToList();
                NotificationManager.Instance?.ShowNotification($"🔒 '{newWord}' ajouté", Type.Info);
                newWord = "";
            }
        }

        if (GUI.changed)
            EditorUtility.SetDirty(data);
    }
}
