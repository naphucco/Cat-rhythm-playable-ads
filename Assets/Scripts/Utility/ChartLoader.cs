using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class responsible for parsing raw JSON chart files into sorted collections of note data.
/// </summary>
public static class ChartLoader
{
    /// <summary>
    /// Parses a JSON TextAsset into a sorted list of NoteData structures ordered chronologically by arrival time (ta).
    /// </summary>
    public static List<NoteData> LoadAndSortChart(TextAsset jsonChartFile)
    {
        if (jsonChartFile == null)
        {
            Debug.LogError("[ChartLoader] JSON chart file is missing! Please assign a valid TextAsset.");
            return null;
        }

        // Wrap the raw JSON array string into an object container compatible with Unity's JsonUtility
        string wrappedJson = "{\"notes\":" + jsonChartFile.text + "}";
        SongChart chart = JsonUtility.FromJson<SongChart>(wrappedJson);

        if (chart == null || chart.notes == null)
        {
            Debug.LogError("[ChartLoader] Failed to parse JSON chart structure.");
            return null;
        }

        List<NoteData> notes = chart.notes;
        
        // Sort notes strictly by their arrival timing (ta) to guarantee accurate sequential processing
        notes.Sort((a, b) => a.ta.CompareTo(b.ta));
        
        Debug.Log($"[ChartLoader] Successfully loaded and sorted {notes.Count} notes from chart.");
        return notes;
    }
}