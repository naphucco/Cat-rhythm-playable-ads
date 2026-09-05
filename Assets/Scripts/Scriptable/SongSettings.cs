using UnityEngine;

[CreateAssetMenu(fileName = "SongSettings", menuName = "RhythmGame/Song Settings")]
public class SongSettings : ScriptableObject
{
    [Header("Spawn Timing")]
    [Tooltip("Time offset in seconds before a note's arrival time ('ta') to spawn the candy so it travels to the hit line.")]
    public float noteTravelTime = 1.0f;

    [Header("Note Classification Thresholds")]
    [Tooltip("Minimum duration in seconds required to classify a note as a Long type candy.")]
    public float longNoteThreshold = 0.2f;

    [Tooltip("Minimum velocity value required to classify a note as a Strong type candy.")]
    public int strongVelocityThreshold = 110;

    [Header("Lane PID Mappings")]
    [Tooltip("Configure how sparse MIDI PIDs map to sequential lane indices for this song.")]
    public PidMapping[] pidMappings;

    /// <summary>
    /// Helper method to convert a JSON PID to a sequential lane index based on asset configuration
    /// </summary>
    public int GetLaneIndex(int jsonPid)
    {
        if (pidMappings != null)
        {
            foreach (var map in pidMappings)
            {
                if (map.jsonPid == jsonPid)
                {
                    return map.laneIndex;
                }
            }
        }

        Debug.LogWarning($"[SongSettings] Unmapped JSON PID '{jsonPid}' found. Defaulting to lane index 0.");
        return 0;
    }
}