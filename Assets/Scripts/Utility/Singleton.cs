using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting = false;
    private static bool _isReloading = false;

    public static T Instance
    {
        get
        {
            if (_isQuitting || _isReloading)
                return null;

            if (_instance == null && Application.isPlaying)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    Debug.LogError($"[Singleton] {typeof(T).Name} not found in scene!");
                    return null;
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _isReloading = false;
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        RegisterForCleanup();
    }


    protected virtual void OnDestroy()
    {
        if (_instance == this as T)
        {
            _instance = null;
            _isReloading = true;
        }
    }

    private static void RegisterForCleanup()
    {
        if (_registered) return;
        _registered = true;

        Application.quitting += OnApplicationQuitting;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private static bool _registered = false;

    private static void OnApplicationQuitting()
    {
        _isQuitting = true;
        _instance = null;
    }

#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            _isQuitting = true;
            _instance = null;
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            _isQuitting = false;
        }
    }
#endif
}