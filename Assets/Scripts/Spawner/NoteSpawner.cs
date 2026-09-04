using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns candies based on MIDI-exported JSON data. 
/// Synchronizes note arrivals with song playback time using 'ta' (Timing Arrival).
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Data & Audio Source")]
    [Tooltip("JSON TextAsset containing the song chart.")]
    [SerializeField] private TextAsset jsonChartFile;
    [Tooltip("AudioSource component that plays the song.")]
    [SerializeField] private AudioSource songAudioSource;

    [Header("Spawn Settings")]
    [Tooltip("Time offset in seconds before 'ta' to spawn the candy so it travels to the hit line.")]
    [SerializeField] private float spawnInAdvanceTime = 2.0f;
    
    [Tooltip("Y coordinate of the judgment/hit line where candies should arrive.")]
    [SerializeField] private float hitLineY = -3.5f;

    [Tooltip("Transform positions or X coordinates corresponding to each lane (pid 0, 2, 3, 5).")]
    [SerializeField] private Transform[] laneTransforms;

    private List<NoteData> allNotes = new List<NoteData>();
    private int currentIndex = 0;
    private float songTimer = 0f;
    private bool isPlaying = false;

    private void Start()
    {
        ParseJSON();
        StartCoroutine(CheckPoolAndStart());
    }

    /// <summary>
    /// Parses the JSON chart file into a list of notes.
    /// </summary>
    private void ParseJSON()
    {
        if (jsonChartFile == null)
        {
            Debug.LogError("[NoteSpawner] JSON chart file is missing!");
            return;
        }

        string wrappedJson = "{\"notes\":" + jsonChartFile.text + "}";
        SongChart chart = JsonUtility.FromJson<SongChart>(wrappedJson);
        
        if (chart != null && chart.notes != null)
        {
            allNotes = chart.notes;
            allNotes.Sort((a, b) => a.ta.CompareTo(b.ta));
            Debug.Log($"[NoteSpawner] Successfully loaded {allNotes.Count} notes.");
        }
    }

    /// <summary>
    /// Waits until the MultiCandyPooler is fully initialized, then starts audio and gameplay.
    /// </summary>
    private IEnumerator CheckPoolAndStart()
    {
        while (MultiCandyPooler.Instance == null || !MultiCandyPooler.Instance.IsInitialized)
        {
            yield return null;
        }

        if (songAudioSource != null)
        {
            songAudioSource.Play();
        }

        songTimer = 0f;
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying || allNotes == null) return;

        // Sync songTimer directly with AudioSource playback time for absolute precision
        if (songAudioSource != null && songAudioSource.isPlaying)
        {
            songTimer = songAudioSource.time;
        }
        else
        {
            songTimer += Time.deltaTime;
        }

        while (currentIndex < allNotes.Count && allNotes[currentIndex].ta - songTimer <= spawnInAdvanceTime)
        {
            SpawnNote(allNotes[currentIndex]);
            currentIndex++;
        }
    }

    /// <summary>
    /// Determines candy type based on pid, velocity (v), and duration (d), then requests it from the pool.
    /// </summary>
    private void SpawnNote(NoteData note)
    {
        PooType candyID = ResolveCandyID(note.pid, note.v, note.d);
        int laneIndex = GetLaneIndex(note.pid);

        if (laneIndex < 0 || laneIndex >= laneTransforms.Length)
        {
            Debug.LogWarning($"[NoteSpawner] Invalid lane index for pid: {note.pid}");
            return;
        }

        Vector3 spawnPosition = laneTransforms[laneIndex].position;

        // Request candy from the multi-pooler using PooType enum
        GameObject candy = MultiCandyPooler.Instance.GetCandy(candyID, spawnPosition, Quaternion.identity);
        
        if (candy != null)
        {
            var mover = candy.GetComponent<CandyMover>() ?? candy.AddComponent<CandyMover>();
            mover.Initialize(note.ta, songTimer, spawnPosition, hitLineY);
        }
    }

    /// <summary>
    /// Maps PID, Velocity (v), and Duration (d) to explicit PooType enums:
    /// 0: Candy1_Long, 1: Candy1_Normal, 2: Candy1_Strong
    /// 3: Candy2_Long, 4: Candy2_Normal, 5: Candy2_Strong
    /// 6: Lollipop_Long
    /// </summary>
    private PooType ResolveCandyID(int pid, int velocity, float duration)
    {
        bool isLong = duration > 0.1f;
        bool isStrong = velocity >= 100;

        // Left Cat Group (pid 0, 2) -> Candy Series 1
        if (pid == 0 || pid == 2)
        {
            if (isLong) return PooType.Candy1_Long;       
            return isStrong ? PooType.Candy1_Strong : PooType.Candy1_Normal;    
        }
        // Right Cat Group (pid 3, 5) -> Candy Series 2
        else if (pid == 3 || pid == 5)
        {
            if (isLong) return PooType.Candy2_Long;       
            return isStrong ? PooType.Candy2_Strong : PooType.Candy2_Normal;    
        }

        // Special or fallback type (Lollipop)
        return PooType.Lollipop_Long; 
    }

    /// <summary>
    /// Maps JSON pid values (0, 2, 3, 5) to local array indices.
    /// </summary>
    private int GetLaneIndex(int pid)
    {
        switch (pid)
        {
            case 0: return 0;
            case 2: return 1;
            case 3: return 2;
            case 5: return 3;
            default: return 0;
        }
    }
}