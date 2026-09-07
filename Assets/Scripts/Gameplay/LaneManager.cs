using UnityEngine;

[ExecuteAlways]
public class LaneManager : Singleton<LaneManager>
{
    [Header("Lane Viewport Setup (Portrait)")]
    [Tooltip("Normalized horizontal positions for Portrait mode.")]
    [SerializeField] private float[] portraitLaneViewportX = new float[] { 0.2f, 0.4f, 0.6f, 0.8f };

    [Header("Lane Viewport Setup (Landscape)")]
    [Tooltip("Normalized horizontal positions for Landscape mode (clamped closer to center).")]
    [SerializeField] private float[] landscapeLaneViewportX = new float[] { 0.35f, 0.45f, 0.55f, 0.65f };

    [Header("Hitline")]
    [SerializeField] private float portraitHitLineViewportY = 0.18f;
    [SerializeField] private float landscapeHitLineViewportY = 0.25f;

    [Tooltip("Viewport Y position where candies spawn (1.0 is the top edge of the screen).")]
    [SerializeField] private float spawnViewportY = 1.05f;

    public float DeathLineY { get; private set; }

    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();

        mainCamera = Camera.main;
        CalculateHitLine();
    }

    private void OnRectTransformDimensionsChange()
    {
        // Tự động tính lại khi thay đổi độ phân giải hoặc xoay màn hình (cả Editor lẫn Runtime)
        CalculateHitLine();
    }

    private void CalculateHitLine()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            bool isLandscape = Screen.width > Screen.height;
            float activeHitLineY = isLandscape ? landscapeHitLineViewportY : portraitHitLineViewportY;
            
            DeathLineY = mainCamera.ViewportToWorldPoint(new Vector3(0f, activeHitLineY, -mainCamera.transform.position.z)).y;
        }
    }

    /// <summary>
    /// Gets the appropriate lane array based on current screen orientation.
    /// </summary>
    private float[] GetActiveLaneViewportX()
    {
        bool isLandscape = Screen.width > Screen.height;
        return isLandscape ? landscapeLaneViewportX : portraitLaneViewportX;
    }

    /// <summary>
    /// Returns an array of calculated World X positions for a specific subset of lanes (for cats)
    /// </summary>
    public float[] GetLaneXSlice(int startIndex, int count)
    {
        float[] activeLanes = GetActiveLaneViewportX();

        if (activeLanes == null || count <= 0) return new float[0];

        startIndex = Mathf.Clamp(startIndex, 0, activeLanes.Length);
        count = Mathf.Clamp(count, 0, activeLanes.Length - startIndex);

        float[] slice = new float[count];
        for (int i = 0; i < count; i++)
        {
            if (mainCamera != null)
            {
                Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(activeLanes[startIndex + i], 0f, -mainCamera.transform.position.z));
                slice[i] = worldPos.x;
            }
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
        float[] activeLanes = GetActiveLaneViewportX();

        if (activeLanes != null && laneIndex >= 0 && laneIndex < activeLanes.Length && mainCamera != null)
        {
            Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(activeLanes[laneIndex], spawnViewportY, -mainCamera.transform.position.z));
            worldPos.z = 0f;
            return worldPos;
        }

        return Vector3.zero;
    }
}