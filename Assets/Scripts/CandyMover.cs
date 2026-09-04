using UnityEngine;

/// <summary>
/// Handles the precise movement of candies along the Y-axis from spawn to hit line,
/// synchronized strictly with the song timer.
/// </summary>
public class CandyMover : MonoBehaviour
{
    private float targetArrivalTime;
    private float spawnSongTime;
    private float startY;
    private float targetY;
    private bool isInitialized = false;

    /// <summary>
    /// Initializes the candy movement parameters upon spawning with a fixed travel duration.
    /// </summary>
    public void Initialize(float ta, float currentSongTime, Vector3 spawnPosition, float hitLineY, float advanceTime)
    {
        targetArrivalTime = ta;
        spawnSongTime = ta - advanceTime;

        transform.position = spawnPosition;
        startY = spawnPosition.y;
        targetY = hitLineY;

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        float currentSongTimer = NoteSpawner.Instance != null ? NoteSpawner.Instance.SongTimer : Time.time;

        float totalDuration = targetArrivalTime - spawnSongTime;
        if (totalDuration <= 0f) return;

        float elapsed = currentSongTimer - spawnSongTime;
        float progress = elapsed / totalDuration;

        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(startY, targetY, progress);
        transform.position = pos;

        // Deactivate and return to pool after it passes the hit line threshold
        if (currentSongTimer > targetArrivalTime + 0.5f)
        {
            isInitialized = false;
            gameObject.SetActive(false);
        }
    }
}