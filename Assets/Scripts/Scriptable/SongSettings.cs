using UnityEngine;

/// <summary>
/// ScriptableObject containing global song configuration parameters,
/// such as note classification thresholds and advance timing.
/// </summary>
[CreateAssetMenu(fileName = "SongSettings", menuName = "RhythmGame/Song Settings")]
public class SongSettings : ScriptableObject
{
    [Header("Spawn Timing")]
    [Tooltip("Time offset in seconds before a note's arrival time ('ta') to spawn the candy so it travels to the hit line.")]
    public float spawnInAdvanceTime = 2.0f;

    [Header("Note Classification Thresholds")]
    [Tooltip("Minimum duration in seconds required to classify a note as a Long type candy.")]
    public float longNoteThreshold = 0.2f;

    [Tooltip("Minimum velocity value required to classify a note as a Strong type candy.")]
    public int strongVelocityThreshold = 110;
}