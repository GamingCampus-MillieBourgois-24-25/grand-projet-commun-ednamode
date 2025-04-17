using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Netcode;
using System.Collections.Generic;

using Type = NotificationData.NotificationType;

public class ChatUI : MonoBehaviour
{
    [Header("Références UI")]
    [Tooltip("Panneau de chat.")]
    [SerializeField] private GameObject chatPanel;

    [Tooltip("Champ de texte pour l'entrée du message.")]
    [SerializeField] private TMP_InputField inputField;

    [Tooltip("Conteneur pour les messages.")]
    [SerializeField] private Transform messageContainer;

    [Tooltip("Préfab de message à instancier.")]
    [SerializeField] private GameObject messagePrefab;

    [Tooltip("Pillule de notification pour les nouveaux messages.")]
    [SerializeField] private Image newMessagePill;

    [Tooltip("ScrollRect pour le défilement automatique.")]
    [SerializeField] private ScrollRect scrollRect;

    [Tooltip("Bouton d'envoi du message.")]
    [SerializeField] private Button sendButton;

    [Header("Filtres")]
    [Tooltip("Liste des mots interdits dans le chat.")]
    [SerializeField] private BannedWordsData bannedWordsData;

    [Header("Paramètres")]
    [Tooltip("Nombre maximum de messages affichés dans le chat.")]
    [SerializeField] private int maxMessages = 50;

    private bool isConnected => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    private readonly Queue<GameObject> activeMessages = new();

    private const string CHAT_LOG_KEY = "Chat_Log";
    private const int MAX_LOG_SIZE = 50;

    private readonly List<string> localLog = new(); // pour le mode offline

    private void Start()
    {
        ChatManager.OnMessageReceived += OnMessageReceived;

        inputField.onSubmit.AddListener(HandleInputSubmit);
        sendButton.onClick.AddListener(() => HandleInputSubmit(inputField.text));

        HideNotification();

        if (!isConnected)
        {
            LogLocal("📢 Local Mode activated");
        }
        LoadChatLog();
    }

    private void OnDestroy()
    {
        ChatManager.OnMessageReceived -= OnMessageReceived;
    }

    private void HandleInputSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        inputField.text = "";

        if (text.StartsWith("/"))
        {
            ExecuteCommand(text);
        }
        else
        {
            if (isConnected)
                ChatManager.Instance.SendChat(text);
            else
                LogLocal($"Moi : {text}");
        }
    }

    private void ExecuteCommand(string input)
    {
        string[] parts = input.Split(' ', 2);
        string cmd = parts[0].ToLower();
        string arg = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
            case "/ping":
                LogLocal("🏓 Pong!");
                break;
            case "/me":
                LogLocal($"🤖 {arg}");
                break;
            case "/ready":
                MultiplayerManager.Instance?.SetReady(true);
                LogLocal("✅ You're ready");
                break;
            case "/unready":
                MultiplayerManager.Instance?.SetReady(false);
                LogLocal("❌ You're not ready");
                break;
            case "/history":
                DisplayRestoredMessagesOnly();
                break;
            case "/clear":
                foreach (Transform child in messageContainer)
                    Destroy(child.gameObject);
                LogLocal("🧹 Chat cleaned");
                break;
           /* case "/vibe":
                VibrationManager.Vibrate();
                break;*/
            case "/clearlog":
                localLog.Clear();
                PlayerPrefs.DeleteKey(CHAT_LOG_KEY);

                foreach (Transform child in messageContainer)
                    Destroy(child.gameObject);

                LogLocal("🧹 History cleared");
                break;
            case "/export":
                ExportChatLog();
                LogLocal("💾 Chat exported to " + Application.persistentDataPath);
                NotificationManager.Instance?.ShowNotification($"Chat exported to {Application.persistentDataPath}", Type.Info);

                break;
            case "/banword":
                if (string.IsNullOrWhiteSpace(arg))
                {
                    LogLocal("❌ Use : /banword word");
                }
                else
                {
                    if (bannedWordsData != null && !bannedWordsData.Contains(arg))
                    {
                        bannedWordsData.bannedWords.Add(arg.ToLower());
                        LogLocal($"🔒 Banned word added : {arg}");
                        NotificationManager.Instance?.ShowNotification($"Banned word added : {arg}", Type.Info);
                    }
                    else
                    {
                        LogLocal($"ℹ️ Word already set or data null.");
                    }
                }
                break;
            case "/unbanword":
                if (string.IsNullOrWhiteSpace(arg))
                {
                    LogLocal("❌ Use : /unbanword word");
                }
                else
                {
                    if (bannedWordsData != null && bannedWordsData.Contains(arg))
                    {
                        bannedWordsData.bannedWords.Remove(arg.ToLower());
                        LogLocal($"🔓Banned word removed : {arg}");
                        NotificationManager.Instance?.ShowNotification($"Banned word removed : {arg}", Type.Info);
                    }
                    else
                    {
                        LogLocal($"ℹ️ Word not found or data null.");
                    }
                }
                break;
            case "/secret":
                LogLocal("🕵️ Commande secrète activée !");
                break;
            default:
                LogLocal($"❓ Unknown command : {cmd}");
                break;
        }
    }

    private string FilterBanWords(string input)
    {
        if (bannedWordsData == null || bannedWordsData.bannedWords.Count == 0)
            return input;

        foreach (string bad in bannedWordsData.bannedWords)
        {
            string stars = new string('*', bad.Length);

            if (System.Text.RegularExpressions.Regex.IsMatch(input, $"\\b{bad}\\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                Debug.LogWarning($"[Chat] ⚠️ Mot banni détecté : '{bad}' dans : \"{input}\"");

                NotificationManager.Instance?.ShowNotification($"Banned word detected : {bad}", Type.Important);
            }

            input = System.Text.RegularExpressions.Regex.Replace(
                input,
                $"\\b{bad}\\b",
                stars,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        return input;
    }

    private void LogLocal(string message)
    {
        localLog.Add(message);

        if (localLog.Count > MAX_LOG_SIZE)
            localLog.RemoveAt(0);

        SaveChatLog();
        DisplayMessage("[Sys]", message);
    }

    private void SaveChatLog()
    {
        string combined = string.Join("||", localLog);
        string encrypted = Encrypt(combined);
        PlayerPrefs.SetString(CHAT_LOG_KEY, encrypted);
        PlayerPrefs.Save();
    }

    private void LoadChatLog()
    {
        localLog.Clear();

        if (PlayerPrefs.HasKey(CHAT_LOG_KEY))
        {
            string rawEncrypted = PlayerPrefs.GetString(CHAT_LOG_KEY);
            string decrypted = Decrypt(rawEncrypted);

            var lines = decrypted.Split(new[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                localLog.Add(line);
                DisplayMessage("[Sys]", line, isRestored: true);
            }
        }
    }

    private void OnMessageReceived(string sender, string message)
    {
        DisplayMessage(sender, message);

        if (!chatPanel.activeInHierarchy)
        {
            ShowNotification();
        }
    }

    private void DisplayMessage(string sender, string message, bool isRestored = false)
    {
        var go = Instantiate(messagePrefab, messageContainer);
        var tmp = go.GetComponentInChildren<TMP_Text>();

        // ✳️ Applique le filtre ici
        message = FilterBanWords(message);

        tmp.text = $"<b>{sender}</b>: {message}";

        if (isRestored)
        {
            tmp.color = new Color(0.75f, 0.75f, 0.75f); // gris clair
            tmp.fontStyle = FontStyles.Italic;
        }
        else if (sender == "[Sys]")
        {
            tmp.color = new Color(1f, 0.84f, 0.4f); // orange doux
            tmp.fontStyle = FontStyles.Bold;
        }

        activeMessages.Enqueue(go);

        // Supprime les plus anciens
        if (activeMessages.Count > maxMessages)
        {
            var toRemove = activeMessages.Dequeue();
            Destroy(toRemove);
        }

        AutoScrollToBottom();


        AutoScrollToBottom();
    }

    private void DisplayRestoredMessagesOnly()
    {
        foreach (Transform child in messageContainer)
            Destroy(child.gameObject);

        foreach (string line in localLog)
            DisplayMessage("[Sys]", line, isRestored: true);
    }

    private void AutoScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // évite le délai d'update
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ShowNotification()
    {
        newMessagePill.gameObject.SetActive(true);
        newMessagePill.DOKill();
        newMessagePill.DOFade(0.3f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void HideNotification()
    {
        newMessagePill.DOKill();
        newMessagePill.gameObject.SetActive(false);
    }

    public void OnChatOpened()
    {
        HideNotification();
    }

    private void ExportChatLog()
    {
        string path = Application.persistentDataPath + "/chat_history.txt";
        System.IO.File.WriteAllLines(path, localLog);
        Debug.Log($"[Chat] Exported to : {path}");
    }

    private string Encrypt(string raw)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(raw);
        return System.Convert.ToBase64String(bytes);
    }

    private string Decrypt(string encoded)
    {
        try
        {
            byte[] bytes = System.Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return ""; // en cas de lecture corrompue
        }
    }

}
