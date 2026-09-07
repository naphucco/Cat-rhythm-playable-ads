using UnityEngine;
using System;

public class CatManager : Singleton<CatManager>
{
    [Header("References")]
    [SerializeField] private CatMoveController[] cats;

    private IDisposable _subscription;

    private void Start()
    {
        // Auto-find cats in children if not assigned in Inspector
        if (cats == null || cats.Length == 0)
        {
            cats = GetComponentsInChildren<CatMoveController>(true);
        }
    }

    private void OnEnable()
    {
        _subscription = this.WhenReady(() => GameManager.Instance)
            .Subscribe(this, mgr =>
            {
                mgr.OnLoseStateEntered += HandleLoseState;
                mgr.OnPickNextSongStateEntered += HandlePickNextSongState;
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
    }

    private void HandleLoseState()
    {
        // Disable movement input when the game is lost
        foreach (var cat in cats)
        {
            if (cat != null)
            {
                cat.enabled = false;
            }
        }
    }

    private void HandlePickNextSongState()
    {
        // Deactivate cat game objects entirely when moving to song selection
        foreach (var cat in cats)
        {
            if (cat != null)
            {
                cat.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Checks if any active cat is currently standing on the specified global lane index.
    /// </summary>
    public bool IsLaneCaught(int laneIndex)
    {
        foreach (var cat in cats)
        {
            if (cat != null && cat.CurrentGlobalLaneIndex == laneIndex)
            {
                return true;
            }
        }
        return false;
    }
}