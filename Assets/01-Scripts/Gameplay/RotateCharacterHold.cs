using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCharacterHold : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private bool isHolding = false;

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
            transform.Rotate(-Vector3.up, mouseDelta.x * Time.deltaTime * 100f);
        }
    }
}
