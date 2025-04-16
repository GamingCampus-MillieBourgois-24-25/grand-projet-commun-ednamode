using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class ChooseCamPoint : MonoBehaviour
{
    [SerializeField] private Camera characterCam;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private RawImage characterRawImage;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private float padding = 1.2f; // Facteur pour ajouter un peu d'espace autour du personnage

    private List<Transform> camPoints = new List<Transform>();
    private Transform targetCamPoint;

    public enum CamPointType
    {
        Face,
        Torso,
        Legs,
        Shoe,
        FullBody
    }
    private void Start()
    {
        GameObject[] camPointObjects = GameObject.FindGameObjectsWithTag("CamPoint");
        foreach (GameObject camPointObject in camPointObjects)
        {
            camPoints.Add(camPointObject.transform);
        }
        if (camPoints.Count == 0)
        {
            Debug.LogWarning("Aucun GameObject avec le tag 'CamPoint' n'a été trouvé dans la scène.");
        }
        else
        {
            Debug.Log($"{camPoints.Count} points de caméra trouvés et ajoutés à la liste.");
        }
    }


    void Update()
    {
        if (targetCamPoint != null)
        {
            characterCam.transform.position = Vector3.Lerp(
                characterCam.transform.position,
                targetCamPoint.position,
                Time.deltaTime * lerpSpeed
                
            );

            characterCam.transform.rotation = Quaternion.Lerp(
                characterCam.transform.rotation,
                targetCamPoint.rotation,
                Time.deltaTime * lerpSpeed
            );
        }
    }

    public void SwitchToCamPoint(CamPointType camPoint)
    {
        GameObject targetObject = GameObject.Find(camPoint.ToString()+"CamPoint");

        if (targetObject != null)
        {
            targetCamPoint = targetObject.transform;
            Debug.Log($"Changement de la caméra vers le point : {camPoint}");
        }
        else
        {
            Debug.LogWarning($"Le GameObject avec le nom '{camPoint}' n'a pas été trouvé dans la scène.");
        }
    }
}
