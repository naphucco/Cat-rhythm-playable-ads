using UnityEngine;
using TMPro;

/// <summary>
/// Simple FPS counter for debugging performance.
/// Lightweight - suitable for playable ads.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showFPS = true;
    [SerializeField] private float updateInterval = 0.5f; // How often to update display
    [SerializeField] private TextMeshProUGUI fpsText;

    [Header("Color Thresholds")]
    [SerializeField] private Color goodColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color badColor = Color.red;
    [SerializeField] private int warningThreshold = 30;
    [SerializeField] private int badThreshold = 20;

    private int _frameCount = 0;
    private float _currentFPS = 0f;
    private float _lastTime = 0f;

    private void Start()
    {
        // Auto-create Text if not assigned
        if (fpsText == null)
        {
            GameObject textGO = new GameObject("FPS_Text");
            textGO.transform.SetParent(transform);

            fpsText = textGO.AddComponent<TextMeshProUGUI>();

            // Set default appearance
            fpsText.fontSize = 32;
            fpsText.color = Color.white;
            fpsText.alignment = TextAlignmentOptions.TopLeft;

            // Set position (top-left corner)
            RectTransform rect = fpsText.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(200, 50);
        }

        if (!showFPS)
        {
            fpsText.gameObject.SetActive(false);
        }

        _lastTime = Time.realtimeSinceStartup;
        
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Update()
    {
        if (!showFPS) return;

        _frameCount++;
        float currentTime = Time.realtimeSinceStartup;
        float delta = currentTime - _lastTime;

        if (delta >= updateInterval)
        {
            // Calculate FPS over the interval
            _currentFPS = _frameCount / delta;

            // Reset counters
            _frameCount = 0;
            _lastTime = currentTime;

            // Update display
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (fpsText == null) return;

        string fpsString;
        string status = "";

        if (_currentFPS >= warningThreshold)
        {
            status = "";
        }
        else if (_currentFPS >= badThreshold)
        {
            status = " [!]";
        }
        else
        {
            status = " [!]";
        }

        fpsString = $"{_currentFPS:F1} FPS{status}";

        // Chọn màu
        Color color = _currentFPS >= warningThreshold ? goodColor :
                      _currentFPS >= badThreshold ? warningColor : badColor;

        fpsText.color = color;
        fpsText.text = fpsString;
    }

    private string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    /// <summary>
    /// Toggle FPS display on/off at runtime.
    /// </summary>
    public void ToggleDisplay()
    {
        showFPS = !showFPS;
        if (fpsText != null)
            fpsText.gameObject.SetActive(showFPS);
    }

    /// <summary>
    /// Get the current FPS value.
    /// </summary>
    public float GetCurrentFPS()
    {
        return _currentFPS;
    }
}