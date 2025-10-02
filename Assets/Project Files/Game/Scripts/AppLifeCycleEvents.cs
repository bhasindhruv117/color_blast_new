using System;
using UnityEngine;

public class AppLifeCycleEvents : MonoBehaviour
{
    #region SINGLETON

    private static AppLifeCycleEvents _instance;
    public static AppLifeCycleEvents Instance => _instance;

    #endregion
    
    #region EVENTS

    public static event Action OnApplicationResumed;
    public static event Action OnApplicationPaused;
    public static event Action OnApplicationStarted;

    private static void RaiseOnApplicationResumed()
    {
        OnApplicationResumed?.Invoke();
    }

    private static void RaiseOnApplicationPaused()
    {
        OnApplicationPaused?.Invoke();
    }

    private static void RaiseOnApplicationStarted()
    {
        OnApplicationStarted?.Invoke();
    }

    #endregion
    
    private long _lastResumedTime;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(this);
            return;
        }
        _instance = this;
        OnApplicationStarted += ApplicationStarted;
        OnApplicationResumed += ApplicationResumed;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
#if UNITY_EDITOR
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     OnApplicationPause(true);
        // }
#endif
    }

    private void ApplicationResumed()
    {
        _lastResumedTime = Util.GetCurrentLocalTimeStamp();
    }

    private void ApplicationStarted()
    {
        _lastResumedTime = Util.GetCurrentLocalTimeStamp();
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            RaiseOnApplicationPaused();
        }
        else
        {
            if (_lastResumedTime > 0) {
                RaiseOnApplicationResumed();
            }
            else {
                RaiseOnApplicationStarted();
            }
        }
    }
}
