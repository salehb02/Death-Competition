using GameAnalyticsSDK;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextMeshProUGUI eventDescription;
    [SerializeField] private GameObject timer;
    [SerializeField] private GameObject tilStartTitle;
    [SerializeField] private GameObject tilEndTitle;
    [SerializeField] private GameObject eventEnded;
    [SerializeField] private TextMeshProUGUI timeLeftText;

    private ESDM.EventItem eventData;
    private Button button;
    private EventsManager eventsManager;

    public void Setup(ESDM.EventItem data)
    {
        button = GetComponent<Button>();
        eventsManager = FindObjectOfType<EventsManager>();

        eventData = data;

        eventTitle.text = data.name;

        if (data.description.Length > 110)
            eventDescription.text = data.description.Substring(0, 110) + "...";
        else
            eventDescription.text = data.description;

        tilStartTitle.SetActive(false);
        tilEndTitle.SetActive(false);
        eventEnded.SetActive(false);

        var isEventStarted = DateTime.Now >= DateTime.Parse(data.startDate);

        if (isEventStarted)
            tilEndTitle.SetActive(true);
        else
            tilStartTitle.SetActive(true);

        var timeLeft = (isEventStarted ? DateTime.Parse(data.endDate) : DateTime.Parse(data.startDate)) - DateTime.Now;

        if (timeLeft.Days > 0)
        {
            // More than one day left
            timeLeftText.text = $"{timeLeft.Days} روز";
        }
        else
        {
            // Less than one day left
            // So show the time left in '00:00' format
            timeLeftText.text = $"{timeLeft.Hours.ToString("D2")}:{timeLeft.Minutes.ToString("D2")}";
        }

        var isEventEnded = timeLeft.Ticks <= 0;

        if (isEventEnded)
        {
            timeLeftText.text = string.Empty;
            tilEndTitle.SetActive(false);
            tilStartTitle.SetActive(false);
            eventEnded.SetActive(true);
        }

        button.onClick.RemoveAllListeners();

        if (isEventStarted)
        {
            if (data.isRegistered)
                button.onClick.AddListener(() => eventsManager.OpenEvent(eventData));
            else
                button.onClick.AddListener(() => eventsManager.RegisterEvent(eventData));

            button.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
            button.onClick.AddListener(() =>
            {
                GameAnalytics.NewDesignEvent($"EVENTID_{data.id}_OPENED");
            });
        }
    }
}