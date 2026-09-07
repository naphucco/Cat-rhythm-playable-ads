using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveRectSize : MonoBehaviour
{
    [SerializeField] private Vector2 portraitSize = new Vector2(166, 218);
    [SerializeField] private Vector2 landscapeSize = new Vector2(150f, 200f);

    private RectTransform _rectTransform;
    private bool _isLandscape;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySize();
    }

    private void Update()
    {
        bool currentLandscape = Screen.width > Screen.height;
        if (currentLandscape != _isLandscape || !Application.isPlaying)
        {
            ApplySize();
        }
    }

    private void ApplySize()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        _isLandscape = Screen.width > Screen.height;
        _rectTransform.sizeDelta = _isLandscape ? landscapeSize : portraitSize;
    }
}