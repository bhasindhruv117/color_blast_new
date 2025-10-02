using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;

public class AfternoonNotif1 : BaseLocalNotification
{
   private NotificationScheduleType _scheduleType;
    private int _index = -1;
    public AfternoonNotif1(int index = -1, NotificationScheduleType scheduleType = NotificationScheduleType.Continuous)
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
        "\u23f3 How’s Your Streak Going?",
        "\ud83c\udfc6 Take a Break, Make a Move!",
        "\ud83d\ude80 Your Best Score Is Near!",
        "\ud83c\udfaf You’re Just One Step Away!",
        "\ud83d\udd79\ufe0f Your Board Misses You!"
    };

    private static readonly List<string> NOTIFICATION_DESCRIPTION = new List<string>()
    {
        "80% of players hit a new high score today! \ud83d\udd25 Join them & keep your streak alive! \ud83d\ude80",
        "Quick 5-min break? Refresh your mind & stack some blocks! \u23f3\ud83d\udca1",
        "Players like you boost their IQ by playing Color Blast in the afternoon!\ud83e\udde0",
        "9/10 players improve their block skills in the afternoon! \ud83c\udfd7\ufe0f\ud83d\udcaa\ud83d\udd25",
        "\ud83d\udd25Jump back in & stack up to keep that progress rolling! \ud83d\ude80"
    };

    private const string NOTIFICATION_TYPE = "afternoon1";

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
        return NotificationsConstants.NOTIFICATION_CATEGORY_MORNING;
    }

    public override DateTime GetNotificationFireTime()
    {
        DateTime triggerTime = DateTime.Now.Date;
        if (_scheduleType == NotificationScheduleType.Continuous)
        {
            triggerTime = triggerTime.AddHours(12);
            triggerTime = triggerTime.AddMinutes(Util.GetRandomIntInRange(0, 60));
        }
        else
        {
            triggerTime = triggerTime.AddDays(1+_index);
            triggerTime = triggerTime.AddHours(12);
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
