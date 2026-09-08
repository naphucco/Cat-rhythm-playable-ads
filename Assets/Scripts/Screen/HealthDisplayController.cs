using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Manages and displays the player's remaining health using UI heart images (fill/empty) with DOTween animations.
/// Listens to RhythmController's OnLivesChangedEvent to update dynamically.
/// </summary>
public class HealthDisplayController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent container or layout group that holds the heart image UI elements.")]
    [SerializeField] private Transform heartsContainer;

    [Tooltip("Array of Image components representing each heart slot.")]
    [SerializeField] private Image[] heartImages;

    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartFillSprite;   // t_igs_heart-fill--red_img
    [SerializeField] private Sprite heartEmptySprite; // t_igs_heart-empty--brown_img

    [Header("Animation Settings")]
    [SerializeField] private float punchScaleAmount = 0.3f;
    [SerializeField] private float animationDuration = 0.3f;

    private int previousLives = -1;

    private void Start()
    {
        // Automatically find all heart images in children if the array is empty
        if (heartImages == null || heartImages.Length == 0)
        {
            if (heartsContainer != null)
            {
                heartImages = heartsContainer.GetComponentsInChildren<Image>();
            }
            else
            {
                heartImages = GetComponentsInChildren<Image>();
            }
        }

        // Subscribe to lives changes from RhythmController
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnLivesChangedEvent += UpdateHealthDisplay;
            
            // Initialize with current lives if game is already active
            previousLives = RhythmController.Instance.CurrentLives;
            UpdateHealthDisplay(previousLives, RhythmController.Instance.CurrentLives);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnLivesChangedEvent -= UpdateHealthDisplay;
        }
    }

    /// <summary>
    /// Updates the visual state of the hearts based on current and max lives with punch scale animation on lost health.
    /// </summary>
    private void UpdateHealthDisplay(int currentLives, int maxLives)
    {
        if (heartImages == null) return;

        bool hasLostLife = previousLives != -1 && currentLives < previousLives;
        int lostIndex = currentLives; // The index of the heart that just became empty

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                // Only enable heart slots up to the maximum lives count; hide any excess slots
                if (i < maxLives)
                {
                    heartImages[i].gameObject.SetActive(true);

                    // If index is less than current lives, show filled red heart, otherwise show empty brown heart
                    if (i < currentLives)
                    {
                        heartImages[i].sprite = heartFillSprite;
                    }
                    else
                    {
                        heartImages[i].sprite = heartEmptySprite;

                        // Play punch scale animation on the specific heart that just turned empty
                        if (hasLostLife && i == lostIndex)
                        {
                            heartImages[i].transform.DOKill();
                            heartImages[i].transform.localScale = Vector3.one;
                            heartImages[i].transform.DOPunchScale(Vector3.one * punchScaleAmount, animationDuration, 10, 1f);
                        }
                    }
                }
                else
                {
                    // Hide extra heart slots if the UI array exceeds maxLives configuration
                    heartImages[i].gameObject.SetActive(false);
                }
            }
        }

        previousLives = currentLives;
    }
}