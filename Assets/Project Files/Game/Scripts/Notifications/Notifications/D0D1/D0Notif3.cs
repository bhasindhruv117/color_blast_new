using System;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;
using Unity.Notifications;

public class D0Notif3 : BaseLocalNotification
{
    #region CONSTANTS

    private const string NOTIFICATION_TITLE = "\ud83d\udd25 Risk It, Score Big!";
    private const string NOTIFICATION_DESCRIPTION = "No fails? That’s legendary! \ud83d\udcaa The greatest players take risks—aim for a higher score! \ud83d\udcc8\ud83d\udd25";
    private const string NOTIFICATION_TYPE = "d0notif3";

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
        return DateTime.Now + new TimeSpan(0, 240, 0);
    }

    public override bool CanScheduleNotification()
    {
        return true;
    }
}
