using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controls the tutorial sequence: listens for input to start the game, 
/// animates guide images back and forth, and fades out when gameplay begins.
/// Supports responsive adjustments for both Portrait and Landscape layouts (Positions & Movement).
/// </summary>
public class TutorialScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tutorialContainer;

    [Tooltip("Left guide parent RectTransform containing image components.")]
    [SerializeField] private RectTransform leftGuide;

    [Tooltip("Right guide parent RectTransform containing image components.")]
    [SerializeField] private RectTransform rightGuide;

    [Tooltip("Dialog RectTransform for position adjustments between orientations.")]
    [SerializeField] private RectTransform dialogRect;

    [Tooltip("Dialog image component (fades out).")]
    [SerializeField] private Image dialogImage;

    private Image[] leftImages;
    private Image[] rightImages;

    [Header("Layout Settings (Portrait)")]
    [SerializeField] private Vector2 portraitDialogPos = new Vector2(0f, 150f);
    [SerializeField] private float portraitMoveDistance = 30f;

    [Header("Layout Settings (Landscape)")]
    [SerializeField] private Vector2 landscapeDialogPos = new Vector2(0f, 50f);
    [SerializeField] private float landscapeMoveDistance = 50f;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float fadeDuration = 0.4f;

    private Sequence pulseSequence;

    private void Awake()
    {
        ApplyLayoutSettings();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered += HandlePlayingStateEntered;
        }
        tutorialContainer.SetActive(true);
        StartMovement();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered -= HandlePlayingStateEntered;
        }

        pulseSequence?.Kill();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Tutorial)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                GameManager.Instance.StartPlaying();
            }
        }
    }

    private void ApplyLayoutSettings()
    {
        // Dynamically adjust the dialog's anchored position based on the current screen orientation (Portrait vs. Landscape)
        bool isLandscape = Screen.width > Screen.height;

        if (dialogRect != null)
        {
            dialogRect.anchoredPosition = isLandscape ? landscapeDialogPos : portraitDialogPos;
        }
    }

    private void StartMovement()
    {
        if (leftGuide == null || rightGuide == null) return;

        pulseSequence = DOTween.Sequence();

        // Automatically select the movement distance based on the current screen orientation
        bool isLandscape = Screen.width > Screen.height;
        float currentMoveDistance = isLandscape ? landscapeMoveDistance : portraitMoveDistance;

        Vector2 leftOriginalPos = leftGuide.anchoredPosition;
        Vector2 rightOriginalPos = rightGuide.anchoredPosition;

        pulseSequence.Join(leftGuide.DOAnchorPosX(leftOriginalPos.x - currentMoveDistance, moveDuration).SetEase(Ease.InOutSine));
        pulseSequence.Join(rightGuide.DOAnchorPosX(rightOriginalPos.x + currentMoveDistance, moveDuration).SetEase(Ease.InOutSine));

        pulseSequence.SetLoops(-1, LoopType.Yoyo);
    }

    private void HandlePlayingStateEntered()
    {
        pulseSequence?.Kill();

        float duration = fadeDuration;

        if (leftImages != null)
        {
            foreach (var img in leftImages)
            {
                if (img != null) img.DOFade(0f, duration);
            }
        }

        if (rightImages != null)
        {
            foreach (var img in rightImages)
            {
                if (img != null) img.DOFade(0f, duration);
            }
        }

        if (dialogImage != null)
        {
            dialogImage.DOFade(0f, duration);
        }

        DOVirtual.DelayedCall(duration, () =>
        {
            if (leftGuide != null) leftGuide.gameObject.SetActive(false);
            if (rightGuide != null) rightGuide.gameObject.SetActive(false);
            if (dialogImage != null) dialogImage.gameObject.SetActive(false);

            enabled = false;
        });
    }
}