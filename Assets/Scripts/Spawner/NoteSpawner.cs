using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the complete lifecycle of note data parsing, synchronization, and candy spawning for the rhythm game.
/// Synchronizes note arrival timings with song playback using a drift-corrected timer and prevents burst lags.
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    // Singleton instance for global access by game elements like CandyMover
    public static NoteSpawner Instance;

    [Header("Data & Audio Source Configuration")]
    [Tooltip("The JSON TextAsset containing the exported MIDI song chart data.")]
    [SerializeField] private TextAsset jsonChartFile;
    
    [Tooltip("The AudioSource component responsible for playing the background track.")]
    [SerializeField] private AudioSource songAudioSource;

    [Header("Spawn Timing & Positioning Settings")]
    [Tooltip("Time offset in seconds before a note's arrival time ('ta') to spawn the candy, ensuring constant travel speed to the hit line.")]
    [SerializeField] private float spawnInAdvanceTime = 2.0f;

    [Tooltip("The target Y coordinate representing the judgment/hit line where candies must arrive.")]
    [SerializeField] private float hitLineY = -3.5f;

    [Header("Note Classification Thresholds")]
    [Tooltip("Minimum duration in seconds required to classify a note as a Long type candy.")]
    [SerializeField] private float longNoteThreshold = 0.2f;
    
    [Tooltip("Minimum velocity value required to classify a note as a Strong type candy.")]
    [SerializeField] private int strongVelocityThreshold = 110;

    [Header("Unified Lane Configuration")]
    [Tooltip("Direct mapping configuration linking MIDI PID values to their corresponding world transforms.")]
    [SerializeField] private List<LaneConfig> laneConfigs = new List<LaneConfig>
    {
        new LaneConfig { pid = 0 },
        new LaneConfig { pid = 2 },
        new LaneConfig { pid = 3 },
        new LaneConfig { pid = 5 }
    };

    // Internal list storing all parsed and sorted notes from the chart
    private List<NoteData> allNotes = new List<NoteData>();
    
    // Tracks the current index of the note being evaluated for spawning
    private int currentIndex = 0;
    
    // Master synchronized timer representing the song's current playback position
    private float songTimer = 0f;
    
    // Flag indicating whether the gameplay session and audio have officially started
    private bool isPlaying = false;

    /// <summary>
    /// Exposes the synchronized song timer publicly so external systems (such as CandyMover) can query exact timing.
    /// </summary>
    public float SongTimer => songTimer;

    /// <summary>
    /// Enforces singleton pattern and applies performance configurations (FPS caps) upon initialization.
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

        // Lock frame rate to 60 FPS to stabilize delta time and eliminate erratic performance spikes on WebGL/desktop
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    /// <summary>
    /// Initiates chart parsing and starts the pool verification coroutine.
    /// </summary>
    private void Start()
    {
        ParseJSON();
        StartCoroutine(CheckPoolAndStart());
    }

    /// <summary>
    /// Parses the raw JSON chart file into a sorted collection of note structures.
    /// Sorts all notes strictly by their arrival timing (ta) to ensure sequential processing.
    /// </summary>
    private void ParseJSON()
    {
        if (jsonChartFile == null)
        {
            Debug.LogError("[NoteSpawner] JSON chart file is missing! Please assign a valid chart asset.");
            return;
        }

        // Wrap the raw JSON array string into an object container compatible with Unity's JsonUtility
        string wrappedJson = "{\"notes\":" + jsonChartFile.text + "}";
        SongChart chart = JsonUtility.FromJson<SongChart>(wrappedJson);

        if (chart != null && chart.notes != null)
        {
            allNotes = chart.notes;
            // Sort notes chronologically by arrival time (ta) to guarantee accurate sequential lookup
            allNotes.Sort((a, b) => a.ta.CompareTo(b.ta));
            Debug.Log($"[NoteSpawner] Successfully loaded and sorted {allNotes.Count} notes.");
        }
    }

    /// <summary>
    /// Coroutine that waits for the MultiCandyPooler to initialize completely, 
    /// then sets up the negative lead-in time buffer to prevent burst-spawn lag spikes at startup.
    /// </summary>
    private IEnumerator CheckPoolAndStart()
    {
        // Yield execution frame-by-frame until the object pooler is fully ready
        while (MultiCandyPooler.Instance == null || !MultiCandyPooler.Instance.IsInitialized)
        {
            yield return null;
        }

        // Initialize songTimer with a negative offset equal to spawnInAdvanceTime.
        // This provides a smooth intro buffer, preventing all early notes from spawning simultaneously on frame 0.
        songTimer = -spawnInAdvanceTime;
        isPlaying = true;
    }

    /// <summary>
    /// Main execution loop running every frame. 
    /// Handles time progression, audio triggering, drift correction, and note look-ahead evaluation.
    /// </summary>
    private void Update()
    {
        if (!isPlaying || allNotes == null) return;

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
        while (currentIndex < allNotes.Count && allNotes[currentIndex].ta - songTimer <= spawnInAdvanceTime)
        {
            SpawnNote(allNotes[currentIndex]);
            currentIndex++;
        }
    }

    /// <summary>
    /// Resolves the correct candy type and lane transform for a given note, 
    /// retrieves the object from the pool, and initializes its movement parameters.
    /// </summary>
    private void SpawnNote(NoteData note)
    {
        // Determine the specific PooType enum based on PID, velocity, and duration attributes
        PooType candyID = ResolveCandyID(note.pid, note.v, note.d);
        
        // Fetch the corresponding world Transform for the note's lane
        Transform laneTransform = GetLaneTransform(note.pid);

        if (laneTransform == null)
        {
            Debug.LogWarning($"[NoteSpawner] Invalid or missing lane transform configuration for PID: {note.pid}");
            return;
        }

        Vector3 spawnPosition = laneTransform.position;

        // Request an instance of the candy from the centralized multi-pooler
        GameObject candy = MultiCandyPooler.Instance.GetCandy(candyID, spawnPosition, Quaternion.identity);

        if (candy != null)
        {
            var mover = candy.GetComponent<CandyMover>();
            if (mover != null)
            {
                // Initialize movement script with timing data and travel specifications
                mover.Initialize(note.ta, songTimer, spawnPosition, hitLineY, spawnInAdvanceTime);
            }
        }
    }

    /// <summary>
    /// Maps a note's PID, velocity, and duration to an explicit PooType enum 
    /// utilizing configurable Inspector thresholds instead of hardcoded values.
    /// </summary>
    private PooType ResolveCandyID(int pid, int velocity, float duration)
    {
        bool isLong = duration > longNoteThreshold;
        bool isStrong = velocity > strongVelocityThreshold;

        // Left Cat Group (PID 0, 2) mapped to Candy Series 1 variants
        if (pid == 0 || pid == 2)
        {
            if (isLong) return PooType.Candy1_Long;       
            return isStrong ? PooType.Candy1_Strong : PooType.Candy1_Normal;    
        }
        // Right Cat Group (PID 3, 5) mapped to Candy Series 2 variants
        else if (pid == 3 || pid == 5)
        {
            if (isLong) return PooType.Candy2_Long;       
            return isStrong ? PooType.Candy2_Strong : PooType.Candy2_Normal;    
        }

        // Fallback default type for any unhandled or special patterns
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

        // Fallback safety measure: return the first available lane transform if mapping fails
        if (laneConfigs.Count > 0) return laneConfigs[0].laneTransform;
        return null;
    }
}

/// <summary>
/// Serializable data structure uniting a MIDI PID value directly with its physical world Transform,
/// simplifying Inspector management and avoiding index mismatches.
/// </summary>
[System.Serializable]
public struct LaneConfig
{
    [Tooltip("The track/lane PID value exported from the MIDI chart.")]
    public int pid;
    
    [Tooltip("The corresponding world Transform reference for this lane.")]
    public Transform laneTransform;
}