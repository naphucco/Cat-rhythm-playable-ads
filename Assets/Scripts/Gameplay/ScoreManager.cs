using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using System;

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

    private Vector3 originalScale = Vector3.one;

    private IDisposable _subscription;

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
        _subscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller => controller.OnNoteHitEvent += HandleNoteHit)
            .AddTo(this);
    }

    private void OnDisable()
    {
        // No need to unsubscribe manually since AddTo has already handled it
        // But if you want to be safe, you still can:
        _subscription?.Dispose();
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