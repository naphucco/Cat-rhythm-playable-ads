using UnityEngine;

/// <summary>
/// Manages background music playback and listens to NoteSpawner events to handle audio triggers.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source Configuration")]
    [Tooltip("AudioSource component responsible for playing the background music track.")]
    [SerializeField] private AudioSource songAudioSource;

    public bool IsPlaying => songAudioSource != null && songAudioSource.isPlaying;
    public float CurrentAudioTime => songAudioSource != null ? songAudioSource.time : 0f;

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
    }

    private void OnEnable()
    {
        // Subscribe to NoteSpawner events safely
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnSongPlayRequested += PlaySong;
            RhythmController.Instance.OnSongStopRequested += StopSong;
        }
    }

    private void Start()
    {
        // Fallback subscription if NoteSpawner was initialized slightly after OnEnable
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnSongPlayRequested -= PlaySong; // Prevent duplicate
            RhythmController.Instance.OnSongPlayRequested += PlaySong;
            RhythmController.Instance.OnSongStopRequested -= StopSong;
            RhythmController.Instance.OnSongStopRequested += StopSong;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnSongPlayRequested -= PlaySong;
            RhythmController.Instance.OnSongStopRequested -= StopSong;
        }
    }

    public void PlaySong()
    {
        if (songAudioSource != null && !songAudioSource.isPlaying)
        {
            songAudioSource.Play();
        }
    }

    public void StopSong()
    {
        if (songAudioSource != null && songAudioSource.isPlaying)
        {
            songAudioSource.Stop();
        }
    }
}