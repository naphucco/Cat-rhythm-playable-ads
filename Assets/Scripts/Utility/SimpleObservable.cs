using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// 1. COROUTINE RUNNER - Scene-bound, non-persistent
// ============================================================

/// <summary>
/// Provides a MonoBehaviour for running coroutines.
/// Created on demand, destroyed when scene unloads.
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    private static bool _isQuitting = false;

    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null && !_isQuitting)
            {
                GameObject go = new GameObject("CoroutineRunner");
                _instance = go.AddComponent<CoroutineRunner>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}

// ============================================================
// 2. OBSERVABLE INSTANCE - Emits when singleton is ready
// ============================================================

/// <summary>
/// Observable that emits a single value when an instance of T becomes available.
/// </summary>
public class ObservableInstance<T> where T : class
{
    private readonly Func<T> _instanceGetter;

    public ObservableInstance(Func<T> instanceGetter)
    {
        _instanceGetter = instanceGetter;
    }

    public IDisposable Subscribe(MonoBehaviour runner, Action<T> onNext)
    {
        var handler = new InstanceHandler(onNext, _instanceGetter, runner);
        return handler;
    }

    private class InstanceHandler : IDisposable
    {
        private Action<T> _callback;
        private Func<T> _getter;
        private Coroutine _coroutine;
        private CoroutineRunner _runner; // Cached reference to prevent re-creation during cleanup
        private bool _disposed;

        public InstanceHandler(Action<T> callback, Func<T> getter, MonoBehaviour runner)
        {
            _callback = callback;
            _getter = getter;
            _runner = CoroutineRunner.Instance;
            _coroutine = _runner.StartCoroutine(WaitForInstance());
        }

        private IEnumerator WaitForInstance()
        {
            while (!_disposed)
            {
                T instance = _getter();
                if (instance != null)
                {
                    _callback?.Invoke(instance);
                    _callback = null;
                    _getter = null;
                    yield break;
                }
                yield return null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Use cached _runner, DO NOT call CoroutineRunner.Instance during cleanup
            if (_coroutine != null && _runner != null)
            {
                _runner.StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _callback = null;
            _getter = null;
            _runner = null;
        }
    }
}

// ============================================================
// 3. EXTENSION METHODS - Fluent API
// ============================================================

/// <summary>
/// Extension methods providing a UniRx-like fluent syntax.
/// </summary>
public static class ObservableExtensions
{
    /// <summary>
    /// Creates an observable that waits for a singleton instance to become available.
    /// Usage: this.WhenReady(() => RhythmController.Instance).Subscribe(...)
    /// </summary>
    public static ObservableInstance<T> WhenReady<T>(this MonoBehaviour mono, Func<T> instanceGetter) where T : class
    {
        return new ObservableInstance<T>(instanceGetter);
    }
}

// ============================================================
// 4. DISPOSABLE CONTAINER - Auto-cleanup on Destroy
// ============================================================

/// <summary>
/// Holds a list of IDisposables and automatically disposes them
/// when the GameObject is destroyed.
/// </summary>
public class DisposableContainer : MonoBehaviour
{
    private List<IDisposable> _disposables = new List<IDisposable>();

    public void Add(IDisposable disposable)
    {
        if (disposable == null) return;
        _disposables.Add(disposable);
    }

    private void OnDestroy()
    {
        foreach (var d in _disposables)
        {
            try { d.Dispose(); }
            catch { /* Ignore disposal errors */ }
        }
        _disposables.Clear();
    }
}

/// <summary>
/// Extension methods for automatic subscription cleanup.
/// </summary>
public static class DisposableExtensions
{
    /// <summary>
    /// Automatically disposes the subscription when the MonoBehaviour is destroyed.
    /// Usage: subscription.AddTo(this);
    /// </summary>
    public static IDisposable AddTo(this IDisposable disposable, MonoBehaviour mono)
    {
        if (mono == null) return disposable;

        var container = mono.gameObject.GetComponent<DisposableContainer>();
        if (container == null)
            container = mono.gameObject.AddComponent<DisposableContainer>();

        container.Add(disposable);
        return disposable;
    }
}