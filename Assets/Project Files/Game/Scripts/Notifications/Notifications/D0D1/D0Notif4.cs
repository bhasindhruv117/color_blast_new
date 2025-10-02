using System;
using Newtonsoft.Json.Linq;
using NotificationsLibrary.Runtime;
using Unity.Notifications;
public class D0Notif4 : BaseLocalNotification
{
    #region CONSTANTS

    private const string NOTIFICATION_TITLE = "\ud83c\udfd7\ufe0f Your Blocks Are Waiting!";
    private const string NOTIFICATION_DESCRIPTION = "The board is set, the challenge is ON! \ud83c\udfc6 Jump back in and keep stacking those blocks! \ud83d\ude80";
    private const string NOTIFICATION_TYPE = "d0notif4";

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
        return DateTime.Now + new TimeSpan(0, 480, 0);
    }
}
