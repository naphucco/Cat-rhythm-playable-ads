using UnityEngine;
using DG.Tweening;
using System;

public class ScreenController : MonoBehaviour
{
    [SerializeField] private GameObject selectSongMenu;

    [Header("Effects")]
    [SerializeField] private HoleEffect holeEffect;
    [SerializeField] private HandAppearEffect handEffect;

    [Header("Sequence Delays")]
    [SerializeField] private float delayBeforeClose = 0.3f;

    private IDisposable _subscription;

    private void Start()
    {
        selectSongMenu.SetActive(false);
    }

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => GameManager.Instance)
            .Subscribe(this, mgr =>
            {
                mgr.OnLoseStateEntered += PlayScreenSequence;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    private void PlayScreenSequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(delayBeforeClose);
        seq.Append(holeEffect.PlayClose());
        seq.Append(handEffect.Play());

        seq.AppendCallback(() =>
        {
            if (selectSongMenu != null)
                selectSongMenu.SetActive(true);

            GameManager.Instance?.ProceedToPickNextSong();
        });

        seq.Append(holeEffect.PlayOpen());

        seq.Play();
    }
}