using UnityEngine;

public class DebugPoints : MonoBehaviour
{
    public Vector3 pointA = new Vector3(-39f, 2.15f, 116f);
    public Vector3 pointB = new Vector3(-43f, 2.15f, 117.26f);
    public Vector3 pointC = new Vector3(-43f, 2.15f, 134.19f);
    public Vector3 pointD = new Vector3(-49f, 2.15f, 116.18f);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointA, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(pointB, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pointC, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointD, 0.5f);
    }
}