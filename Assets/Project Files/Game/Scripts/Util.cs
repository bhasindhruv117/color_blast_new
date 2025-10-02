
using System;
using UnityEngine;
using Random = System.Random;

public class Util
{
    private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static Random random = new Random();
    
    public static long GetCurrentLocalTimeStamp()
    {
        return Convert.ToInt64(DateTime.UtcNow.Subtract(_epoch).TotalSeconds);
    }

    public static int GetDaysSinceInstall()
    {
        if (!PlayerPrefs.HasKey("InstallTime"))
        {
            return 0;
        }
        
        string installTimeInString = PlayerPrefs.GetString("InstallTime");
        long installTime = Convert.ToInt64(installTimeInString);
        long secondsFromInstall = GetCurrentLocalTimeStamp() - installTime;
        return (int)secondsFromInstall / GameConstants.SECONDS_IN_DAY;
    }

    public static int GetCohortFromInstall()
    {
        int cohort = 0;
        if (!PlayerPrefs.HasKey("InstallTime"))
        {
            return 0;
        }
        string installTimeInString = PlayerPrefs.GetString("InstallTime");
        long installTime = Convert.ToInt64(installTimeInString);
        var installDate = GetDateTimeFromTimeStamp(installTime).Date;
        DateTime now = DateTime.Now.Date;
        cohort = (now - installDate).Days;
        return cohort;
    }

    public static void SetInstallTime()
    {
        if (PlayerPrefs.HasKey("InstallTime"))
        {
            return;
        }
        
        PlayerPrefs.SetString("InstallTime", GetCurrentLocalTimeStamp().ToString());
        PlayerPrefs.Save();
    }

    public static int GetRandomIntInRange(int min, int max)
    {
        return random.Next(min, max);
    }
    
    public static bool IsTimePassed(long timeStamp, int seconds)
    {
        return GetCurrentLocalTimeStamp() - timeStamp > seconds;
    }
    
    public static int GetBuildVersion()
    {
        int.TryParse(Application.version.Split('.')[1], out var result);
        return result;
    }

    public static int GetDaysDiffFromTimeStamp(long timestamp)
    {
        DateTime date = GetDateTimeFromTimeStamp(timestamp);
        DateTime current = DateTime.Now;
        return current.Subtract(date).Days;
    }

    public static DateTime GetDateTimeFromTimeStamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
    }
}
