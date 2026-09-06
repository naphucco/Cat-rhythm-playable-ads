using UnityEngine;
using TMPro;
using System.Collections;
using System;

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

    [Header("Animation")]
    [SerializeField] private float displayDuration = 1.0f;
    [SerializeField] private float floatUpDistance = 80f;

    private int currentCombo = 0;
    private Coroutine feedbackCoroutine;
    private Camera mainCamera;
    private CatMoveController catMove;
    private IDisposable _subscription;

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
        Vector3 catWorldPos = transform.position;
        catWorldPos.y += 1.2f;

        if (mainCamera == null) mainCamera = Camera.main;
        Vector3 screenPos = mainCamera != null ? mainCamera.WorldToScreenPoint(catWorldPos) : catWorldPos;

        feedbackText.rectTransform.position = screenPos;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        feedbackText.transform.localScale = Vector3.one * 0.5f;
        Color color = feedbackText.color;
        color.a = 1f;
        feedbackText.color = color;

        float elapsed = 0f;
        Vector3 startPos = screenPos;
        Vector3 endPos = startPos + new Vector3(0, floatUpDistance, 0);

        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / displayDuration;

            feedbackText.rectTransform.position = Vector3.Lerp(startPos, endPos, t);

            float scale = t < 0.2f ? Mathf.Lerp(0.5f, 1.2f, t / 0.2f) : Mathf.Lerp(1.2f, 1f, (t - 0.2f) / 0.8f);
            feedbackText.transform.localScale = Vector3.one * scale;

            float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            color.a = alpha;
            feedbackText.color = color;

            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        feedbackCoroutine = null;
    }
}