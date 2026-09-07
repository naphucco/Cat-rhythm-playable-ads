using System.Collections;
using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    [Header("Backgrounds")]
    [SerializeField] private GameObject portraitBackground;
    [SerializeField] private GameObject landscapeBackground;

    [Header("Canvases")]
    [SerializeField] private GameObject portraitCanvas;
    [SerializeField] private GameObject landscapeCanvas;

    void Start()
    {
        StartCoroutine(InitOrientationRoutine());
    }

    private IEnumerator InitOrientationRoutine()
    {
        yield return null;
        UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        bool isLandscape = Screen.width > Screen.height;

        if (landscapeBackground != null) landscapeBackground.SetActive(isLandscape);
        if (portraitBackground != null) portraitBackground.SetActive(!isLandscape);

        if (landscapeCanvas != null) landscapeCanvas.SetActive(isLandscape);
        if (portraitCanvas != null) portraitCanvas.SetActive(!isLandscape);
    }
}