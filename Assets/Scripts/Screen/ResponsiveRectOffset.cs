using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ResponsiveRectOffset : MonoBehaviour
{
    [SerializeField] private Vector2 portraitPosition = new Vector2(0f, -170f);
    [SerializeField] private Vector2 landscapePosition = new Vector2(0f, -80f);

    private RectTransform _rectTransform;
    private bool _isLandscape;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplyLayout();
    }

    private void Update()
    {
        bool currentLandscape = Screen.width > Screen.height;
        
        if (currentLandscape != _isLandscape || !Application.isPlaying)
        {
            ApplyLayout();
        }
    }

    private void ApplyLayout()
    {
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        _isLandscape = Screen.width > Screen.height;
        _rectTransform.anchoredPosition = _isLandscape ? landscapePosition : portraitPosition;
    }
}