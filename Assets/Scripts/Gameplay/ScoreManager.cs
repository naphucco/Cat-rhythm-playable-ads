using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.UI;

/// <summary>
/// Manages score tracking and UI updates by subscribing to RhythmController hit events.
/// </summary>
public class ScoreManager : Singleton<ScoreManager>
{
    [Header("References")]
    [Tooltip("Reference to SongSettings to lookup score values.")]
    [SerializeField] private SongSettings songSettings;
    [Tooltip("Optional UI Text component to display the current score.")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject heathImage;

    [Header("Score Animation Settings")]
    [Tooltip("Scale multiplier when the score updates.")]
    [SerializeField] private float punchScaleAmount = 0.2f;
    [Tooltip("Duration of the scale animation.")]
    [SerializeField] private float animationDuration = 0.2f;

    private int currentScore = 0;
    public int CurrentScore => currentScore;

    private Vector3 originalScale = Vector3.one;

    private IDisposable _hitSubscription;
    private IDisposable _stateSubscription;

    protected override void Awake()
    {
        base.Awake();

        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
        }
    }

    private void OnEnable()
    {
        _hitSubscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller => controller.OnNoteHitEvent += HandleNoteHit)
            .AddTo(this);

        _stateSubscription = this.WhenReady(() => GameManager.Instance)
            .Subscribe(this, mgr =>
            {
                mgr.OnPickNextSongStateEntered += HandlePickNextSongState;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _hitSubscription?.Dispose();
        _stateSubscription?.Dispose();
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

    private void HandlePickNextSongState()
    {
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }

        if (heathImage != null)
        {
            heathImage.SetActive(false);
        }
    }
}