using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controls the tutorial sequence: listens for input to start the game, 
/// animates guide images back and forth, and fades out when gameplay begins.
/// </summary>
public class TutorialController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Left guide parent RectTransform containing image components.")]
    [SerializeField] private RectTransform leftGuide;
    
    [Tooltip("Right guide parent RectTransform containing image components.")]
    [SerializeField] private RectTransform rightGuide;

    private Image[] leftImages;
    private Image[] rightImages;

    [Header("Movement Settings")]
    [Tooltip("Distance of back-and-forth movement in pixels.")]
    [SerializeField] private float moveDistance = 30f;
    [Tooltip("Duration for a single movement direction.")]
    [SerializeField] private float moveDuration = 0.6f;

    [Header("Fade Settings")]
    [Tooltip("Fade-out duration when the tutorial concludes.")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Sequence pulseSequence;

    private void Awake()
    {
        // Automatically fetch all Image components from the parent and its children
        if (leftGuide != null) leftImages = leftGuide.GetComponentsInChildren<Image>();
        if (rightGuide != null) rightImages = rightGuide.GetComponentsInChildren<Image>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered += HandlePlayingStateEntered;
        }

        StartMovement();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered -= HandlePlayingStateEntered;
        }

        // Kill the sequence to prevent memory leaks
        pulseSequence?.Kill();
    }

    private void Update()
    {
        // Only check for input if the game is currently in the Tutorial state
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Tutorial)
        {
            // Detect mouse click or screen touch
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                // Trigger state change and start the rhythm sequence
                GameManager.Instance.StartPlaying();
            }
        }
    }

    private void StartMovement()
    {
        if (leftGuide == null || rightGuide == null) return;

        pulseSequence = DOTween.Sequence();

        Vector2 leftOriginalPos = leftGuide.anchoredPosition;
        Vector2 rightOriginalPos = rightGuide.anchoredPosition;

        pulseSequence.Join(leftGuide.DOAnchorPosX(leftOriginalPos.x - moveDistance, moveDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine));
        pulseSequence.Join(rightGuide.DOAnchorPosX(rightOriginalPos.x + moveDistance, moveDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine));
    }

    private void HandlePlayingStateEntered()
    {
        // Stop looping movement immediately
        pulseSequence?.Kill();

        float duration = fadeDuration;

        // Fade out all images found in the left guide hierarchy
        if (leftImages != null)
        {
            foreach (var img in leftImages)
            {
                if (img != null) img.DOFade(0f, duration);
            }
        }

        // Fade out all images found in the right guide hierarchy
        if (rightImages != null)
        {
            foreach (var img in rightImages)
            {
                if (img != null) img.DOFade(0f, duration);
            }
        }

        DOVirtual.DelayedCall(duration, () =>
        {
            if (leftGuide != null) leftGuide.gameObject.SetActive(false);
            if (rightGuide != null) rightGuide.gameObject.SetActive(false);
            
            enabled = false;
        });
    }
}