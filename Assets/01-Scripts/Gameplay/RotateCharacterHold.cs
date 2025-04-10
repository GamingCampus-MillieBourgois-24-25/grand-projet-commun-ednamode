using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCharacterHold : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private bool isHolding = false;
    private float currentRotationSpeed = 0f;

    [SerializeField]
    private float decelerationRate = 5f;
    [SerializeField]
    private float maxRotationSpeed = 3000f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.UI.Hold.started += OnHoldStarted;
        inputActions.UI.Hold.canceled += OnHoldCanceled;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.UI.Hold.started -= OnHoldStarted;
        inputActions.UI.Hold.canceled -= OnHoldCanceled;
        inputActions.Disable();
    }

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        // Vérifie si le pointeur est dans le premier tiers gauche de l'écran
        if (IsPointerInLeftThird())
        {
            isHolding = true;
            currentRotationSpeed = 0f;
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        isHolding = false;
    }

    private void Update()
    {
        if (isHolding)
        {
            Vector2 pointerDelta = GetPointerDelta();
            currentRotationSpeed = pointerDelta.x * 100;
            currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed, maxRotationSpeed);

            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);
        }
        else if (currentRotationSpeed != 0f)
        {
            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);
            currentRotationSpeed = Mathf.MoveTowards(currentRotationSpeed, 0f, decelerationRate * Time.deltaTime);
        }
    }

    private bool IsPointerInLeftThird()
    {
        Vector2 pointerPosition = GetPointerPosition();

        // Calculer la largeur du premier tiers gauche de l'écran
        float screenWidth = Screen.width;
        float leftThirdWidth = screenWidth / 3f;

        // Vérifier si la position du pointeur est dans le premier tiers gauche
        return pointerPosition.x <= leftThirdWidth;
    }

    private Vector2 GetPointerPosition()
    {
        // Récupérer la position de la souris ou du toucher
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    private Vector2 GetPointerDelta()
    {
        // Récupérer le delta de la souris ou du toucher
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.delta.ReadValue();
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue();
        }

        return Vector2.zero;
    }
}
