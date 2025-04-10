using System.Collections;
using UnityEngine;

/// <summary>
/// Service statique de vibration mobile.
/// Ne dépend d'aucun GameObject dans la scène.
/// </summary>
public static class VibrationManager
{
    private static bool vibrationEnabled = true;
    private static float vibrationIntensity = 1f;

    /// <summary>
    /// Déclenche une vibration si activée
    /// </summary>
    public static void Vibrate()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (!vibrationEnabled) return;

        Handheld.Vibrate();
        Debug.Log("[Vibration] Déclenchée (intensité simulée = " + vibrationIntensity + ")");
#endif
    }

    public static void VibratePattern(params float[] delays)
    {
        if (!vibrationEnabled) return;
        InstanceBehaviour.Instance.StartCoroutine(VibrateRoutine(delays));
    }

    private static IEnumerator VibrateRoutine(float[] delays)
    {
        foreach (var d in delays)
        {
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
            yield return new WaitForSeconds(d * vibrationIntensity); // simule intensité par délai
        }
    }



    public static void SetVibrationEnabled(bool enabled)
    {
        vibrationEnabled = enabled;
    }

    public static void SetVibrationIntensity(float intensity)
    {
        vibrationIntensity = Mathf.Clamp01(intensity);
    }

    public static bool IsVibrationEnabled() => vibrationEnabled;
    public static float GetVibrationIntensity() => vibrationIntensity;
}
