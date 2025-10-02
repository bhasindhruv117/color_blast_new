using System;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;
using Unity.Notifications;

public class D0Notif2 : BaseLocalNotification
{
    #region CONSTANTS

    private const string NOTIFICATION_TITLE = "\ud83c\udfc6 9/10 Players Go Beyond This!";
    private const string NOTIFICATION_DESCRIPTION = "\ud83d\ude80 Place 10 blocks & level up your skills! \ud83c\udfd7\ufe0f";
    private const string NOTIFICATION_TYPE = "d0notif2";

    #endregion
    public override string GetNotificationExtraData()
    {
        JObject jObject = new JObject();
        jObject.Add(new JProperty(NotificationsConstants.NOTIFICATION_TYPE_IDENTIFIER, NOTIFICATION_TYPE));
        return jObject.ToString();
    }

    public override string GetNotificationTitle()
    {
        return NOTIFICATION_TITLE;
    }

    public override string GetNotificationText()
    {
        return NOTIFICATION_DESCRIPTION;
    }

    public override string GetNotificationCategory()
    {
        return NotificationsConstants.NOTIFICATION_CATEGORY_D0D1;
    }

    public override DateTime GetNotificationFireTime()
    {
        return DateTime.Now + new TimeSpan(0, 60, 0);
    }

    public override bool CanScheduleNotification()
    {
        return true;
    }
}
