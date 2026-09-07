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

    [Header("Touch Controls")]
    [SerializeField] private bool useTouchInput = true;
    private int activeFingerId = -1;

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
            initialY = LaneManager.Instance.DeathLineY + catVisualOffsetY;
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

        // ==========================================
        // KEYBOARD CONTROLS (Great for Desktop/WebGL)
        // ==========================================
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

        // ==========================================
        // TOUCH & MOUSE INPUT SEPARATION
        // ==========================================
        if (useTouchInput && Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    void HandleTouchInput()
    {
        bool isThisCatLeft = laneStartIndex == 0;
        bool fingerStillActive = false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            float normalizedX = touch.position.x / Screen.width;
            bool isLeftSide = normalizedX < 0.5f;

            // Touch Began: Claim the new touch if it falls into this cat's designated side
            if (touch.phase == TouchPhase.Began)
            {
                if (isLeftSide == isThisCatLeft)
                {
                    if (activeFingerId == -1)
                    {
                        activeFingerId = touch.fingerId;
                        isDragging = true;
                    }
                }
            }

            // Track the specific finger assigned to this cat
            if (touch.fingerId == activeFingerId)
            {
                fingerStillActive = true;

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position);
                    currentLaneIndex = LaneManager.Instance.GetClosestLaneIndex(assignedLaneXPositions, worldPos.x);
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    activeFingerId = -1;
                    isDragging = false;
                }
            }
        }

        // Failsafe in case the tracked touch is lost abruptly
        if (activeFingerId != -1 && !fingerStillActive)
        {
            activeFingerId = -1;
            isDragging = false;
        }
    }

    void HandleMouseInput()
    {
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
            currentLaneIndex = LaneManager.Instance.GetClosestLaneIndex(assignedLaneXPositions, mouseWorldPos.x);
        }
    }
}