using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BannedWordsData))]
public class BannedWordsDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        if (GUILayout.Button("🛠 Ouvrir le gestionnaire de mots bannis"))
        {
            BannedWordsEditorWindow.ShowWindow((BannedWordsData)target);
        }

        GUILayout.Space(15);

        if (GUILayout.Button("📥 Importer depuis un fichier .txt", GUILayout.Height(35)))
        {
            BannedWordsData data = (BannedWordsData)target;

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
                    }
                }
                data.bannedWords.Sort(); // 📑 Trie la liste après import


                EditorUtility.SetDirty(data);
                Debug.Log($"[BannedWords] ✅ {added} mot(s) importé(s) depuis : {path}");
            }
            else
            {
                Debug.LogWarning("[BannedWords] ❌ Aucune donnée importée.");
            }
        }
    }
}
