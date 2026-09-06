using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a multi-type object pool for candies in a rhythm/WebGL game.
/// Uses a Coroutine to stagger the instantiation process across multiple frames,
/// completely eliminating startup CPU performance spikes.
/// </summary>
public class Pooler : Singleton<Pooler>
{
    [System.Serializable]
    public struct PoolConfig
    {
        [Tooltip("Identifier for the candy type, matching the lane or JSON ID.")]
        public ObjectType id;
        
        [Tooltip("Prefab corresponding to this specific candy type.")]
        public GameObject prefab;
        
        [Tooltip("Initial number of instances pre-allocated in the pool.")]
        public int poolSize;
    }

    [Header("Pool Configurations")]
    [SerializeField] private List<PoolConfig> poolConfigs;
    
    // Dictionary storing queues of pooled game objects categorized by their candy ID
    private Dictionary<ObjectType, Queue<GameObject>> poolDictionary;

    // Flag to check if the pool is fully initialized before the NoteSpawner starts requesting items
    public bool IsInitialized { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();

        poolDictionary = new Dictionary<ObjectType, Queue<GameObject>>();        
        // Execute the staggered initialization coroutine instead of instantiating everything at once
        StartCoroutine(InitializePoolsStaggered());
    }

    /// <summary>
    /// Coroutine that sequentially initializes pool items one frame at a time to protect the framerate.
    /// </summary>
    private IEnumerator InitializePoolsStaggered()
    {
        foreach (var config in poolConfigs)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < config.poolSize; i++)
            {
                // Instantiate the object and hide it
                GameObject candy = Instantiate(config.prefab);
                candy.SetActive(false);
                candy.transform.SetParent(transform);
                objectQueue.Enqueue(candy);

                // Pause execution for 1 frame after each instantiation to share the CPU load
                yield return null; 
            }

            poolDictionary.Add(config.id, objectQueue);
        }

        IsInitialized = true;
    }

    /// <summary>
    /// Retrieves a candy instance from the pool based on the specified candy ID.
    /// </summary>
    public GameObject GetCandy(ObjectType id, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(id))
        {
            Debug.LogWarning($"[MultiCandyPooler] Candy ID {id} pool does not exist!");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[id];

        // Reuse an existing pooled object if available
        if (queue.Count > 0)
        {
            GameObject candy = queue.Dequeue();
            candy.SetActive(true);
            candy.transform.position = position;
            candy.transform.rotation = rotation;
            return candy;
        }
        else
        {
            // Fallback safety measure: dynamically instantiate a new object if the pool runs dry
            foreach (var config in poolConfigs)
            {
                if (config.id == id)
                {
                    GameObject candy = Instantiate(config.prefab, position, rotation);
                    return candy;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Returns a candy back to the pool after it goes off-screen or gets collected.
    /// </summary>
    public void ReturnCandy(ObjectType id, GameObject candy)
    {
        candy.SetActive(false);
        candy.transform.SetParent(transform);
        
        if (poolDictionary.ContainsKey(id))
        {
            poolDictionary[id].Enqueue(candy);
        }
        else
        {
            Destroy(candy);
        }
    }
}