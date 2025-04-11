using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCharacterHold : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private bool _isHolding = false;
    public event Action<bool> OnHoldingStateChanged; // Événement déclenché lorsque isHolding change

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

    private bool IsHolding
    {
        get => _isHolding;
        set
        {
            if (_isHolding != value)
            {
                _isHolding = value;
                OnHoldingStateChanged?.Invoke(_isHolding); // Déclencher l'événement
                Debug.Log($"isHolding a changé : {_isHolding}");
            }
        }
    }

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        if (IsPointerInLeftThird())
        {
            IsHolding = true; // Utiliser la propriété pour déclencher l'événement
            currentRotationSpeed = 0f;
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        IsHolding = false; 
    }
        

    private void Update()
    {
        if (IsHolding)
        {
            Vector2 inputDelta = GetInputDelta();

            currentRotationSpeed = inputDelta.x*100;
            if (currentRotationSpeed > maxRotationSpeed)
            {
                currentRotationSpeed = maxRotationSpeed;
            }
            else if (currentRotationSpeed < -maxRotationSpeed)
            {
                currentRotationSpeed = -maxRotationSpeed;
            }
            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);
        }
        else if (currentRotationSpeed != 0f)
        {
            // Appliquer la rotation
            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);

            // Réduire progressivement la vitesse de rotation
            float deceleration = decelerationRate * Time.deltaTime;
            if (currentRotationSpeed > 0f)
            {
                currentRotationSpeed = Mathf.Max(0f, currentRotationSpeed - deceleration); // Réduire vers 0
            }
            else if (currentRotationSpeed < 0f)
            {
                currentRotationSpeed = Mathf.Min(0f, currentRotationSpeed + deceleration); // Augmenter vers 0
            }
        }

    }

    private bool IsPointerInLeftThird()
    {
        Vector2 pointerPosition = GetPointerPosition();
        float screenWidth = Screen.width;
        float leftThirdWidth = screenWidth / 3f;
        return pointerPosition.x <= leftThirdWidth;
    }

    private Vector2 GetPointerPosition()
    {
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


    private Vector2 GetInputDelta()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.delta.ReadValue();
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue();
        }

        return Vector2.zero;
    }

}
