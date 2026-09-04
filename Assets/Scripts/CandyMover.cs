using UnityEngine;

public class CandyMover : MonoBehaviour
{
    private float targetArrivalTime;
    private float spawnTime;
    private float startY;
    private float targetY;
    private bool isInitialized = false;

    public void Initialize(float ta, float currentSongTime, Vector3 spawnPosition, float hitLineY)
    {
        targetArrivalTime = ta;
        spawnTime = currentSongTime;

        transform.position = spawnPosition;
        startY = spawnPosition.y;
        targetY = hitLineY;

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        float songTime = Time.time;

        float totalDuration = targetArrivalTime - spawnTime;
        if (totalDuration <= 0f) return;

        float elapsed = songTime - spawnTime;
        float progress = elapsed / totalDuration;

        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(startY, targetY, progress);
        transform.position = pos;

        if (songTime > targetArrivalTime + 0.5f)
        {
            isInitialized = false;
            gameObject.SetActive(false);
        }
    }
}