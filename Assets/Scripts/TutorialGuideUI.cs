using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controls tutorial prompt images moving back and forth, and fading out when the tutorial ends.
/// Requires DOTween.
/// </summary>
public class TutorialGuideUI : MonoBehaviour
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
            GameManager.Instance.OnPlayingStateEntered += FadeOutAndDisable;
        }

        StartMovement();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered -= FadeOutAndDisable;
        }

        // Kill the sequence to prevent memory leaks
        pulseSequence?.Kill();
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

    private void FadeOutAndDisable()
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

        // Deactivate object after fade finishes
        DOVirtual.DelayedCall(duration, () =>
        {
            gameObject.SetActive(false);
        });
    }
}