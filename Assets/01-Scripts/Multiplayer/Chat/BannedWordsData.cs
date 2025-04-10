using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(menuName = "Chat/Banned Words Data")]
public class BannedWordsData : ScriptableObject
{
    [Tooltip("Mots bannis à filtrer")]
    public List<string> bannedWords = new();

    public bool Contains(string word)
    {
        foreach (string b in bannedWords)
        {
            if (string.Equals(b, word, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("📥 Importer depuis un fichier TXT")]
    public void ImportFromTextFile()
    {
        string path = EditorUtility.OpenFilePanel("Importer mots bannis", Application.dataPath, "txt");

        if (string.IsNullOrEmpty(path)) return;

        string[] lines = System.IO.File.ReadAllLines(path);

        int added = 0;
        foreach (string line in lines)
        {
            string clean = line.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(clean) && !bannedWords.Contains(clean))
            {
                bannedWords.Add(clean);
                added++;
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[BannedWords] ✅ {added} mot(s) importé(s) depuis {path}");
    }

    [ContextMenu("📤 Exporter vers un fichier TXT")]
    public void ExportToTextFile()
    {
        string path = EditorUtility.SaveFilePanel("Exporter mots bannis", Application.dataPath, "banned_words", "txt");

        if (string.IsNullOrEmpty(path)) return;

        bannedWords.Sort();
        System.IO.File.WriteAllLines(path, bannedWords);
        Debug.Log($"[BannedWords] 💾 Exporté vers : {path}");
    }
#endif

}
