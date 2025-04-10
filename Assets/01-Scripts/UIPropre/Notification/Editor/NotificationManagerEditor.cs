#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NotificationManager))]
public class NotificationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NotificationManager manager = (NotificationManager)target;

        if (Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Test Notification", EditorStyles.boldLabel);

            if (GUILayout.Button("Afficher notification de test"))
            {
                manager.ShowNotification(manager.DebugMessage, manager.DebugType, manager.DebugIcon);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Le test est uniquement disponible en Play Mode.", MessageType.Info);
        }
    }
}
#endif
