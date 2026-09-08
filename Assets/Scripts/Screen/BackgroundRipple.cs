using UnityEngine;
using System.Collections;
using System;

public class RipplePulse : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material backgroundMaterial;

    [Header("Settings")]
    [SerializeField] private float pulseDuration = 0.7f;
    [SerializeField] private float maxRippleTime = 2.0f;

    private Coroutine rippleCoroutine;
    private IDisposable _subscription;

    private void Start()
    {
        if (backgroundMaterial == null)
            Debug.LogError("[RipplePulse] backgroundMaterial is NULL!");
    }

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller =>
            {
                controller.OnNoteHitEvent += OnNoteHit;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    private void OnNoteHit(int laneIndex, ObjectType candyType)
    {
        Debug.Log($"[RipplePulse] OnNoteHit: lane={laneIndex}, candyType={candyType}");
        if (candyType == ObjectType.Lollipop_Long)
        {
            TriggerRipple();
        }
    }

    public void TriggerRipple()
    {
        if (backgroundMaterial == null)
        {
            return;
        }
        if (rippleCoroutine != null)
            StopCoroutine(rippleCoroutine);
        rippleCoroutine = StartCoroutine(PlayRipple());
    }

    private IEnumerator PlayRipple()
    {
        backgroundMaterial.SetFloat("_RippleTime", 0f);

        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / pulseDuration;
            float value = Mathf.Lerp(0f, maxRippleTime, progress);
            backgroundMaterial.SetFloat("_RippleTime", value);
            yield return null;
        }

        backgroundMaterial.SetFloat("_RippleTime", maxRippleTime);
        backgroundMaterial.SetFloat("_RippleTime", 0f);

        Debug.Log("[RipplePulse] PlayRipple finished");
        rippleCoroutine = null;
    }
}