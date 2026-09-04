using UnityEngine;

/// <summary>
/// Serializable data structure uniting a MIDI PID value directly with its physical scene world Transform,
/// simplifying inspector management and avoiding index mismatch bugs.
/// </summary>
[System.Serializable]
public struct LaneConfig
{
    [Tooltip("The track/lane PID value exported from the MIDI chart.")]
    public int pid;
    
    [Tooltip("The corresponding scene world Transform reference for this lane.")]
    public Transform laneTransform;
}