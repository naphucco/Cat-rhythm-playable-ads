using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectSongScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform handImage;
    [SerializeField] private RectTransform song1Image;
    [SerializeField] private RectTransform song2Image;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.8f;
    [SerializeField] private float punchDuration = 0.6f;
    [SerializeField] private float handZoomScale = 0.8f;
    [SerializeField] private float cardZoomScale = 1.2f;
    [SerializeField] private float handPunchAngle = 30f;

    [Header("Position Offset (Optional)")]
    [SerializeField] private Vector2 fingerOffset = Vector2.zero;

    private Sequence loopSequence;

    private void Start()
    {
        Button btn1 = song1Image.GetComponent<Button>();
        Button btn2 = song2Image.GetComponent<Button>();

        if (btn1 != null)
        {
            btn1.onClick.AddListener(() =>
            {
                Debug.Log("=== SONG 1 CLICKED ===");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
        else
        {
            Debug.LogWarning("song1Image has no Button component!");
        }

        if (btn2 != null)
        {
            btn2.onClick.AddListener(() =>
            {
                Debug.Log("=== SONG 2 CLICKED ===");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
        else
        {
            Debug.LogWarning("song2Image has no Button component!");
        }
    }

    public void PlaySelectLoop()
    {
        if (handImage == null || song1Image == null || song2Image == null)
            return;

        Canvas.ForceUpdateCanvases();

        RectTransform parentRect = handImage.parent as RectTransform;

        Vector2 pos1 = (Vector2)parentRect.InverseTransformPoint(song1Image.position) + fingerOffset;
        Vector2 pos2 = (Vector2)parentRect.InverseTransformPoint(song2Image.position) + fingerOffset;

        Vector3 handDefaultScale = Vector3.one;
        Vector3 song1DefaultScale = Vector3.one;
        Vector3 song2DefaultScale = Vector3.one;
        Vector3 defaultRotation = Vector3.zero;
        Vector3 punchRotation = new Vector3(0, 0, handPunchAngle);

        handImage.anchoredPosition = pos1;

        loopSequence = DOTween.Sequence();

        loopSequence.Append(handImage.DOAnchorPos(pos1, moveDuration).SetEase(Ease.OutQuad));

        Sequence song1Interaction = DOTween.Sequence();
        song1Interaction.Join(handImage.DOScale(handZoomScale, punchDuration / 2).SetEase(Ease.OutQuad));
        song1Interaction.Join(handImage.DORotate(punchRotation, punchDuration / 2).SetEase(Ease.OutQuad));
        song1Interaction.Join(song1Image.DOScale(cardZoomScale, punchDuration / 2).SetEase(Ease.OutQuad));

        song1Interaction.Append(handImage.DOScale(handDefaultScale, punchDuration / 2).SetEase(Ease.InQuad));
        song1Interaction.Join(handImage.DORotate(defaultRotation, punchDuration / 2).SetEase(Ease.InQuad));
        song1Interaction.Join(song1Image.DOScale(song1DefaultScale, punchDuration / 2).SetEase(Ease.InQuad));

        loopSequence.Append(song1Interaction);

        loopSequence.Append(handImage.DOAnchorPos(pos2, moveDuration).SetEase(Ease.InOutQuad));

        Sequence song2Interaction = DOTween.Sequence();
        song2Interaction.Join(handImage.DOScale(handZoomScale, punchDuration / 2).SetEase(Ease.OutQuad));
        song2Interaction.Join(handImage.DORotate(punchRotation, punchDuration / 2).SetEase(Ease.OutQuad));
        song2Interaction.Join(song2Image.DOScale(cardZoomScale, punchDuration / 2).SetEase(Ease.OutQuad));

        song2Interaction.Append(handImage.DOScale(handDefaultScale, punchDuration / 2).SetEase(Ease.InQuad));
        song2Interaction.Join(handImage.DORotate(defaultRotation, punchDuration / 2).SetEase(Ease.InQuad));
        song2Interaction.Join(song2Image.DOScale(song2DefaultScale, punchDuration / 2).SetEase(Ease.InQuad));

        loopSequence.Append(song2Interaction);

        loopSequence.Append(handImage.DOAnchorPos(pos1, moveDuration).SetEase(Ease.InOutQuad));

        loopSequence.SetLoops(-1);
    }

    private void OnDestroy()
    {
        loopSequence?.Kill();
    }

    public void SelectSong()
    {
        Debug.Log("seeeeeeeeeeee");
    }
}