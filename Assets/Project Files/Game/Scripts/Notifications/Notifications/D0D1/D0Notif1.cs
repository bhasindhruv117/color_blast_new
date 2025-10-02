using System;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;
using Unity.Notifications;

public class D0Notif1 : BaseLocalNotification
{
    #region CONSTANTS

    private const string NOTIFICATION_TITLE = "\ud83d\ude80 Ready to Blast Some Blocks?";
    private const string NOTIFICATION_DESCRIPTION = "The ultimate block puzzle awaits! \ud83d\udd25\n\ud83c\udfa8 Stack, clear, and win big! \ud83c\udfc6";
    private const string NOTIFICATION_TYPE = "d0notif1";

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
        return DateTime.Now + new TimeSpan(60);
    }

    public override bool CanScheduleNotification()
    {
        return true;
    }
}
