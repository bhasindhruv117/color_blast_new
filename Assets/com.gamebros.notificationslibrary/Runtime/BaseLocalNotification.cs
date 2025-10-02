using System;
using Unity.Notifications;
using Unity.Notifications.Android;

namespace NotificationsLibrary.Runtime
{
    public abstract class BaseLocalNotification
    {
        public virtual int? GetNotificationId()
        {
            return 0;
        }

        public virtual int GetNotificationBadge()
        {
            return 0;
        }

        public abstract string GetNotificationExtraData();

        public abstract string GetNotificationTitle();

        public virtual string GetNotificationGroup()
        {
            return String.Empty;
        }

        public abstract string GetNotificationText();

        public virtual bool CanShowNotificationInForeGround()
        {
            return false;
        }

        public virtual bool IsGroupSummaryEnabled()
        {
            return false;
        }

        public abstract string GetNotificationCategory();

        /// <summary>
        /// Return Fire time in form of DateTime
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetNotificationFireTime();

        public virtual bool CanScheduleNotification()
        {
            return true;
        }

        public virtual TimeSpan? GetNotificationRepeatInterval()
        {
            return null;
        }

        public virtual NotificationStyle GetNotificationStyle()
        {
            return NotificationStyle.None;
        }

        public virtual string GetLargeIcon()
        {
            return "icon_1";
        }

        public virtual string GetSmallIcon()
        {
            return "icon_0";
        }

        public virtual BigPictureStyle? GetBigPictureStyle()
        {
            return null;
        }
    }
    
}
