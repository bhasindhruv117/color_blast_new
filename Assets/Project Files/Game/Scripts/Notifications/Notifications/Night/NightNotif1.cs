using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;
using UnityEngine;

public class NightNotif1 : BaseLocalNotification
{
    private NotificationScheduleType _scheduleType;
    private int _index = -1;
    public NightNotif1(int index = -1, NotificationScheduleType scheduleType = NotificationScheduleType.Continuous)
    {
        _scheduleType = scheduleType;
        _index = index;
        if (_index == -1)
        {
            _index = Util.GetRandomIntInRange(0, NOTIFICATION_TITLE.Count);
        }
    }
    
    #region CONSTANTS

    private static readonly List<string> NOTIFICATION_TITLE = new List<string>()
    {
        "\ud83d\udca4 One Last Game Before Bed?",
        "\ud83c\udfc6 Your High Score Awaits!",
        "\ud83e\udde0 Brain Training Before Sleep!",
        "\ud83d\ude80 Still Time to Make a Move!",
        "\ud83d\udd25 Don’t Let the Streak Die!"
    };

    private static readonly List<string> NOTIFICATION_DESCRIPTION = new List<string>()
    {
        "Keep your brain sharp! \ud83e\udde0 A quick game before bed boosts focus & strategy! \ud83c\udfaf\ud83d\udd25",
        "The day’s almost over, but there’s still time to break your record! \ud83d\ude80\ud83d\udcaf",
        "Studies say puzzle games help you relax & improve memory! Ready to play? \ud83c\udf1f",
        "Not enough gaming today? \ud83c\udfd7\ufe0f Stack some blocks & end your night with a winning move! \ud83c\udfaf\ud83d\udd25",
        "You’re on a hot streak! One more game before bed & keep the momentum going! \ud83d\udcaa"
    };

    private const string NOTIFICATION_TYPE = "night1";

    #endregion
    public override string GetNotificationExtraData()
    {
        JObject jObject = new JObject();
        jObject.Add(new JProperty(NotificationsConstants.NOTIFICATION_TYPE_IDENTIFIER, NOTIFICATION_TYPE));
        return jObject.ToString();
    }

    public override string GetNotificationTitle()
    {
        return NOTIFICATION_TITLE[_index];
    }

    public override string GetNotificationText()
    {
        return NOTIFICATION_DESCRIPTION[_index];
    }

    public override string GetNotificationCategory()
    {
        return NotificationsConstants.NOTIFICATION_CATEGORY_NIGHT;
    }

    public override DateTime GetNotificationFireTime()
    {
        DateTime triggerTime = DateTime.Now.Date;
        if (_scheduleType == NotificationScheduleType.Continuous)
        {
            triggerTime = triggerTime.AddHours(20);
            triggerTime = triggerTime.AddMinutes(Util.GetRandomIntInRange(0, 60));
        }
        else
        {
            triggerTime = triggerTime.AddDays(1+_index);
            triggerTime = triggerTime.AddHours(20);
            triggerTime = triggerTime.AddMinutes(Util.GetRandomIntInRange(0, 60));
        }
        return triggerTime;
    }

    public override TimeSpan? GetNotificationRepeatInterval()
    {
        if (_scheduleType == NotificationScheduleType.Lapser)
        {
            return new TimeSpan(5, 0, 0,0);
        }
        return base.GetNotificationRepeatInterval();
    }
}
