using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class ChooseCamPoint : MonoBehaviour
{
    [SerializeField] private Camera characterCam;
    [SerializeField] private float lerpSpeed = 5f;

    private List<Transform> camPoints = new List<Transform>();
    private Transform targetCamPoint;

    private void Start()
    {
        GameObject[] camPointObjects = GameObject.FindGameObjectsWithTag("CamPoint");
        foreach (GameObject camPointObject in camPointObjects)
        {
            camPoints.Add(camPointObject.transform);
        }
        if (camPoints.Count == 0)
        {
            Debug.LogWarning("Aucun GameObject avec le tag 'CamPoint' n'a �t� trouv� dans la sc�ne.");
        }
        else
        {
            Debug.Log($"{camPoints.Count} points de cam�ra trouv�s et ajout�s � la liste.");
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

    public void SwitchToCamPoint(string camPoint)
    {
        GameObject targetObject = GameObject.Find(camPoint);

        if (targetObject != null)
        {
            targetCamPoint = targetObject.transform;
            Debug.Log($"Changement de la cam�ra vers le point : {camPoint}");
        }
        else
        {
            Debug.LogWarning($"Le GameObject avec le nom '{camPoint}' n'a pas �t� trouv� dans la sc�ne.");
        }
    }


}
