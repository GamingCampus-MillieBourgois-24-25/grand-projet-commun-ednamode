using UnityEngine;

public class Rotatable : MonoBehaviour
{
    [SerializeField] private float speed = 300f; // Vitesse de rotation (ajustable dans l’inspecteur)
    [SerializeField] private bool inverted = false; // Inversion de la direction

    private bool isRotating = false;
    private Vector2 lastPosition;

    void Update()
    {
        // Gestion de la souris (PC)
        if (Input.GetMouseButtonDown(0)) // Clic gauche pressé
        {
            isRotating = true;
            lastPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) // Clic gauche relâché
        {
            isRotating = false;
        }

        // Gestion du toucher (mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isRotating = true;
                lastPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isRotating = false;
            }
        }

        // Rotation si en cours
        if (isRotating)
        {
            Vector2 currentPosition = Vector2.zero;

            // Souris
            if (Input.GetMouseButton(0))
            {
                currentPosition = Input.mousePosition;
            }
            // Toucher
            else if (Input.touchCount > 0)
            {
                currentPosition = Input.GetTouch(0).position;
            }

            // Calculer le déplacement horizontal
            float deltaX = currentPosition.x - lastPosition.x;
            float rotationAmount = deltaX * speed * Time.deltaTime;

            // Appliquer la rotation autour de l’axe Y (Vector3.up)
            transform.Rotate(Vector3.up, inverted ? rotationAmount : -rotationAmount, Space.World);

            // Mettre à jour la dernière position
            lastPosition = currentPosition;
        }
    }
}