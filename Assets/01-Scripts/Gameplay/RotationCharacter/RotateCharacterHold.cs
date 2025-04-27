using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCharacterHold : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private bool _isHolding = false;
    public event Action<bool> OnHoldingStateChanged;
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

        // S'abonner � l'�v�nement pour g�rer la rotation
        OnHoldingStateChanged += HandleHoldingStateChanged;
    }

    private void OnDisable()
    {
        inputActions.UI.Hold.started -= OnHoldStarted;
        inputActions.UI.Hold.canceled -= OnHoldCanceled;
        inputActions.Disable();

        // Se d�sabonner de l'�v�nement
        OnHoldingStateChanged -= HandleHoldingStateChanged;
    }

    private bool IsHolding
    {
        get => _isHolding;
        set
        {
            if (_isHolding != value)
            {
                _isHolding = value;
                OnHoldingStateChanged?.Invoke(_isHolding); // D�clencher l'�v�nement
                Debug.Log($"isHolding a chang� : {_isHolding}");
            }
        }
    }

    private void OnHoldStarted(InputAction.CallbackContext context)
    {
        if (IsPointerInLeftThird())
        {
            IsHolding = true; // Utiliser la propri�t� pour d�clencher l'�v�nement
            currentRotationSpeed = 0f;
        }
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        IsHolding = false;
    }

    private void HandleHoldingStateChanged(bool isHolding)
    {
        if (isHolding)
        {
            // Commencer la rotation
            StartRotating();
        }
        else
        {
            // Arr�ter la rotation et appliquer la d�c�l�ration
            StopRotating();
        }
    }

    private void StartRotating()
    {
        // Utiliser une coroutine pour g�rer la rotation pendant que IsHolding est vrai
        StartCoroutine(RotateWhileHolding());
    }

    private void StopRotating()
    {
        // Appliquer la d�c�l�ration
        StartCoroutine(ApplyDeceleration());
    }

    private System.Collections.IEnumerator RotateWhileHolding()
    {
        while (IsHolding)
        {
            Vector2 inputDelta = GetInputDelta();

            currentRotationSpeed = inputDelta.x * 100;
            currentRotationSpeed = Mathf.Clamp(currentRotationSpeed, -maxRotationSpeed, maxRotationSpeed);

            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);
            yield return null; // Attendre la prochaine frame
        }
    }

    private System.Collections.IEnumerator ApplyDeceleration()
    {
        while (currentRotationSpeed != 0f)
        {
            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);

            float deceleration = decelerationRate * Time.deltaTime;
            if (currentRotationSpeed > 0f)
            {
                currentRotationSpeed = Mathf.Max(0f, currentRotationSpeed - deceleration); // R�duire vers 0
            }
            else if (currentRotationSpeed < 0f)
            {
                currentRotationSpeed = Mathf.Min(0f, currentRotationSpeed + deceleration); // Augmenter vers 0
            }

            yield return null; // Attendre la prochaine frame
        }
    }

    private bool IsPointerInLeftThird()
    {
        Vector2 pointerPosition = GetPointerPosition();
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float leftFortyPercentWidth = screenWidth * 0.4f;
        float bottomEightyPercentHeight = screenHeight * 0.9f;

        return pointerPosition.x <= leftFortyPercentWidth && pointerPosition.y <= bottomEightyPercentHeight;
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
