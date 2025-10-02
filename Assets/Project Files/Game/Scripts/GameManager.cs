using System;
using UnityEngine;
using Watermelon;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.unityLogger.logEnabled = false;
#if DEVELOPMENT_BUILD
        Debug.unityLogger.logEnabled = true;
#endif
        Util.SetInstallTime();
        var localNotifications = LocalNotifications.Instance;
    }
}
