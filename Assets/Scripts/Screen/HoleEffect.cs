using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HoleEffect : MonoBehaviour
{
    [SerializeField] private Image holeImage;
    [SerializeField] private Vector4 startSize = new Vector4(2.0f, 2.0f, 0f, 0f);
    [SerializeField] private Vector4 endSize = new Vector4(0.01f, 0.01f, 0f, 0f);
    [SerializeField] private float duration = 0.8f;

    private Material holeMaterial;

    private void Start()
    {
        holeMaterial = new Material(holeImage.material);
        holeImage.material = holeMaterial;

        holeImage.gameObject.SetActive(false);
        holeMaterial.SetVector("_HoleSize", startSize);
    }

    public Tween Play()
    {
        holeImage.gameObject.SetActive(true);
        holeMaterial.SetVector("_HoleSize", startSize);

        return DOTween.To(
            () => holeMaterial.GetVector("_HoleSize"),
            (v) => holeMaterial.SetVector("_HoleSize", v),
            endSize,
            duration
        ).SetEase(Ease.InCubic);
    }

    private void OnDestroy()
    {
        // Cleanup instance material để tránh memory leak
        if (holeMaterial != null)
        {
            Destroy(holeMaterial);
        }
    }
}