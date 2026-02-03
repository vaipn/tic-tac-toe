using UnityEngine;

public abstract class PersistentMonoSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting ;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[MonoBehaviourSingletonPersistent] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                { 
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }
     }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize is called on awake only if this is the first instance
    /// </summary>
    protected abstract void Initialize();

    private void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}