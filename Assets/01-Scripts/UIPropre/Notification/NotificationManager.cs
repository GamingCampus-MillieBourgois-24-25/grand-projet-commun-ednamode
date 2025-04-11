using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Paramètres généraux")]
    [Tooltip("Parent où seront affichées les notifications.")]
    [SerializeField] private Transform notificationParent;

    [Tooltip("Prefab de notification (doit contenir NotificationUI).")]
    [SerializeField] private GameObject notificationPrefab;

    [Tooltip("Nombre max de notifications affichées en même temps.")]
    [SerializeField] private int maxVisible = 3;

    [Header("Couleurs par type")]
    [Tooltip("Couleur pour les notifications normales.")]
    public Color colorNormal = Color.white;

    [Tooltip("Couleur pour les notifications info.")]
    public Color colorInfo = Color.yellow;

    [Tooltip("Couleur pour les notifications importantes.")]
    public Color colorImportant = Color.red;

    private readonly Queue<NotificationData> pendingQueue = new();
    private readonly List<NotificationUI> activeNotifications = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Appel public pour afficher une notification.
    /// </summary>
    public void ShowNotification(string message, NotificationData.NotificationType type = NotificationData.NotificationType.Normal, Sprite icon = null)
    {
        Color color = GetColorByType(type);
        var data = new NotificationData(message, type, color, icon);
        Enqueue(data);
    }

    private Color GetColorByType(NotificationData.NotificationType type)
    {
        return type switch
        {
            NotificationData.NotificationType.Info => colorInfo,
            NotificationData.NotificationType.Important => colorImportant,
            _ => colorNormal,
        };
    }

    private void Enqueue(NotificationData data)
    {
        pendingQueue.Enqueue(data);
        TryDisplayNext();
    }

    private void TryDisplayNext()
    {
        if (activeNotifications.Count >= maxVisible || pendingQueue.Count == 0)
            return;

        var data = pendingQueue.Dequeue();

        GameObject notifGO = ObjectPool.Instance.Spawn(notificationPrefab, notificationParent.position, Quaternion.identity, notificationParent);
        NotificationUI ui = notifGO.GetComponent<NotificationUI>();
        if (ui == null)
        {
            Debug.LogError("[NotificationManager] Le prefab ne contient pas de NotificationUI.");
            return;
        }

        int index = activeNotifications.Count;
        activeNotifications.Add(ui);
        notifGO.transform.SetSiblingIndex(0); // Pour que la dernière notif passe au-dessus

        ui.Initialize(data, notificationPrefab, index);
    }

    public void NotifyDespawn(NotificationUI notification)
    {
        if (activeNotifications.Contains(notification))
        {
            int oldIndex = activeNotifications.IndexOf(notification);
            activeNotifications.Remove(notification);

            for (int i = oldIndex; i < activeNotifications.Count; i++)
            {
                activeNotifications[i].ShiftToIndex(i);
                activeNotifications[i].transform.SetSiblingIndex(activeNotifications.Count - i); // Pousse vers le bas
            }

            TryDisplayNext();
        }
    }

#if UNITY_EDITOR
    // === Test Bouton dans l'inspecteur ===
    [Header("Debug depuis l’éditeur")]
    [Tooltip("Message de test")]
    [SerializeField] private string debugMessage = "Test message";

    [Tooltip("Type de test")]
    [SerializeField] private NotificationData.NotificationType debugType = NotificationData.NotificationType.Normal;

    [Tooltip("Icône de test")]
    [SerializeField] private Sprite debugIcon;

    // Accès utilisé par l’éditeur personnalisé
    public string DebugMessage => debugMessage;
    public NotificationData.NotificationType DebugType => debugType;
    public Sprite DebugIcon => debugIcon;


    [ContextMenu("Tester Notification Normale")]
    public void TestNotification_Normal() => ShowNotification("Notification normale", NotificationData.NotificationType.Normal);

    [ContextMenu("Tester Notification Info")]
    public void TestNotification_Info() => ShowNotification("Notification info", NotificationData.NotificationType.Info);

    [ContextMenu("Tester Notification Importante")]
    public void TestNotification_Important() => ShowNotification("Notification importante", NotificationData.NotificationType.Important);
#endif
}
