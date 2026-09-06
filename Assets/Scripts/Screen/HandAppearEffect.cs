using UnityEngine;
using DG.Tweening;

public class HandAppearEffect : MonoBehaviour
{
    [SerializeField] private RectTransform handRect;
    [SerializeField] private float appearDuration = 0.6f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float shrinkDuration = 0.3f;
    [SerializeField] private Vector3 startScale = Vector3.zero;
    [SerializeField] private Vector3 peakScale = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private Vector3 endScale = Vector3.zero;

    private void Start()
    {
        handRect.gameObject.SetActive(false);
    }

    public Sequence Play()
    {
        handRect.gameObject.SetActive(true);
        handRect.localScale = startScale;

        Sequence seq = DOTween.Sequence();

        seq.Append(handRect.DOScale(peakScale, appearDuration / 2f).SetEase(Ease.OutBack));
        seq.Append(handRect.DOScale(Vector3.one, appearDuration / 2f).SetEase(Ease.InBack));

        seq.AppendInterval(holdDuration);

        seq.Append(handRect.DOScale(endScale, shrinkDuration).SetEase(Ease.InBack));

        return seq;
    }
}