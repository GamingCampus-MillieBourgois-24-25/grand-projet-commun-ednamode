using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    private SerializedProperty panels;
    private ReorderableList panelList;

    private readonly Dictionary<string, string[]> groupedProperties = new()
    {
        {
            "🎬 Transition d’Écran", new[]
            {
                "screenTransitionPanel",
                "centerPosition",
                "openExitPosition",
                "closeStartPosition",
                "screenMoveDuration",
                "autoPlayIntroOnStart",
                "transitionIntroSound",
                "transitionOutroSound"
            }
        },
        {
            "🔊 Sons UI", new[]
            {
                "uiAudioSource",
                "clickSound",
                "panelOpenSound",
                "panelCloseSound"
            }
        },
        {
            "📱 Gestes Tactiles", new[]
            {
                "OnSwipeLeft",
                "OnSwipeRight",
                "OnTap",
                "OnDoubleTap"
            }
        },
        {
            "⚙️ Animation Panels", new[]
            {
                "animationDuration",
                "animationEase",
                "hiddenScale"
            }
        },
        {
            "⏳ Countdown", new[]
            {
                "countdownPanel",
                "countdownText",
                "normalColor",
                "alertColor"
            }
        }
    };

    private void OnEnable()
    {
        panels = serializedObject.FindProperty("panels");

        panelList = new ReorderableList(serializedObject, panels, true, true, true, true);
        panelList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "🧩 Liste des Panels (modulables)");
        };

        panelList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = panels.GetArrayElementAtIndex(index);
            rect.y += 2;

            var panelNameProp = element.FindPropertyRelative("panelName");
            string label = string.IsNullOrEmpty(panelNameProp.stringValue) ? $"Panel {index + 1}" : panelNameProp.stringValue;

            EditorGUI.PropertyField(
                new Rect(rect.x + 8, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element, new GUIContent(label), true);
        };

        panelList.elementHeightCallback = (index) =>
        {
            return EditorGUI.GetPropertyHeight(panels.GetArrayElementAtIndex(index)) + 4;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        foreach (var group in groupedProperties)
        {
            DrawSection(group.Key, group.Value);
        }

        EditorGUILayout.Space(10);
        panelList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string label, string[] properties)
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        foreach (var prop in properties)
        {
            var property = serializedObject.FindProperty(prop);
            if (property != null)
                EditorGUILayout.PropertyField(property, true);
            else
                EditorGUILayout.HelpBox($"Champ introuvable : {prop}", MessageType.Warning);
        }
    }
}
