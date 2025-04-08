using UnityEngine;

public struct NotificationData
{
    public enum NotificationType { Info, Normal, Important }

    public string message;
    public NotificationType type;
    public Color color;
    public Sprite icon;

    public NotificationData(string message, NotificationType type, Color color, Sprite icon = null)
    {
        this.message = message;
        this.type = type;
        this.color = color;
        this.icon = icon;
    }
}
