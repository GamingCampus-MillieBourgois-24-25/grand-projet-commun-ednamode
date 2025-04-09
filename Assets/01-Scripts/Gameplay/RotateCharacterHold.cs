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
    private float maxRotationSpeed = 100f;
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
        isHolding = true;
        currentRotationSpeed = 0f;
    }

    private void OnHoldCanceled(InputAction.CallbackContext context)
    {
        isHolding = false;
    }

    private void Update()
    {
        if (isHolding)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentRotationSpeed = mouseDelta.x*100;
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
            transform.Rotate(-Vector3.up, currentRotationSpeed * Time.deltaTime);
            currentRotationSpeed = Mathf.MoveTowards(currentRotationSpeed, 0f, decelerationRate * Time.deltaTime);
        }
    }
}
