using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Rotatable : MonoBehaviour
{
    [SerializeField] private Button rotateLeftButton;
    [SerializeField] private Button rotateRightButton;
    [SerializeField] private float speed = 1;
    [SerializeField] private bool inverted;

    private float rotationDirection = 0f;
    private bool rotateAllowed = false;

    private void Start()
    {
        AddEventTrigger(rotateLeftButton.gameObject, EventTriggerType.PointerDown, _ => StartRotation(-1));
        AddEventTrigger(rotateLeftButton.gameObject, EventTriggerType.PointerUp, _ => StopRotation());

        AddEventTrigger(rotateRightButton.gameObject, EventTriggerType.PointerDown, _ => StartRotation(1));
        AddEventTrigger(rotateRightButton.gameObject, EventTriggerType.PointerUp, _ => StopRotation());
    }

    private void StartRotation(float direction)
    {
        rotationDirection = direction;
        if (!rotateAllowed)
        {
            rotateAllowed = true;
            StartCoroutine(Rotate());
        }
    }

    private void StopRotation()
    {
        rotateAllowed = false;
    }

    private IEnumerator Rotate()
    {
        while (rotateAllowed)
        {
            float rotationAmount = rotationDirection * speed;
            transform.Rotate(Vector3.up, (inverted ? -rotationAmount : rotationAmount), Space.World);
            yield return null;
        }
    }

    private void AddEventTrigger(GameObject obj, EventTriggerType type, System.Action<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(eventData => action(eventData));
        trigger.triggers.Add(entry);
    }
}
