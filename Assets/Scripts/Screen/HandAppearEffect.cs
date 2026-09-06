using UnityEngine;
using DG.Tweening;

public class HandAppearEffect : MonoBehaviour
{
    [SerializeField] private RectTransform handRect;
    [SerializeField] private float duration = 0.6f;

    public Tween Play()
    {
        handRect.gameObject.SetActive(true);
        handRect.localScale = Vector3.zero;
        return handRect.DOScale(1f, duration).SetEase(Ease.OutBack);
    }
}