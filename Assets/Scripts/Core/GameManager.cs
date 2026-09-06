using System;
using UnityEngine;

/// <summary>
/// Manages the overall game states (Tutorial, Playing, Win, PickNextSong, Lose) and coordinates with RhythmController and AudioManager.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Tutorial,
        Playing,
        Win,
        PickNextSong,
        Lose
    }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Tutorial;

    public GameState CurrentState => currentState;

    // Events for UI and external systems to listen to state changes
    public event Action OnTutorialStateEntered;
    public event Action OnPlayingStateEntered;
    public event Action OnWinStateEntered;
    public event Action OnPickNextSongStateEntered;
    public event Action OnLoseStateEntered;

    private void Start()
    {
        // Subscribe to RhythmController events for Miss, Win, and Lose conditions
        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnNoteMissEvent += HandleNoteMiss;
            RhythmController.Instance.OnGameWin += HandleGameWin;
            RhythmController.Instance.OnGameLose += HandleGameLose;
        }

        // Start the game in Tutorial state
        SetState(GameState.Tutorial);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.OnNoteMissEvent -= HandleNoteMiss;
            RhythmController.Instance.OnGameWin -= HandleGameWin;
            RhythmController.Instance.OnGameLose -= HandleGameLose;
        }
    }

    /// <summary>
    /// Public method called by UI (e.g., "Start Game" button) to dismiss tutorial and begin gameplay.
    /// </summary>
    public void StartPlaying()
    {
        if (currentState != GameState.Tutorial) return;

        SetState(GameState.Playing);

        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.StartGame();
        }
    }

    /// <summary>
    /// Public method called by UI button on the Win screen to proceed to song selection.
    /// </summary>
    public void ProceedToPickNextSong()
    {
        if (currentState != GameState.Win) return;
        SetState(GameState.PickNextSong);
    }

    private void HandleNoteMiss(int laneIndex)
    {
        if (currentState != GameState.Playing) return;

        if (RhythmController.Instance != null)
        {
            RhythmController.Instance.TriggerLose();
        }
    }

    private void HandleGameWin()
    {
        if (currentState != GameState.Playing) return;
        SetState(GameState.Win);
    }

    private void HandleGameLose()
    {
        if (currentState != GameState.Playing) return;
        SetState(GameState.Lose);
    }

    private void SetState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Tutorial:
                OnTutorialStateEntered?.Invoke();
                break;
            case GameState.Playing:
                OnPlayingStateEntered?.Invoke();
                break;
            case GameState.Win:
                OnWinStateEntered?.Invoke();
                break;
            case GameState.PickNextSong:
                OnPickNextSongStateEntered?.Invoke();
                break;
            case GameState.Lose:
                OnLoseStateEntered?.Invoke();
                break;
        }
    }
}