using UnityEngine;

public class Rotatable : MonoBehaviour
{
    [SerializeField] private float speed = 300f; 
    [SerializeField] private bool inverted = false; 

    private bool isRotating = false;
    private Vector2 lastPosition;

    void Update()
    {
        // Gestion de la souris 
        if (Input.GetMouseButtonDown(0)) 
        {
            isRotating = true;
            lastPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) 
        {
            isRotating = false;
        }

        // Gestion du toucher 
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

            float deltaX = currentPosition.x - lastPosition.x;
            float rotationAmount = deltaX * speed * Time.deltaTime;

            transform.Rotate(Vector3.up, inverted ? rotationAmount : -rotationAmount, Space.World);

            lastPosition = currentPosition;
        }
    }
}