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
        var localNotifications = LocalNotifications.Instance;
    }
}
