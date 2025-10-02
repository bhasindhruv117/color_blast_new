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
        Util.SetInstallTime();
        var localNotifications = LocalNotifications.Instance;
    }
}
