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

    [Header("Keyboard Controls")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;

    private float[] assignedLaneXPositions;
    private Camera mainCamera;
    private bool isDragging = false;
    private int currentLaneIndex = 0;
    private float initialY;
    private float initialZ;

    public int CurrentGlobalLaneIndex => laneStartIndex + currentLaneIndex;

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
        if (assignedLaneXPositions == null || assignedLaneXPositions.Length == 0) return;

        if (Input.GetKeyDown(moveLeftKey))
        {
            currentLaneIndex = Mathf.Clamp(currentLaneIndex - 1, 0, assignedLaneXPositions.Length - 1);
            isDragging = false;
        }
        else if (Input.GetKeyDown(moveRightKey))
        {
            currentLaneIndex = Mathf.Clamp(currentLaneIndex + 1, 0, assignedLaneXPositions.Length - 1);
            isDragging = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            float screenNormalizedX = Input.mousePosition.x / Screen.width;
            bool isLeftSideOfScreen = screenNormalizedX < 0.5f;
            bool isThisCatOnLeft = laneStartIndex == 0;

            if (isLeftSideOfScreen == isThisCatOnLeft)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // Find the closest lane index within this cat's assigned viewport X subset
            currentLaneIndex = LaneManager.Instance.GetClosestLaneIndex(assignedLaneXPositions, mouseWorldPos.x);
        }
    }
}