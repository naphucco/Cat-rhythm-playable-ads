using UnityEngine;
using TMPro;
using System.Collections;
using System;
using DG.Tweening;

public class ComboFeedbackController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Combo Thresholds")]
    [SerializeField] private int lowComboMax = 4;
    [SerializeField] private int midComboMax = 9;

    [Header("Messages")]
    [SerializeField] private string[] lowComboMessages = { "Sweet!", "Nice!" };
    [SerializeField] private string[] midComboMessages = { "Yummy!", "Delicious!" };
    [SerializeField] private string[] highComboMessages = { "Tasty!", "Perfect!" };

    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 1.0f;
    [SerializeField] private float floatUpDistance = 80f;
    [SerializeField] private float initialScale = 0.5f;
    [SerializeField] private float peakScale = 1.2f;
    [SerializeField] private float scaleDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float verticalOffset = 1.2f;

    private int currentCombo = 0;
    private Coroutine feedbackCoroutine;
    private Camera mainCamera;
    private CatMoveController catMove;
    private IDisposable _subscription;
    private Sequence currentSequence;

    private void Start()
    {
        mainCamera = Camera.main;
        catMove = GetComponent<CatMoveController>();
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller =>
            {
                controller.OnNoteHitEvent += OnNoteHit;
                controller.OnNoteMissEvent += OnNoteMiss;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();

        if (currentSequence != null)
        {
            currentSequence.Kill();
            currentSequence = null;
        }
    }

    private void OnNoteHit(int laneIndex, ObjectType candyType)
    {
        if (catMove != null && catMove.CurrentGlobalLaneIndex != laneIndex) return;
        currentCombo++;
        ShowComboFeedback();
    }

    private void OnNoteMiss(int laneIndex)
    {
        if (catMove != null && catMove.CurrentGlobalLaneIndex != laneIndex) return;
        currentCombo = 0;
    }

    private void ShowComboFeedback()
    {
        if (feedbackText == null) return;
        string message = GetMessageByCombo();

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ShowFeedbackAnimation(message));
    }

    private string GetMessageByCombo()
    {
        if (currentCombo > midComboMax)
            return highComboMessages[UnityEngine.Random.Range(0, highComboMessages.Length)];
        else if (currentCombo > lowComboMax)
            return midComboMessages[UnityEngine.Random.Range(0, midComboMessages.Length)];
        else
            return lowComboMessages[UnityEngine.Random.Range(0, lowComboMessages.Length)];
    }

    private IEnumerator ShowFeedbackAnimation(string message)
    {
        // Kill existing sequence if any
        if (currentSequence != null)
        {
            currentSequence.Kill();
            currentSequence = null;
        }

        // Kill all tweens on this text
        feedbackText.DOKill();
        feedbackText.transform.DOKill();
        feedbackText.rectTransform.DOKill();

        // Calculate position above cat's head
        Vector3 catWorldPos = transform.position;
        catWorldPos.y += verticalOffset;

        if (mainCamera == null) mainCamera = Camera.main;
        Vector3 screenPos = mainCamera != null ? mainCamera.WorldToScreenPoint(catWorldPos) : catWorldPos;

        // Reset EVERYTHING before starting new animation
        feedbackText.rectTransform.position = screenPos;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        feedbackText.transform.localScale = Vector3.one * initialScale;

        Color color = feedbackText.color;
        color.a = 1f;
        feedbackText.color = color;

        // Create new sequence
        currentSequence = DOTween.Sequence();

        // Phase 1: Scale up
        currentSequence.Append(feedbackText.transform.DOScale(peakScale, scaleDuration));
        currentSequence.Join(feedbackText.DOFade(1f, scaleDuration));

        // Phase 2: Scale back to normal
        currentSequence.Append(feedbackText.transform.DOScale(1f, scaleDuration));

        // Phase 3: Wait
        float waitTime = displayDuration - (scaleDuration * 2) - fadeOutDuration;
        if (waitTime > 0) currentSequence.AppendInterval(waitTime);

        // Phase 4: Float up and fade out
        Vector3 endPos = screenPos + new Vector3(0, floatUpDistance, 0);
        currentSequence.Append(feedbackText.rectTransform.DOMove(endPos, fadeOutDuration));
        currentSequence.Join(feedbackText.DOFade(0f, fadeOutDuration));

        yield return currentSequence.WaitForCompletion();

        feedbackText.gameObject.SetActive(false);
        currentSequence = null;
        feedbackCoroutine = null;
    }
}