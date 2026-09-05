using UnityEngine;

/// <summary>
/// Manages the tutorial overlay and listens for player touch or click input to transition into the Playing state.
/// </summary>
public class TutorialController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The tutorial UI panel or canvas object that should be hidden when gameplay starts.")]
    [SerializeField] private GameObject tutorialPanel;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered += HandlePlayingStateEntered;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayingStateEntered -= HandlePlayingStateEntered;
        }
    }

    private void Update()
    {
        // Only check for input if the game is currently in the Tutorial state
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Tutorial)
        {
            // Detect mouse click (for PC/Editor/WebGL testing) or screen touch (for Mobile/Web)
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                // Trigger state change and start the rhythm sequence
                GameManager.Instance.StartPlaying();
            }
        }
    }

    private void HandlePlayingStateEntered()
    {
        // Hide the tutorial panel when gameplay begins
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Disable this tutorial controller object to save performance
        gameObject.SetActive(false);
    }
}