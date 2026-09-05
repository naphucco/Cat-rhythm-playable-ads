using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages score tracking and UI updates by subscribing to RhythmController hit events.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("References")]
    [Tooltip("Reference to SongSettings to lookup score values.")]
    [SerializeField] private SongSettings songSettings;
    [Tooltip("Optional UI Text component to display the current score.")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private int currentScore = 0;
    public int CurrentScore => currentScore;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void OnEnable()
    {
        StartCoroutine(SubscribeToRhythmRoutine());
    }

    private void OnDisable()
    {
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnNoteHitEvent -= HandleNoteHit;
        }
    }

    private IEnumerator SubscribeToRhythmRoutine()
    {
        while (RhythmController.Instance == null)
        {
            yield return null;
        }

        RhythmController.Instance.OnNoteHitEvent += HandleNoteHit;
    }

    private void HandleNoteHit(int laneIndex, ObjectType candyType)
    {
        if (songSettings == null) return;

        int scoreToAdd = songSettings.GetScore(candyType);
        currentScore += scoreToAdd;

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}