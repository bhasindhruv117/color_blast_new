using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;

public class MorningNotif1 : BaseLocalNotification
{
    private NotificationScheduleType _scheduleType;
    private int _index = -1;
    public MorningNotif1(int index = -1, NotificationScheduleType scheduleType = NotificationScheduleType.Continuous)
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
        "\u2600\ufe0f Good Morning, Block Master!",
        "\ud83d\ude80 Jumpstart Your Day!",
        "\ud83c\udf08 Ready to Build Something Big?",
        "\ud83e\udde0 Morning Boost Unlocked!",
        "\ud83c\udfae Your Blocks Are Calling!"
    };

    private static readonly List<string> NOTIFICATION_DESCRIPTION = new List<string>()
    {
        "A new day, a new challenge! \ud83c\udf1f\ud83c\udfaf",
        "The best way to wake up? A quick game of Color Blast! \u2615\ud83d\udca1",
        "A little brain workout in the morning goes a long way! \ud83c\udfd7\ufe0f",
        "Sharpen your focus with a quick block-stacking session! \ud83c\udfc6 Let’s make today a high-score day! \ud83d\udd25",
        "Let’s get stacking and start your morning with a winning move! \ud83c\udfae\ud83d\udd25"
    };

    private const string NOTIFICATION_TYPE = "morning1";

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
            triggerTime = triggerTime.AddHours(7);
            triggerTime = triggerTime.AddMinutes(Util.GetRandomIntInRange(0, 60));
        }
        else
        {
            triggerTime = triggerTime.AddDays(1+_index);
            triggerTime = triggerTime.AddHours(7);
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
