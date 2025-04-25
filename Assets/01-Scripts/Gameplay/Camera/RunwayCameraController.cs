using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    [Header("🎨 Effets")]
    [Tooltip("Volume de post-traitement pour les effets visuels.")]
    [SerializeField] private Volume postProcessVolume;

    [Tooltip("Intensité de l'effet vignette.")]
    [Range(0f, 1f)]
    [SerializeField] private float vignetteIntensity = 0.5f;

    [Tooltip("Durée de l'effet vignette.")]
    [Range(0f, 1f)]
    [SerializeField] private float vignetteDuration = 0.2f;

    [SerializeField] private AudioClip shutterSound;

    private AudioSource audioSource;
    private Camera localCam;
    private Vignette vignette;

    private void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }
        else
        {
            Debug.LogWarning("[RunwayCam] ⚠️ Vignette non trouvée dans le PostProcessVolume.");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
    }

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
            TriggerPhotoEffect();
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
        
        // Optionnel : léger shake pour effet "impact photo"
        localCam.transform.DOShakePosition(0.2f, 0.2f);
    }

    private void TriggerPhotoEffect()
    {
        if (vignette != null)
        {
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, vignetteIntensity, vignetteDuration / 2)
                   .OnComplete(() => DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, vignetteDuration / 2));
        }

        if (shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }
    }
}
