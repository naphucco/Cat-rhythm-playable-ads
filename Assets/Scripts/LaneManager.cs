using UnityEngine;

public class LaneManager : MonoBehaviour
{
    [Header("Lane Viewport Setup (X ranges from 0.0 to 1.0 across the screen)")]
    [Tooltip("Normalized horizontal positions (0 to 1) for each lane across the screen width.")]
    [SerializeField] private float[] laneViewportX = new float[] { 0.2f, 0.4f, 0.6f, 0.8f };

    [Header("Vertical Heights Setup")]
    [Tooltip("Viewport Y position where candies spawn (1.0 is the top edge of the screen).")]
    [SerializeField] private float spawnViewportY = 1.05f;

    private Camera mainCamera;
    public static LaneManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        mainCamera = Camera.main;
    }

    /// <summary>
    /// Returns an array of calculated World X positions for a specific subset of lanes (for cats)
    /// </summary>
    public float[] GetLaneXSlice(int startIndex, int count)
    {
        if (laneViewportX == null || count <= 0) return new float[0];

        startIndex = Mathf.Clamp(startIndex, 0, laneViewportX.Length);
        count = Mathf.Clamp(count, 0, laneViewportX.Length - startIndex);

        float[] slice = new float[count];
        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(laneViewportX[startIndex + i], 0f, -mainCamera.transform.position.z));
            slice[i] = worldPos.x;
        }
        return slice;
    }

    /// <summary>
    /// Finds the index of the closest lane among the given world X positions subset
    /// </summary>
    public int GetClosestLaneIndex(float[] laneXPositions, float worldX)
    {
        if (laneXPositions == null || laneXPositions.Length == 0) return 0;

        int closestIndex = 0;
        float minDistance = Mathf.Abs(laneXPositions[0] - worldX);

        for (int i = 1; i < laneXPositions.Length; i++)
        {
            float distance = Mathf.Abs(laneXPositions[i] - worldX);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    /// <summary>
    /// Gets the dynamic world spawn position at the top of the screen for a specific lane index
    /// </summary>
    public Vector3 GetSpawnPosition(int laneIndex)
    {
        if (laneViewportX != null && laneIndex >= 0 && laneIndex < laneViewportX.Length)
        {
            // Map lane X and top screen Viewport Y to world coordinates
            Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(laneViewportX[laneIndex], spawnViewportY, -mainCamera.transform.position.z));
            worldPos.z = 0f;
            return worldPos;
        }

        return Vector3.zero;
    }
}