using System;
using System.Collections;
using Unity.Notifications;
using Unity.Notifications.Android;
using UnityEngine;

namespace NotificationsLibrary.Runtime
{
    public class NotificationsManager : MonoBehaviour
    {
        #region GAMEOBJECTS_REFERENCES

        public AndroidNotificationChannel[] _channels = new []
        {
            new AndroidNotificationChannel(NotificationsConstants.NOTIFICATION_CATEGORY_D0D1,NotificationsConstants.NOTIFICATION_CATEGORY_D0D1_NAME,NotificationsConstants.NOTIFICATION_CATEGORY_D0D1_DESC,Importance.High),
            new AndroidNotificationChannel(NotificationsConstants.NOTIFICATION_CATEGORY_MORNING,NotificationsConstants.NOTIFICATION_CATEGORY_MORNING_NAME,NotificationsConstants.NOTIFICATION_CATEGORY_MORNING_DESC,Importance.High),
            new AndroidNotificationChannel(NotificationsConstants.NOTIFICATION_CATEGORY_AFTERNOON,NotificationsConstants.NOTIFICATION_CATEGORY_AFTERNOON_NAME,NotificationsConstants.NOTIFICATION_CATEGORY_AFTERNOON_DESC,Importance.High),
            new AndroidNotificationChannel(NotificationsConstants.NOTIFICATION_CATEGORY_EVENING,NotificationsConstants.NOTIFICATION_CATEGORY_EVENING_NAME,NotificationsConstants.NOTIFICATION_CATEGORY_EVENING_DESC,Importance.High),
            new AndroidNotificationChannel(NotificationsConstants.NOTIFICATION_CATEGORY_NIGHT,NotificationsConstants.NOTIFICATION_CATEGORY_NIGHT_NAME,NotificationsConstants.NOTIFICATION_CATEGORY_NIGHT_DESC,Importance.High),
        };

        #endregion
        
        private static NotificationsManager _instance;
        public static NotificationsManager Instance => _instance;
        
        private Notification? _lastRespondedNotification;

        #region GETTERS

        public Notification? LastRespondedNotification => _lastRespondedNotification;

        #endregion

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
                return;
            }
            InitializeNotificationChannels();
        }
        
        IEnumerator Start()
        {
            var request = NotificationCenter.RequestPermission();
            yield return new WaitUntil(() => request.Status == NotificationsPermissionStatus.RequestPending);
            Debug.Log("Permission result: " + request.Status);
            
            var query = NotificationCenter.QueryLastRespondedNotification();
            yield return new WaitUntil(() => query.State == QueryLastRespondedNotificationState.Pending);
            if (query.State == QueryLastRespondedNotificationState.HaveRespondedNotification)
            {
                _lastRespondedNotification = query.Notification;
            }
            else
            {
                _lastRespondedNotification = null;
            }
        }

        private void InitializeNotificationChannels()
        {
            NotificationCenterArgs args = NotificationCenterArgs.Default;
            args.PresentationOptions = NotificationPresentation.Badge | NotificationPresentation.Sound | NotificationPresentation.Vibrate;
            args.AndroidChannelName = "Default";
            args.AndroidChannelDescription = "Default notification channel";
            args.AndroidChannelId = "default";
            NotificationCenter.Initialize(args);
            
#if UNITY_ANDROID
            foreach (var channel in _channels) {
                AndroidNotificationCenter.RegisterNotificationChannel(channel);
            }
#endif
        }

        public void ScheduleNotification(BaseLocalNotification notification)
        {
            if (!notification.CanScheduleNotification() || notification.GetNotificationFireTime() <= DateTime.Now) {
                return;
            }
#if UNITY_ANDROID
            AndroidNotification androidNotification = new AndroidNotification();
            androidNotification.Number = notification.GetNotificationBadge();
            androidNotification.IntentData = notification.GetNotificationExtraData();
            androidNotification.Title = notification.GetNotificationTitle();
            androidNotification.Text = notification.GetNotificationText();
            androidNotification.Group = notification.GetNotificationGroup();
            androidNotification.ShowInForeground = notification.CanShowNotificationInForeGround();
            androidNotification.GroupSummary = notification.IsGroupSummaryEnabled();
            androidNotification.FireTime = notification.GetNotificationFireTime();
            androidNotification.Style = notification.GetNotificationStyle();
            if (!string.IsNullOrEmpty(notification.GetLargeIcon())) {
                androidNotification.LargeIcon = notification.GetLargeIcon();
            }

            if (!string.IsNullOrEmpty(notification.GetSmallIcon()))
            {
                androidNotification.SmallIcon = notification.GetSmallIcon();
            }

            if (notification.GetNotificationStyle() == NotificationStyle.BigPictureStyle) {
                androidNotification.BigPicture = notification.GetBigPictureStyle();
            }

            if (notification.GetNotificationRepeatInterval() != null) {
                androidNotification.RepeatInterval = notification.GetNotificationRepeatInterval();
            }
            if (notification.GetNotificationId() != 0) {
                AndroidNotificationCenter.SendNotificationWithExplicitID(androidNotification,notification.GetNotificationCategory(), (int)notification.GetNotificationId());
            }else {
                AndroidNotificationCenter.SendNotification(androidNotification, notification.GetNotificationCategory());
            }
#else
            Notification notificationObj = new Notification();
            if (notification.GetNotificationId() != 0) {
                notificationObj.Identifier = notification.GetNotificationId();
            }
            notificationObj.Badge = notification.GetNotificationBadge();
            notificationObj.Data = notification.GetNotificationExtraData();
            notificationObj.Title = notification.GetNotificationTitle();
            notificationObj.Group = notification.GetNotificationGroup();
            notificationObj.Text = notification.GetNotificationText();
            notificationObj.ShowInForeground = notification.CanShowNotificationInForeGround();
            notificationObj.IsGroupSummary = notification.IsGroupSummaryEnabled();

            NotificationCenter.ScheduleNotification<NotificationSchedule>(notificationObj,
                    notification.GetNotificationCategory(), notification.GetNotificationSchedule());
                Debug.Log($"Notification scheduled with Title: {notification.GetNotificationTitle()}");
#endif
        }

        public void CancelNotification(int id)
        {
            NotificationCenter.CancelScheduledNotification(id);
        }

        public void CancelAllNotifications()
        {
            NotificationCenter.CancelAllScheduledNotifications();
        }

        public void RemoveNotificationFromTray(int id)
        {
            NotificationCenter.CancelDeliveredNotification(id);
        }

        public void RemoveAllNotificationsFromTray()
        {
            NotificationCenter.CancelAllDeliveredNotifications();
        }

        public NotificationsPermissionStatus GetNotificationsPermissionStatus()
        {
           NotificationsPermissionRequest request = NotificationCenter.RequestPermission();
           return request.Status;
        }
    }
}

