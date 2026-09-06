using UnityEngine;
using DG.Tweening;
using System;

public class LoseScreenController : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private HoleEffect holeEffect;
    [SerializeField] private HandAppearEffect handEffect;
    [SerializeField] private ButtonFadeEffect buttonEffect;

    [Header("Sequence Delays")]
    [SerializeField] private float delayBeforeClose = 0.3f;

    private IDisposable _subscription;

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => GameManager.Instance)
            .Subscribe(this, mgr =>
            {
                mgr.OnLoseStateEntered += PlayLoseSequence;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    private void PlayLoseSequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(delayBeforeClose);
        seq.Append(holeEffect.PlayClose());
        seq.Append(handEffect.Play());
        seq.Append(holeEffect.PlayOpen());
        // seq.Append(buttonEffect.Play());

        seq.Play();
    }
}