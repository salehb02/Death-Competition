using System;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnityNotificationManager : MonoBehaviour
{
    #region Singleton
    public static UnityNotificationManager Instance;

    private void Awake()
    {
        transform.SetParent(null);

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private const string FIRST_TIME_PREFS = "LOCAL_NOTIFICATION_INIT";

    private void Start()
    {
        AddNotificationChannels();
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        FirstTimeNotification();
        Every48HourNotification();
    }

    private void AddNotificationChannels()
    {
        var c = new AndroidNotificationChannel()
        {
            Id = "defaultTest",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c);
    }

    // EXAMPLE ------------------------------------------------------------------
    //public void SendTestNotif(string channelID)
    //{
    //    var notification = new AndroidNotification();
    //    notification.Title = "SomeTitle";
    //    notification.Text = "SomeText";
    //    notification.FireTime = System.DateTime.Now.AddSeconds(10);
    //    AndroidNotificationCenter.SendNotification(notification, channelID);
    //}
    // --------------------------------------------------------------------------

    private void FirstTimeNotification()
    {
        if (PlayerPrefs.HasKey(FIRST_TIME_PREFS))
            return;

        PlayerPrefs.SetInt(FIRST_TIME_PREFS, 1);

        var notification = new AndroidNotification();
        notification.Title = null;
        notification.Text = "برای چالش جدید آماده ای؟";

        var currentTime = System.DateTime.Now;
        var nextDayTime = currentTime;

        if (currentTime.Hour >= 0 && currentTime.Hour < 17)
        {
            nextDayTime = new System.DateTime(nextDayTime.Year, nextDayTime.Month, nextDayTime.Day, 21, 0, 0);
        }
        else
        {
            nextDayTime.AddDays(1);
            nextDayTime = new System.DateTime(nextDayTime.Year, nextDayTime.Month, nextDayTime.Day, 13, 0, 0);
        }

        notification.FireTime = nextDayTime;

        AndroidNotificationCenter.SendNotification(notification, "defaultTest");
    }

    private void Every48HourNotification()
    {
        var lastDate = DateTime.Now;

        for (int i = 0; i < 15; i++)
        {
            var notification = new AndroidNotification();
            notification.Title = null;
            notification.Text = GetEvery48HourNotificationText();

            var nextDayTime = lastDate.AddDays(2);

            if (nextDayTime.Hour >= 0 && nextDayTime.Hour < 9)
                nextDayTime = new System.DateTime(nextDayTime.Year, nextDayTime.Month, nextDayTime.Day, 9, 0, 0);

            lastDate = nextDayTime;
            notification.FireTime = nextDayTime;

            AndroidNotificationCenter.SendNotification(notification, "defaultTest");
        }
    }

    private string GetEvery48HourNotificationText()
    {
        var texts = new List<string>()
        {
            "به همین زودی منو یادت رفت؟",
            "نمی خوای برگردی؟",
            "حریف داره رجز میخونه نمیخوای جوابشو بدی؟",
            "برای چالش جدید آماده ای؟"
        };

        return texts[Random.Range(0, texts.Count)];
    }

    public void RegisterSimpleNotification(string title, string description, DateTime fireTime, int id)
    {
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = description;
        notification.FireTime = fireTime;
        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, "defaultTest", id);
    }

    public void CancelAllNotificationsWithID(int id)
    {
        AndroidNotificationCenter.CancelNotification(id);
    }
}