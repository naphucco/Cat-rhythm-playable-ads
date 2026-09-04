using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class responsible for parsing raw JSON chart files into sorted collections of note data using dynamic SongSettings mapping.
/// </summary>
public static class ChartLoader
{
    /// <summary>
    /// Parses a JSON TextAsset into a sorted list of NoteData structures with mapped sequential lane indices based on SongSettings.
    /// </summary>
    public static List<NoteData> LoadAndSortChart(TextAsset jsonChartFile, SongSettings settings)
    {
        if (jsonChartFile == null)
        {
            Debug.LogError("[ChartLoader] JSON chart file is missing! Please assign a valid TextAsset.");
            return null;
        }

        if (settings == null)
        {
            Debug.LogError("[ChartLoader] SongSettings asset is required for mapping chart PIDs.");
            return null;
        }

        string wrappedJson = "{\"notes\":" + jsonChartFile.text + "}";
        SongChart chart = JsonUtility.FromJson<SongChart>(wrappedJson);

        if (chart == null || chart.notes == null)
        {
            Debug.LogError("[ChartLoader] Failed to parse JSON chart structure.");
            return null;
        }

        List<NoteData> notes = chart.notes;
        
        // Map raw JSON PIDs to clean sequential lane indices and assign to LaneIndex property
        foreach (var note in notes)
        {
            note.LaneIndex = settings.GetLaneIndex(note.pid);
        }

        // Sort notes strictly by their arrival timing (ta) to guarantee accurate sequential processing
        notes.Sort((a, b) => a.ta.CompareTo(b.ta));
        
        Debug.Log($"[ChartLoader] Successfully loaded, mapped, and sorted {notes.Count} notes from chart.");
        return notes;
    }
}