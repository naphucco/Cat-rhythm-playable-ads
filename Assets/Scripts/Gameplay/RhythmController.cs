using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the rhythm game's core execution loop, note spawning, and integrates with LaneManager for lane positioning.
/// </summary>
public class RhythmController : MonoBehaviour
{
    public static RhythmController Instance;

    [Header("Game Configuration")]
    [SerializeField] private bool autoStart;

    [Header("Data Configuration")]
    [Tooltip("Reference to the global SongSettings ScriptableObject asset containing thresholds and timings.")]
    [SerializeField] private SongSettings songSettings;

    [Tooltip("JSON TextAsset containing the exported MIDI song chart data.")]
    [SerializeField] private TextAsset jsonChartFile;

    [Tooltip("Viewport Y position for the absolute bottom screen where missed candies disappear.")]
    [SerializeField] private float missViewportY = 0.0f;

    [Header("Lane Mapping Rules")]
    [Tooltip("Define which lane indices correspond to Cat 1 (left) vs Cat 2 (right) for candy type resolution.")]
    [SerializeField] private int cat1MaxLaneIndex = 1;

    private List<NoteData> allNotes = new List<NoteData>();
    private int currentIndex = 0;
    private float songTimer = 0f;
    private bool isPlaying = false;
    private bool isGameEnded = false;

    // Decoupled events for Audio and Game State management
    public event Action OnSongPlayRequested;
    public event Action OnSongStopRequested;
    public event Action OnGameWin;
    public event Action OnGameLose;

    // Expose public events for hit and miss notifications (OnNoteHitEvent now includes laneIndex and candyType)
    public event Action<int, ObjectType> OnNoteHitEvent;
    public event Action<int> OnNoteMissEvent;

    /// <summary>
    /// Exposes the synchronized song timer publicly for external components.
    /// </summary>
    public float SongTimer => songTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        if (songSettings == null)
        {
            Debug.LogError("[RhythmController] SongSettings asset is not assigned in the inspector!");
            return;
        }

        // Pass songSettings to ChartLoader to map raw PIDs to clean lane indices
        allNotes = ChartLoader.LoadAndSortChart(jsonChartFile, songSettings);

        if (autoStart)
        {
            StartGame();
        }
    }

    /// <summary>
    /// Public method to explicitly start the game session.
    /// </summary>
    public void StartGame()
    {
        if (isPlaying || isGameEnded) return;
        StartCoroutine(InitializeAndPlayRoutine());
    }

    private IEnumerator InitializeAndPlayRoutine()
    {
        while (Pooler.Instance == null || !Pooler.Instance.IsInitialized)
        {
            yield return null;
        }

        songTimer = -songSettings.noteTravelTime;
        currentIndex = 0;
        isPlaying = true;
        isGameEnded = false;
    }

    private void Update()
    {
        if (!isPlaying || isGameEnded || allNotes == null || songSettings == null) return;

        float previousTimer = songTimer;

        if (AudioManager.Instance != null && AudioManager.Instance.IsPlaying)
        {
            songTimer = AudioManager.Instance.CurrentAudioTime;
        }
        else
        {
            songTimer += Time.deltaTime;
        }

        // Trigger audio playback request via event when intro countdown reaches zero
        if (previousTimer < 0f && songTimer >= 0f)
        {
            OnSongPlayRequested?.Invoke();
        }

        // Look-ahead spawning window
        while (currentIndex < allNotes.Count && allNotes[currentIndex].ta - songTimer <= songSettings.noteTravelTime)
        {
            SpawnNote(allNotes[currentIndex]);
            currentIndex++;
        }

        // Check for Win Condition
        if (currentIndex >= allNotes.Count && allNotes.Count > 0)
        {
            float lastNoteTime = allNotes[allNotes.Count - 1].ta;
            if (songTimer > lastNoteTime + 2.0f)
            {
                TriggerWin();
            }
        }
    }

    private void SpawnNote(NoteData note)
    {
        int laneIndex = note.LaneIndex;

        // Determine the visual prefab type based on lane index, velocity, and duration
        ObjectType candyID = ResolveCandyID(laneIndex, note.v, note.d);

        // Fetch spawn position at the top of the screen aligned with the correct lane X
        Vector3 spawnPosition = LaneManager.Instance.GetSpawnPosition(laneIndex);

        // Calculate world Y coordinate of the absolute bottom screen for missed notes
        Camera mainCam = Camera.main;
        float missY = mainCam.ViewportToWorldPoint(new Vector3(0f, missViewportY, -mainCam.transform.position.z)).y;

        // Request pooled candy instance
        GameObject candy = Pooler.Instance.GetCandy(candyID, spawnPosition, Quaternion.identity);

        if (candy != null)
        {
            var mover = candy.GetComponent<CandyMover>();
            if (mover != null)
            {
                float hitLineY = LaneManager.Instance.HitLineY;

                mover.Initialize(
                    candyID,
                    note.ta,
                    spawnPosition,
                    hitLineY,
                    missY,
                    songSettings.noteTravelTime,
                    note.LaneIndex
                );
            }
        }
    }

    /// <summary>
    /// Resolves which candy variant to instantiate based on lane assignment and note properties.
    /// </summary>
    private ObjectType ResolveCandyID(int laneIndex, int velocity, float duration)
    {
        bool isLong = duration > songSettings.longNoteThreshold;
        bool isStrong = velocity > songSettings.strongVelocityThreshold;

        // Determine if the note belongs to Cat 1 (left) or Cat 2 (right) using clean lane index
        if (laneIndex <= cat1MaxLaneIndex)
        {
            if (isLong) return ObjectType.Candy1_Long;
            return isStrong ? ObjectType.Candy1_Strong : ObjectType.Candy1_Normal;
        }
        else
        {
            if (isLong) return ObjectType.Candy2_Long;
            return isStrong ? ObjectType.Candy2_Strong : ObjectType.Candy2_Normal;
        }
    }

    /// <summary>
    /// Callback executed when a candy is successfully caught by a cat at the hit line.
    /// </summary>
    public void RegisterHit(int laneIndex, ObjectType candyType)
    {
        OnNoteHitEvent?.Invoke(laneIndex, candyType);
    }

    /// <summary>
    /// Callback executed when a candy passes the hit line without being caught and reaches the bottom.
    /// </summary>
    public void RegisterMiss(int laneIndex)
    {
        OnNoteMissEvent?.Invoke(laneIndex);
    }

    /// <summary>
    /// Triggers game win sequence and fires corresponding events.
    /// </summary>
    public void TriggerWin()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isPlaying = false;

        OnSongStopRequested?.Invoke();
        OnGameWin?.Invoke();
    }

    /// <summary>
    /// Triggers game lose sequence and fires corresponding events.
    /// </summary>
    public void TriggerLose()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isPlaying = false;

        OnSongStopRequested?.Invoke();
        OnGameLose?.Invoke();
    }
}