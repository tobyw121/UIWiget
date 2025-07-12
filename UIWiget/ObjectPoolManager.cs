// Dateiname: ObjectPoolManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ein einfacher Singleton-Manager für das Pooling von GameObjects.
/// Reduziert Instanziierungs-Overhead und Garbage Collection.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Holt ein Objekt aus dem Pool oder instanziiert ein neues, falls der Pool leer ist.
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!_pool.ContainsKey(prefab) || _pool[prefab].Count == 0)
        {
            GameObject newObj = Instantiate(prefab);
            // Speichern Sie eine Referenz zum Prefab, um das Objekt später zurückzugeben
            var pooledObj = newObj.AddComponent<PooledObject>();
            pooledObj.OriginalPrefab = prefab;
            return newObj;
        }

        GameObject objFromPool = _pool[prefab].Dequeue();
        objFromPool.SetActive(true);
        return objFromPool;
    }

    /// <summary>
    /// Gibt ein Objekt in den Pool zurück.
    /// </summary>
    public void Release(GameObject instance)
    {
        if (instance == null) return;

        var pooledObj = instance.GetComponent<PooledObject>();
        if (pooledObj == null || pooledObj.OriginalPrefab == null)
        {
            Debug.LogWarning("Versuch, ein Objekt ohne PooledObject-Komponente oder Prefab-Referenz freizugeben. Zerstöre es stattdessen.", instance);
            Destroy(instance);
            return;
        }

        GameObject prefab = pooledObj.OriginalPrefab;
        if (!_pool.ContainsKey(prefab))
        {
            _pool[prefab] = new Queue<GameObject>();
        }

        _pool[prefab].Enqueue(instance);
        instance.SetActive(false);
    }
}

/// <summary>
/// Hilfskomponente, um die Referenz zum ursprünglichen Prefab zu speichern.
/// </summary>
public class PooledObject : MonoBehaviour
{
    public GameObject OriginalPrefab;
}