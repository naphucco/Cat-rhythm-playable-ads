using UnityEngine;

public class CatMoveController : MonoBehaviour
{
    [Header("Lane Configuration")]
    [SerializeField] private int laneStartIndex = 0; // Starting index in LaneManager (e.g., 0 for left cat, 2 for right cat)
    [SerializeField] private int laneCount = 2;       // Number of lanes this cat can use (e.g., 2 lanes)
    [SerializeField] private float snapSpeed = 20f;   // Smooth transition speed

    private Transform[] assignedLanes;
    private Camera mainCamera;
    private bool isDragging = false;
    private int currentLaneIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;

        // Fetch assigned lanes slice from LaneManager
        if (LaneManager.Instance != null)
        {
            assignedLanes = LaneManager.Instance.GetLaneSlice(laneStartIndex, laneCount);

            if (assignedLanes.Length > 0)
            {
                currentLaneIndex = 0;
                transform.position = assignedLanes[currentLaneIndex].position;
            }
        }
    }

    void Update()
    {
        HandleInputAndSnap();

        // Smoothly interpolate position to the target lane
        if (assignedLanes != null && assignedLanes.Length > 0)
        {
            Vector3 pos = transform.position;
            float targetX = assignedLanes[currentLaneIndex].position.x;
            pos.x = Mathf.Lerp(pos.x, targetX, snapSpeed * Time.deltaTime);
            transform.position = pos;
        }
    }

    void HandleInputAndSnap()
    {
        if (Input.GetMouseButtonDown(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging && assignedLanes != null && assignedLanes.Length > 0)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            
            // Find the closest lane within this cat's assigned subset
            currentLaneIndex = LaneManager.Instance.GetClosestLaneIndex(assignedLanes, mouseWorldPos.x);
        }
    }
}