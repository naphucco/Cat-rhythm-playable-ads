using UnityEngine;

/// <summary>
/// Handles constant-speed movement along the Y-axis. Candy moves smoothly from spawn to missY,
/// checking hit/miss precisely at targetArrivalTime without altering speed mid-air.
/// </summary>
public class CandyMover : MonoBehaviour
{
    private ObjectType candyId;
    private float targetArrivalTime;
    private float spawnSongTime;
    private float startY;
    private float missY;
    private float speed; 
    private int laneIndex;
    private bool isActive = false;
    private bool hasPassedHitLine = false;

    public void Initialize(ObjectType id, float ta, Vector3 spawnPosition, float hitLineY, float missY, float travelTime, int laneIndex)
    {
        candyId = id;
        targetArrivalTime = ta;
        spawnSongTime = ta - travelTime;

        transform.position = spawnPosition;
        startY = spawnPosition.y;
        this.missY = missY;
        this.laneIndex = laneIndex;
        hasPassedHitLine = false;

        float distanceToHitLine = startY - hitLineY;
        speed = travelTime > 0f ? distanceToHitLine / travelTime : 0f;

        isActive = true;
    }

    private void Update()
    {
        if (!isActive) return;

        float currentSongTimer = RhythmController.Instance != null ? RhythmController.Instance.SongTimer : Time.time;
        float elapsed = currentSongTimer - spawnSongTime;

        Vector3 pos = transform.position;
        pos.y = startY - speed * elapsed;
        transform.position = pos;

        // Check hit condition precisely when the candy reaches the target arrival time at the hit line
        if (!hasPassedHitLine && currentSongTimer >= targetArrivalTime)
        {
            hasPassedHitLine = true;
            if (CatMoveController.IsLaneCaught(laneIndex))
            {
                RhythmController.Instance?.RegisterHit(laneIndex, candyId);
                isActive = false;
                
                if (Pooler.Instance != null)
                {
                    Pooler.Instance.ReturnCandy(candyId, gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                return;
            }
        }

        // Check miss condition when the candy reaches missY after passing the hit line
        if (hasPassedHitLine && pos.y <= missY)
        {
            RhythmController.Instance?.RegisterMiss(laneIndex);
            isActive = false;
            
            if (Pooler.Instance != null)
            {
                Pooler.Instance.ReturnCandy(candyId, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}