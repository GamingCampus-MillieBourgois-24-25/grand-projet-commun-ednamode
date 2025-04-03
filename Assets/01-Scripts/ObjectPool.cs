using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Syst�me de pool d�objets g�n�rique avec singleton global.
/// Optimis� pour r�utiliser des GameObjects sans recr�ation.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// R�cup�re un objet depuis le pool, instanci� sous un parent donn�.
    /// </summary>
    /// <param name="prefab">Le prefab � utiliser.</param>
    /// <param name="parent">Le parent Transform dans lequel instancier.</param>
    /// <returns>Le GameObject actif.</returns>
    public GameObject Spawn(GameObject prefab, Transform parent)
    {
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        if (pool[prefab].Count > 0)
        {
            GameObject obj = pool[prefab].Dequeue();
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(prefab, parent);
        }
    }

    /// <summary>
    /// R�cup�re un objet depuis le pool � une position et une rotation donn�es.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        if (pool[prefab].Count > 0)
        {
            GameObject obj = pool[prefab].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(prefab, position, rotation);
        }
    }

    /// <summary>
    /// R�cup�re un objet depuis le pool � une position, rotation et parent donn�s.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        if (pool[prefab].Count > 0)
        {
            GameObject obj = pool[prefab].Dequeue();
            obj.transform.SetParent(parent, false);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(prefab, position, rotation, parent);
        }
    }

    /// <summary>
    /// Remet un objet dans le pool.
    /// </summary>
    public void Despawn(GameObject obj, GameObject prefab)
    {
        obj.SetActive(false);

        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        pool[prefab].Enqueue(obj);
    }
}
