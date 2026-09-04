using UnityEngine;

public class LaneManager : MonoBehaviour
{
    [Header("Lane Setup (Ordered left to right)")]
    [SerializeField] private Transform[] allLanes; // Array of all lanes in the scene

    public static LaneManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Returns a subset of lanes for a specific cat based on array slicing (index range)
    /// </summary>
    public Transform[] GetLaneSlice(int startIndex, int count)
    {
        if (allLanes == null || count <= 0) return new Transform[0];

        startIndex = Mathf.Clamp(startIndex, 0, allLanes.Length);
        count = Mathf.Clamp(count, 0, allLanes.Length - startIndex);

        Transform[] slice = new Transform[count];
        System.Array.Copy(allLanes, startIndex, slice, 0, count);
        return slice;
    }

    /// <summary>
    /// Finds the index of the closest lane among the given subset
    /// </summary>
    public int GetClosestLaneIndex(Transform[] lanes, float worldX)
    {
        if (lanes == null || lanes.Length == 0) return 0;

        int closestIndex = 0;
        float minDistance = Mathf.Abs(lanes[0].position.x - worldX);

        for (int i = 1; i < lanes.Length; i++)
        {
            float distance = Mathf.Abs(lanes[i].position.x - worldX);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Gets the world X coordinate of a lane by its direct global index (used for NoteSpawner matching JSON data)
    /// </summary>
    public float GetLaneX(int laneIndex)
    {
        if (allLanes != null && laneIndex >= 0 && laneIndex < allLanes.Length)
        {
            return allLanes[laneIndex].position.x;
        }
        return 0f;
    }
}