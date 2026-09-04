using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the data structure for an individual note/candy parsed from the MIDI JSON chart.
/// </summary>
[Serializable]
public class NoteData
{
    [Tooltip("Unique identifier for the note in the entire song.")]
    public int id;

    [Tooltip("Original MIDI note number (auxiliary data).")]
    public int n;

    [Tooltip("Absolute arrival time in seconds from song start when the note reaches the hit line.")]
    public float ta;

    [Tooltip("Delta time relative to the previous note in the global sequence.")]
    public float ts;

    [Tooltip("Duration of the note (legacy MIDI attribute).")]
    public float d;

    [Tooltip("Velocity value used to determine the candy variant (e.g., 50 = Normal, 127 = Strong).")]
    public int v;

    [Tooltip("Position ID / Lane ID determining the lane and cat assignment (e.g., 0, 2, 3, 5).")]
    public int pid;

    // Clean property representing the resolved lane index for game logic
    public int LaneIndex { get; set; }
}

/// <summary>
/// Wrapper class used to satisfy Unity's JsonUtility requirement for parsing top-level JSON arrays.
/// </summary>
[Serializable]
public class SongChart
{
    [Tooltip("List containing all parsed note data items for the track.")]
    public List<NoteData> notes;
}