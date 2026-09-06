using UnityEngine;
using DG.Tweening;

public class ButtonFadeEffect : MonoBehaviour
{
    [SerializeField] private CanvasGroup buttonGroup;
    [SerializeField] private float duration = 0.4f;

    public Tween Play()
    {
        buttonGroup.gameObject.SetActive(true);
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
        return buttonGroup.DOFade(1f, duration);
    }
}