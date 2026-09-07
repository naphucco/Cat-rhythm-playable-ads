using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class ResponsiveBackground : MonoBehaviour
{
    [SerializeField] private Sprite portraitSprite;
    [SerializeField] private Sprite landscapeSprite;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    private void Update()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (_spriteRenderer == null) 
            _spriteRenderer = GetComponent<SpriteRenderer>();

        bool isLandscape = Screen.width > Screen.height;
        Sprite targetSprite = isLandscape ? landscapeSprite : portraitSprite;

        if (targetSprite != null && _spriteRenderer.sprite != targetSprite)
        {
            _spriteRenderer.sprite = targetSprite;
        }
    }
}