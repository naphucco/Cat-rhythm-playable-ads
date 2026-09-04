using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the rhythm game's core execution loop, including manual start control, 
/// audio synchronization, drift correction, win/loss conditions, and modular candy spawning.
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    public static NoteSpawner Instance;

    [Header("Data & Audio Configuration")]
    [Tooltip("Reference to the global SongSettings ScriptableObject asset containing thresholds and timings.")]
    [SerializeField] private SongSettings songSettings;

    [Tooltip("JSON TextAsset containing the exported MIDI song chart data.")]
    [SerializeField] private TextAsset jsonChartFile;

    [Tooltip("AudioSource component responsible for playing the background music track.")]
    [SerializeField] private AudioSource songAudioSource;

    [Header("Spawn Positioning")]
    [Tooltip("Y coordinate of the judgment/hit line where candies should arrive (scene/layout specific).")]
    [SerializeField] private float hitLineY = -3.5f;

    [Header("Scene Lane Setup")]
    [Tooltip("Direct mapping configuration linking MIDI PID values to their respective scene world transforms.")]
    [SerializeField]
    private List<LaneConfig> laneConfigs = new List<LaneConfig>
    {
        new LaneConfig { pid = 0 },
        new LaneConfig { pid = 2 },
        new LaneConfig { pid = 3 },
        new LaneConfig { pid = 5 }
    };

    // Internal collection storing all parsed and sorted notes from the chart
    private List<NoteData> allNotes = new List<NoteData>();

    // Tracks the current index of the note being evaluated for look-ahead spawning
    private int currentIndex = 0;

    // Master synchronized timer representing the active song playback position
    private float songTimer = 0f;

    // Flags indicating current gameplay state
    private bool isPlaying = false;
    private bool isGameEnded = false;

    // Events for external UI or GameManager listeners
    public event Action OnGameWin;
    public event Action OnGameLose;

    /// <summary>
    /// Exposes the synchronized song timer publicly so external components (such as CandyMover) can query exact timing.
    /// </summary>
    public float SongTimer => songTimer;

    /// <summary>
    /// Enforces the singleton pattern and locks application frame rate for performance stability.
    /// </summary>
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

    /// <summary>
    /// Parses chart data via ChartLoader on start, but waits for an explicit StartGame() call to begin.
    /// </summary>
    private void Start()
    {
        if (songSettings == null)
        {
            Debug.LogError("[NoteSpawner] SongSettings asset is not assigned in the inspector!");
            return;
        }

        allNotes = ChartLoader.LoadAndSortChart(jsonChartFile);

        // StartGame()
    }

    /// <summary>
    /// Public method to explicitly start the game session when triggered by a UI button or external controller.
    /// </summary>
    public void StartGame()
    {
        if (isPlaying || isGameEnded) return;
        StartCoroutine(InitializeAndPlayRoutine());
    }

    /// <summary>
    /// Coroutine waiting for the MultiCandyPooler to initialize completely, 
    /// then sets up the negative lead-in time buffer before starting playback.
    /// </summary>
    private IEnumerator InitializeAndPlayRoutine()
    {
        while (MultiCandyPooler.Instance == null || !MultiCandyPooler.Instance.IsInitialized)
        {
            yield return null;
        }

        songTimer = -songSettings.spawnInAdvanceTime;
        currentIndex = 0;
        isPlaying = true;
        isGameEnded = false;
    }

    /// <summary>
    /// Main execution loop running every frame. 
    /// Handles time progression, audio triggering, drift correction, note look-ahead evaluation, and win conditions.
    /// </summary>
    private void Update()
    {
        if (!isPlaying || isGameEnded || allNotes == null || songSettings == null) return;

        float previousTimer = songTimer;
        songTimer += Time.deltaTime;

        // Trigger the background audio source precisely when the intro countdown transitions from negative to zero
        if (previousTimer < 0f && songTimer >= 0f)
        {
            if (songAudioSource != null)
            {
                songAudioSource.Play();
            }
        }

        // Drift correction mechanism: periodically re-sync songTimer with actual AudioSource.time 
        // using smooth interpolation (Lerp) only when playing and when drift exceeds safe thresholds.
        if (songAudioSource != null && songAudioSource.isPlaying && songTimer >= 0f)
        {
            float audioTime = songAudioSource.time;
            float drift = audioTime - songTimer;

            if (Mathf.Abs(drift) > 0.05f)
            {
                songTimer = Mathf.Lerp(songTimer, audioTime, 0.1f);
            }
        }

        // Look-ahead spawning window: evaluates upcoming notes and triggers them 
        // exactly 'spawnInAdvanceTime' seconds before their designated arrival time (ta).
        while (currentIndex < allNotes.Count && allNotes[currentIndex].ta - songTimer <= songSettings.spawnInAdvanceTime)
        {
            SpawnNote(allNotes[currentIndex]);
            currentIndex++;
        }

        // Check for Win Condition: All notes have been spawned and the song timer has passed the final note's arrival time plus buffer
        if (currentIndex >= allNotes.Count && allNotes.Count > 0)
        {
            float lastNoteTime = allNotes[allNotes.Count - 1].ta;
            if (songTimer > lastNoteTime + 2.0f)
            {
                TriggerWin();
            }
        }
    }

    /// <summary>
    /// Resolves the correct candy type and lane transform for a given note, 
    /// retrieves the object from the pool, and initializes its movement parameters.
    /// </summary>
    private void SpawnNote(NoteData note)
    {
        PooType candyID = ResolveCandyID(note.pid, note.v, note.d);
        Transform laneTransform = GetLaneTransform(note.pid);

        if (laneTransform == null)
        {
            Debug.LogWarning($"[NoteSpawner] Invalid or missing lane transform configuration for PID: {note.pid}");
            return;
        }

        Vector3 spawnPosition = laneTransform.position;
        GameObject candy = MultiCandyPooler.Instance.GetCandy(candyID, spawnPosition, Quaternion.identity);

        if (candy != null)
        {
            var mover = candy.GetComponent<CandyMover>();
            if (mover != null)
            {
                mover.Initialize(note.ta, songTimer, spawnPosition, hitLineY, songSettings.spawnInAdvanceTime);
            }
        }
    }

    /// <summary>
    /// Maps a note's PID, velocity, and duration to an explicit PooType enum 
    /// utilizing thresholds provided by the global SongSettings asset.
    /// </summary>
    private PooType ResolveCandyID(int pid, int velocity, float duration)
    {
        bool isLong = duration > songSettings.longNoteThreshold;
        bool isStrong = velocity > songSettings.strongVelocityThreshold;

        if (pid == 0 || pid == 2)
        {
            if (isLong) return PooType.Candy1_Long;
            return isStrong ? PooType.Candy1_Strong : PooType.Candy1_Normal;
        }
        else if (pid == 3 || pid == 5)
        {
            if (isLong) return PooType.Candy2_Long;
            return isStrong ? PooType.Candy2_Strong : PooType.Candy2_Normal;
        }

        return PooType.Lollipop_Long;
    }

    /// <summary>
    /// Finds and returns the target lane Transform directly from the unified lane configuration list.
    /// </summary>
    private Transform GetLaneTransform(int pid)
    {
        foreach (var config in laneConfigs)
        {
            if (config.pid == pid)
            {
                return config.laneTransform;
            }
        }

        if (laneConfigs.Count > 0) return laneConfigs[0].laneTransform;
        return null;
    }

    /// <summary>
    /// Triggers the game win state and stops the gameplay loop.
    /// </summary>
    public void TriggerWin()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isPlaying = false;

        if (songAudioSource != null && songAudioSource.isPlaying)
        {
            songAudioSource.Stop();
        }

        Debug.Log("[NoteSpawner] Game Win!");
        OnGameWin?.Invoke();
    }

    /// <summary>
    /// Triggers the game lose state and stops the gameplay loop.
    /// </summary>
    public void TriggerLose()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isPlaying = false;

        if (songAudioSource != null && songAudioSource.isPlaying)
        {
            songAudioSource.Stop();
        }

        Debug.Log("[NoteSpawner] Game Lose!");
        OnGameLose?.Invoke();
    }
}
