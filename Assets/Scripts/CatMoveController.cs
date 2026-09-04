using UnityEngine;

public class CatMoveController : MonoBehaviour
{
    [Header("Lane Configuration")]
    [SerializeField] private int laneStartIndex = 0; // Starting index in LaneManager (e.g., 0 for left cat, 2 for right cat)
    [SerializeField] private int laneCount = 2;       // Number of lanes this cat can use (e.g., 2 lanes)
    [SerializeField] private float snapSpeed = 20f;   // Smooth transition speed for movement

    private float[] assignedLaneXPositions;
    private Camera mainCamera;
    private bool isDragging = false;
    private int currentLaneIndex = 0;
    private float initialY;
    private float initialZ;

    void Start()
    {
        mainCamera = Camera.main;

        // Lock initial Y and Z coordinates so the cat stays on its designated vertical track
        initialY = transform.position.y;
        initialZ = transform.position.z;

        // Fetch assigned lane X positions slice from LaneManager
        if (LaneManager.Instance != null)
        {
            assignedLaneXPositions = LaneManager.Instance.GetLaneXSlice(laneStartIndex, laneCount);

            if (assignedLaneXPositions.Length > 0)
            {
                currentLaneIndex = 0;
                
                // Set initial position matching the first assigned lane X, keeping Y and Z locked
                Vector3 startPos = new Vector3(assignedLaneXPositions[currentLaneIndex], initialY, initialZ);
                transform.position = startPos;
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