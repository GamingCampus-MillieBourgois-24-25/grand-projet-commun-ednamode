using UnityEngine;

public class InstanceBehaviour : MonoBehaviour
{
    private static InstanceBehaviour _instance;

    public static InstanceBehaviour Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("InstanceBehaviour");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<InstanceBehaviour>();
            }
            return _instance;
        }
    }
}
