using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;

public class EveningNotif1 : BaseLocalNotification
{
    private NotificationScheduleType _scheduleType;
    private int _index = -1;
    public EveningNotif1(int index = -1, NotificationScheduleType scheduleType = NotificationScheduleType.Continuous)
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
        "\ud83c\udf81 Claim Your Rewards Now!",
        "\ud83d\udd25 Your Challenge Awaits!",
        "\u23f3 Last Chance: Special Challenge!",
        "\ud83d\ude80 Let’s Break Some Records!",
        "\ud83c\udfae Your Evening Just Got Better!"
    };

    private static readonly List<string> NOTIFICATION_DESCRIPTION = new List<string>()
    {
        "You left something behind… Daily rewards are waiting! \ud83c\udfc6 ",
        "\ud83d\udd79\ufe0f Compete, score high, and win exciting boosters! \ud83c\udfc6",
        "Last few hours left to win exclusive rewards for the day! Don’t miss out—log in now! \ud83c\udfaf\ud83d\udd25",
        "\ud83c\udfc6 Play now & see if you can make it to the top! \ud83d\udd25",
        "Need a break? Relax, stack some blocks, and unwind with Color Blast! \ud83c\udf19\ud83e\udde0"
    };

    private const string NOTIFICATION_TYPE = "evening1";

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
        return NotificationsConstants.NOTIFICATION_CATEGORY_EVENING;
    }

    public override DateTime GetNotificationFireTime()
    {
        DateTime triggerTime = DateTime.Now.Date;
        if (_scheduleType == NotificationScheduleType.Continuous)
        {
            triggerTime = triggerTime.AddHours(16);
            triggerTime = triggerTime.AddMinutes(Util.GetRandomIntInRange(0, 60));
        }
        else
        {
            triggerTime = triggerTime.AddDays(1+_index);
            triggerTime = triggerTime.AddHours(16);
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
