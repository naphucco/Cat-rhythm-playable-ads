using System.Collections.Generic;
using UnityEngine;

public class CatMoveController : MonoBehaviour
{
    [Header("Lane Configuration")]
    [SerializeField] private int laneStartIndex = 0; // Starting index in LaneManager (e.g., 0 for left cat, 2 for right cat)
    [SerializeField] private int laneCount = 2;      // Number of lanes this cat can use (e.g., 2 lanes)
    [SerializeField] private int initialLaneIndex = 0; // Default standing column upon initialization (calculated within this cat's laneCount range)
    [SerializeField] private float snapSpeed = 20f;   // Smooth transition speed for movement

    [Header("Visual Adjustment")]
    [SerializeField] private float catVisualOffsetY = -0.5f;

    private float[] assignedLaneXPositions;
    private Camera mainCamera;
    private bool isDragging = false;
    private int currentLaneIndex = 0;
    private float initialY;
    private float initialZ;

    // Static registry and global index property for CandyMover hit detection integration
    private static readonly List<CatMoveController> activeCats = new List<CatMoveController>();
    public int CurrentGlobalLaneIndex => laneStartIndex + currentLaneIndex;

    void OnEnable()
    {
        if (!activeCats.Contains(this))
            activeCats.Add(this);
    }

    void OnDisable()
    {
        activeCats.Remove(this);
    }

    /// <summary>
    /// Checks if any active cat is currently standing on the specified global lane index.
    /// </summary>
    public static bool IsLaneCaught(int laneIndex)
    {
        foreach (var cat in activeCats)
        {
            if (cat != null && cat.CurrentGlobalLaneIndex == laneIndex)
            {
                return true;
            }
        }
        return false;
    }

    void Start()
    {
        mainCamera = Camera.main;
        initialZ = transform.position.z;

        if (LaneManager.Instance != null)
        {
            initialY = LaneManager.Instance.HitLineY + catVisualOffsetY;
            assignedLaneXPositions = LaneManager.Instance.GetLaneXSlice(laneStartIndex, laneCount);

            if (assignedLaneXPositions != null && assignedLaneXPositions.Length > 0)
            {
                currentLaneIndex = Mathf.Clamp(initialLaneIndex, 0, assignedLaneXPositions.Length - 1);

                Vector3 pos = transform.position;
                pos.x = assignedLaneXPositions[currentLaneIndex];
                pos.y = initialY;
                pos.z = initialZ;
                transform.position = pos;
            }
        }
    }

    void Update()
    {
        HandleInputAndSnap();

        // Smoothly interpolate only the X position to the target lane, keeping Y and Z strictly locked
        if (assignedLaneXPositions != null && assignedLaneXPositions.Length > 0)
        {
            Vector3 pos = transform.position;
            float targetX = assignedLaneXPositions[currentLaneIndex];

            pos.x = Mathf.Lerp(pos.x, targetX, snapSpeed * Time.deltaTime);
            pos.y = initialY; // Ensure vertical position never shifts
            pos.z = initialZ; // Ensure depth remains unchanged

            transform.position = pos;
        }
    }

    void HandleInputAndSnap()
    {
        if (Input.GetMouseButtonDown(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging && assignedLaneXPositions != null && assignedLaneXPositions.Length > 0)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // Find the closest lane index within this cat's assigned viewport X subset
            currentLaneIndex = LaneManager.Instance.GetClosestLaneIndex(assignedLaneXPositions, mouseWorldPos.x);
        }
    }
}