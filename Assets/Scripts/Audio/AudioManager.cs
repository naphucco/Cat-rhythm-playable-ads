using UnityEngine;
using System;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Background Music")]
    [SerializeField] private AudioSource songAudioSource;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip crySound;
    [SerializeField] private AudioClip mooewSound;
    [SerializeField] private AudioClip waterDropSound;

    public bool IsPlaying => songAudioSource != null && songAudioSource.isPlaying;
    public float CurrentAudioTime => songAudioSource != null ? songAudioSource.time : 0f;

    private IDisposable _subscription;

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller =>
            {
                controller.OnSongPlayRequested += PlaySong;
                controller.OnSongStopRequested += StopSong;
                controller.OnGameLose += PlayCrySound;
                controller.OnGameWin += PlayMooewSound;
                controller.OnNoteHitEvent += OnNoteHit;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    // === HANDLER ===
    private void OnNoteHit(int laneIndex, ObjectType candyType)
    {
        if (candyType == ObjectType.Lollipop_Long)
        {
            PlayWaterDrop();
        }
    }

    // === EXISTING METHODS ===
    public void PlaySong()
    {
        if (songAudioSource == null) return;
        songAudioSource.Play();
    }

    public void StopSong()
    {
        if (songAudioSource != null && songAudioSource.isPlaying)
        {
            songAudioSource.Stop();
        }
    }

    public void PlayCrySound()
    {
        if (sfxAudioSource != null && crySound != null)
        {
            sfxAudioSource.PlayOneShot(crySound);
        }
    }

    public void PlayMooewSound()
    {
        if (sfxAudioSource != null && mooewSound != null)
        {
            sfxAudioSource.PlayOneShot(mooewSound);
        }
    }

    public void PlayWaterDrop()
    {
        if (sfxAudioSource != null && waterDropSound != null)
        {
            sfxAudioSource.PlayOneShot(waterDropSound);
        }
    }
}