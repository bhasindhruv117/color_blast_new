using System.Collections.Generic;
using NotificationsLibrary.Runtime;

public class LocalNotifications 
{
   #region SINGLETON

   private static LocalNotifications _instance;

   public static LocalNotifications Instance
   {
      get
      {
         if (_instance == null)
         {
            _instance = new LocalNotifications();
         }
         return _instance;
      }
   }

   private LocalNotifications()
   {
      AppLifeCycleEvents.OnApplicationPaused += ApplicationPaused;
      AppLifeCycleEvents.OnApplicationStarted += ApplicationStarted;
      AppLifeCycleEvents.OnApplicationResumed += ApplicationResumed;
   }

   #region NOTIFICATIONS

   private static readonly List<BaseLocalNotification> D0D1Notifications = new List<BaseLocalNotification>()
   {
      new D0Notif1(),
      new D0Notif2(),
      new D0Notif3(),
      new D0Notif4(),
   };

   private static readonly List<BaseLocalNotification> LapserNotifications = new List<BaseLocalNotification>()
   {
      // Morning Lapser
      new MorningNotif1(0, NotificationScheduleType.Lapser),
      new MorningNotif1(1, NotificationScheduleType.Lapser),
      new MorningNotif1(2, NotificationScheduleType.Lapser),
      new MorningNotif1(3, NotificationScheduleType.Lapser),
      new MorningNotif1(4, NotificationScheduleType.Lapser),
      
      // Afternoon Lapser
      new AfternoonNotif1(0,NotificationScheduleType.Lapser),
      new AfternoonNotif1(1,NotificationScheduleType.Lapser),
      new AfternoonNotif1(2,NotificationScheduleType.Lapser),
      new AfternoonNotif1(3,NotificationScheduleType.Lapser),
      new AfternoonNotif1(4,NotificationScheduleType.Lapser),
      
      // Evening Lapser
      new EveningNotif1(0,NotificationScheduleType.Lapser),
      new EveningNotif1(1,NotificationScheduleType.Lapser),
      new EveningNotif1(2,NotificationScheduleType.Lapser),
      new EveningNotif1(3,NotificationScheduleType.Lapser),
      new EveningNotif1(4,NotificationScheduleType.Lapser),
      
      // Night Lapser
      new NightNotif1(0,NotificationScheduleType.Lapser),
      new NightNotif1(1,NotificationScheduleType.Lapser),
      new NightNotif1(2,NotificationScheduleType.Lapser),
      new NightNotif1(3,NotificationScheduleType.Lapser),
      new NightNotif1(4,NotificationScheduleType.Lapser),
   };

   private static readonly List<BaseLocalNotification> ContinuousNotifications = new List<BaseLocalNotification>()
   {
      new MorningNotif1(),
      new AfternoonNotif1(),
      new EveningNotif1(),
      new NightNotif1(),
   };

   #endregion

   private void ApplicationResumed()
   {
      NotificationsManager.Instance.CancelAllNotifications();
      ScheduleContinuousNotifications();
      ScheduleLapserNotifications();
   }

   private void ApplicationStarted()
   {
      NotificationsManager.Instance.CancelAllNotifications();
      ScheduleContinuousNotifications();
      ScheduleLapserNotifications();
   }

   private void ScheduleContinuousNotifications()
   {
      foreach (var notification in ContinuousNotifications) {
         NotificationsManager.Instance.ScheduleNotification(notification);
      }
   }

   private void ScheduleLapserNotifications()
   {
      foreach (var notification in LapserNotifications) {
         NotificationsManager.Instance.ScheduleNotification(notification);
      }
   }

   private void ApplicationPaused()
   {
      ScheduleNotificationsOnPause();
   }

   private void ScheduleNotificationsOnPause()
   {
      if (Util.GetDaysSinceInstall() > 0) {
         return;
      }
      foreach (var notification in D0D1Notifications) {
         NotificationsManager.Instance.ScheduleNotification(notification);
      }
   }

   #endregion
   
   
   
}
