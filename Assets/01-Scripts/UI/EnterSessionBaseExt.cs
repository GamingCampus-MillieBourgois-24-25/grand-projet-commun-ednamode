using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class EnterSessionFix : MonoBehaviour
{
    private object sessionBaseInstance;
    private Type sessionBaseType;
    private Button enterSessionButton;

    private void Awake()
    {
        sessionBaseType = Type.GetType("Unity.Multiplayer.Widgets.EnterSessionBase, Assembly-CSharp");

        if (sessionBaseType == null)
        {
            Debug.LogError("[EnterSessionFix] Impossible de trouver `EnterSessionBase`. V�rifie que le package est bien inclus.");
            return;
        }

        // R�cup�rer l'instance de `EnterSessionBase`
        sessionBaseInstance = GetComponent(sessionBaseType);

        if (sessionBaseInstance == null)
        {
            Debug.LogError("[EnterSessionFix] Aucun `EnterSessionBase` trouv� sur cet objet !");
            return;
        }

        // Acc�der � `m_EnterSessionButton` via Reflection
        FieldInfo buttonField = sessionBaseType.GetField("m_EnterSessionButton",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (buttonField != null)
        {
            enterSessionButton = buttonField.GetValue(sessionBaseInstance) as Button;

            if (enterSessionButton == null)
            {
                Debug.LogError("[EnterSessionFix] ? Le bouton `m_EnterSessionButton` est NULL !");
            }
            else
            {
                Debug.Log("[EnterSessionFix] ? Le bouton `m_EnterSessionButton` est bien trouv�.");
            }
        }
        else
        {
            Debug.LogError("[EnterSessionFix] Impossible de trouver `m_EnterSessionButton` via Reflection.");
        }
    }
}
