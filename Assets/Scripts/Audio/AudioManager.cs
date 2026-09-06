using UnityEngine;
using System;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Background Music")]
    [SerializeField] private AudioSource songAudioSource;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip crySound;

    public bool IsPlaying => songAudioSource != null && songAudioSource.isPlaying;
    public float CurrentAudioTime => songAudioSource != null ? songAudioSource.time : 0f;

    private IDisposable _subscription;

    private void OnEnable()
    {
        // Safe subscription: waits for RhythmController to be ready,
        // auto-disposes when this GameObject is destroyed.
        _subscription = this.WhenReady(() => RhythmController.Instance)
            .Subscribe(this, controller =>
            {
                controller.OnSongPlayRequested += PlaySong;
                controller.OnSongStopRequested += StopSong;
                controller.OnGameLose += PlayCrySound;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
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

    public void PlayCrySound()
    {
        if (sfxAudioSource != null && crySound != null)
        {
            sfxAudioSource.PlayOneShot(crySound);
        }
    }
}