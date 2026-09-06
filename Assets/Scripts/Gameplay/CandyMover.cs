using UnityEngine;

/// <summary>
/// Handles constant-speed movement along the Y-axis. Candy moves smoothly from spawn to missY,
/// checking hit/miss precisely at targetArrivalTime without altering speed mid-air.
/// Automatically clears itself when the game ends (win or lose), instead of lingering on screen.
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

    // Design note - why OnSongStopRequested (RhythmController) instead of OnGameLose/GameManager:
    //
    // 1) OnSongStopRequested fires on BOTH TriggerWin() and TriggerLose(), so clearing candies
    //    on Win is also covered "for free" - not just Lose. Binding to OnGameLose only would
    //    miss the Win case. Clearing candies is tied to the audio/song lifecycle ("song stopped"),
    //    not to the gameplay outcome (win vs lose) - so it should bind to the event that reflects
    //    that lifecycle, not the outcome.
    //
    // 2) Architecture layering: RhythmController is the core/domain layer that owns gameplay
    //    logic and spawns CandyMover instances directly. GameManager sits ABOVE RhythmController
    //    (it listens to RhythmController's events and translates them into high-level states:
    //    Tutorial/Playing/Win/Lose) for the presentation layer (UI, CatAnimationController...)
    //    to consume. Dependencies flow one-way: RhythmController -> GameManager -> Presentation.
    //    If CandyMover (core layer) listened to GameManager (a layer above it) instead, that
    //    would create a conceptual reverse-dependency (core depending on something that itself
    //    depends on core). Listening directly to RhythmController keeps CandyMover at the same
    //    layer it belongs to.
    private void OnEnable()
    {
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnSongStopRequested += HandleGameEnded;
        }
    }

    private void OnDisable()
    {
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnSongStopRequested -= HandleGameEnded;
        }
    }

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
                Deactivate();
                return;
            }
        }

        // Check miss condition when the candy reaches missY after passing the hit line
        if (hasPassedHitLine && pos.y <= missY)
        {
            RhythmController.Instance?.RegisterMiss(laneIndex);
            Deactivate();
        }
    }

    /// <summary>
    /// Called when the song stops (either Win or Lose) - immediately clears this candy
    /// from screen instead of letting it keep falling, avoiding a "stuck/laggy" feel.
    /// </summary>
    private void HandleGameEnded()
    {
        if (!isActive) return; // already inactive/pooled, nothing to clean up
        Deactivate();
    }

    /// <summary>
    /// Shared cleanup: stop updating and return this candy to the pool (or just disable it).
    /// </summary>
    private void Deactivate()
    {
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