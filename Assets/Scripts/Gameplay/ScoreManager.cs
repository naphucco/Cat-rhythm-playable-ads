using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

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

    [Header("Score Animation Settings")]
    [Tooltip("Scale multiplier when the score updates.")]
    [SerializeField] private float punchScaleAmount = 0.2f;
    [Tooltip("Duration of the scale animation.")]
    [SerializeField] private float animationDuration = 0.2f;

    private int currentScore = 0;
    public int CurrentScore => currentScore;

    // Lưu lại kích thước gốc của UI text để tween xong trả về đúng cỡ cũ
    private Vector3 originalScale = Vector3.one;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
        }
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

            scoreText.transform.DOKill(true);
            scoreText.transform.DOScale(originalScale * (1f + punchScaleAmount), animationDuration / 2f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }
}