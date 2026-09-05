using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        ScaleBackground();
    }

    private void Update()
    {
        // Update scaling in the Editor when changing the Game tab aspect ratio or resizing
        if (!Application.isPlaying)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            ScaleBackground();
        }
    }

    private void ScaleBackground()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || mainCamera == null) return;

        // Calculate world screen dimensions to fit the camera bounds perfectly
        float worldScreenHeight = mainCamera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * mainCamera.aspect;

        Sprite sprite = spriteRenderer.sprite;
        float spriteWidth = sprite.rect.width / sprite.pixelsPerUnit;
        float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;

        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;
        float maxScale = Mathf.Max(scaleX, scaleY);

        // Adjust scale and lock X position to the camera, while leaving Y completely manual
        transform.localScale = new Vector3(maxScale, maxScale, 1f);

        Vector3 pos = transform.position;
        pos.x = mainCamera.transform.position.x; // Center horizontally with the camera
        // pos.y is preserved exactly where you manually position it in the Inspector
        transform.position = pos;
    }
}