using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the rhythm game's core execution loop, using decoupled C# events for audio playback triggers.
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
            Debug.LogError("[NoteSpawner] SongSettings asset is not assigned in the inspector!");
            return;
        }

        allNotes = ChartLoader.LoadAndSortChart(jsonChartFile);

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

        songTimer = -songSettings.spawnInAdvanceTime;
        currentIndex = 0;
        isPlaying = true;
        isGameEnded = false;
    }

    private void Update()
    {
        if (!isPlaying || isGameEnded || allNotes == null || songSettings == null) return;

        float previousTimer = songTimer;
        songTimer += Time.deltaTime;

        // Trigger audio playback request via event when intro countdown reaches zero
        if (previousTimer < 0f && songTimer >= 0f)
        {
            OnSongPlayRequested?.Invoke();
        }

        // Look-ahead spawning window
        while (currentIndex < allNotes.Count && allNotes[currentIndex].ta - songTimer <= songSettings.spawnInAdvanceTime)
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
        PooType candyID = ResolveCandyID(note.pid, note.v, note.d);
        Transform laneTransform = GetLaneTransform(note.pid);

        if (laneTransform == null)
        {
            Debug.LogWarning($"[NoteSpawner] Invalid or missing lane transform configuration for PID: {note.pid}");
            return;
        }

        Vector3 spawnPosition = laneTransform.position;
        GameObject candy = Pooler.Instance.GetCandy(candyID, spawnPosition, Quaternion.identity);

        if (candy != null)
        {
            var mover = candy.GetComponent<CandyMover>();
            if (mover != null)
            {
                mover.Initialize(note.ta, songTimer, spawnPosition, hitLineY, songSettings.spawnInAdvanceTime);
            }
        }
    }

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

    private Transform GetLaneTransform(int pid)
    {
        foreach (var config in laneConfigs)
        {
            if (config.pid == pid) return config.laneTransform;
        }

        return laneConfigs.Count > 0 ? laneConfigs[0].laneTransform : null;
    }

    public void TriggerWin()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isPlaying = false;

        OnSongStopRequested?.Invoke();
        OnGameWin?.Invoke();
    }

    public void TriggerLose()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        isPlaying = false;

        OnSongStopRequested?.Invoke();
        OnGameLose?.Invoke();
    }
}