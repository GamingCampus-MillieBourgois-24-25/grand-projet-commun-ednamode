using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class RunwayCameraController : MonoBehaviour
{
    [Header("📸 Points de caméra")]
    [Tooltip("Liste des positions intermédiaires de la caméra avant d'arriver à la position finale.")]
    [SerializeField] private List<Transform> cameraSpots;

    [Tooltip("Position finale de la caméra pour le défilé.")]
    [SerializeField] private Transform finalSpot;

    [Header("⏱️ Timing")]
    [Tooltip("Temps entre chaque changement de caméra.")]
    [SerializeField] private float timeBetweenShots = 0.5f;

    [Tooltip("Durée du mouvement vers chaque spot.")]
    [SerializeField] private float moveDuration = 0.3f;

    [Tooltip("Effet de flash à chaque position (optionnel).")]
    [SerializeField] private GameObject flashEffectPrefab;

    private Camera localCam;

    public void StartPhotoSequence(Transform targetToLookAt)
    {
        localCam = NetworkPlayerManager.Instance.GetLocalPlayer()?.GetLocalCamera();
        if (localCam == null)
        {
            Debug.LogWarning("[RunwayCam] 🚫 Caméra locale introuvable !");
            return;
        }

        StartCoroutine(PhotoSequenceCoroutine(targetToLookAt));
    }

    private IEnumerator PhotoSequenceCoroutine(Transform target)
    {
        foreach (var spot in cameraSpots)
        {
            MoveCameraToSpot(spot, target);
            TriggerFlash();
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // Dernière position
        MoveCameraToSpot(finalSpot, target);
    }

    private void MoveCameraToSpot(Transform spot, Transform target)
    {
        // Transition/Animation vers la position le spot
        //localCam.transform.DOMove(spot.position, moveDuration).SetEase(Ease.InOutSine);
        //localCam.transform.DOLookAt(target.position + Vector3.up * 1.5f, moveDuration).SetEase(Ease.InOutSine);

        // Snap a la position du spot
        localCam.transform.position = spot.position;
        localCam.transform.LookAt(target.position + Vector3.up * 1.5f);

        // Petit effet de zoom rapide
        localCam.fieldOfView = 50f;
        DOTween.To(() => localCam.fieldOfView, x => localCam.fieldOfView = x, 60f, 0.2f);
    }

    private void TriggerFlash()
    {
        if (flashEffectPrefab != null)
        {
            var flash = Instantiate(flashEffectPrefab, localCam.transform);
            Destroy(flash, 0.5f);  // Auto-destruction après effet
        }
    }
}
