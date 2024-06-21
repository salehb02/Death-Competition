using DeathMatch;
using GameAnalyticsSDK;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventsManager : MonoBehaviour
{
    [SerializeField] private Button eventsButton;
    [SerializeField] private Button closeButton;

    [Header("Not Enough Level")]
    [SerializeField] private Transform notEnoughLevelPanel;
    [SerializeField] private Button notEnoughLevelCloseButton;

    [Header("Not Registered")]
    [SerializeField] private GameObject registerFirstPanel;
    [SerializeField] private Button closeRegisterFirstButton;
    [SerializeField] private Button toRegisterPanelButton;

    [Header("Current Events")]
    [SerializeField] private Transform currentEventsPanel;
    [SerializeField] private Transform currentEventsHolder;
    [SerializeField] private EventItem eventItem;

    private ESDM.Events currentEvents;
    private EventDetails eventDetails;
    private QuestionCreation questionCreation;
    private MainUI mainUI;

    public ESDM.Events GetAllEvents { get => currentEvents; }

    private void Start()
    {
        mainUI = FindObjectOfType<MainUI>();
        questionCreation = FindObjectOfType<QuestionCreation>();
        eventDetails = FindObjectOfType<EventDetails>();

        SaveManager.Set(SaveManager.UNIQUE_EVENTS_ACTIVE, false);
        CloseCurrentEventsPanel();
        CloseNotEnoughLevelPanel();
        RemoveExistEvents();
        GetEvents();

        closeButton.onClick.AddListener(CloseCurrentEventsPanel);
        closeButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        closeButton.onClick.AddListener(() =>
        {
            GameAnalytics.NewDesignEvent("EVENTS_PANEL_CLOSED");
        });

        eventsButton.onClick.AddListener(OpenCurrentEventsPanel);
        eventsButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        notEnoughLevelCloseButton.onClick.AddListener(CloseNotEnoughLevelPanel);
        notEnoughLevelCloseButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        toRegisterPanelButton.onClick.AddListener(() =>
        {
            CloseRegisterFirstPanel();
            mainUI.OpenRegister();
        });
        toRegisterPanelButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        closeRegisterFirstButton.onClick.AddListener(CloseNotEnoughLevelPanel);
        closeRegisterFirstButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    public void OpenCurrentEventsPanel()
    {
        if (currentEvents.events.Count == 0 || GameManager.Instance.LatestPlayerInfo.data.userScore.level.level < 3)
        {
            OpenNotEnoughLevelPanel();
            return;
        }

        if (SaveManager.Get<bool>(SaveManager.IS_GUEST))
        {
            OpenRegisterFirstPanel();
            return;
        }

        GameAnalytics.NewDesignEvent("EVENTS_PANEL_OPENED");
        currentEventsPanel.gameObject.SetActive(true);
    }

    public void CloseCurrentEventsPanel()
    {
        currentEventsPanel.gameObject.SetActive(false);
    }

    private void GetEvents()
    {
        ServerConnection.Instance.GetEvents(OnGetEvents);
    }

    [System.Obsolete]
    private void OnGetEvents(ESDM.Events events)
    {
        currentEvents = events;
        LoadCurrentEventsUI();
    }

    private void LoadCurrentEventsUI()
    {
        RemoveExistEvents();

        foreach (var item in currentEvents.events)
        {
            var instance = Instantiate(eventItem, currentEventsHolder);
            instance.Setup(item);
        }
    }

    private void RemoveExistEvents()
    {
        foreach (Transform obj in currentEventsHolder)
            Destroy(obj.gameObject);
    }

    [System.Obsolete]
    public void RegisterEvent(ESDM.EventItem eventData)
    {
        var canParticipateMultipleEvents = true;
        var hasRegisteredBefore = currentEvents.events.Where(x => x.isRegistered).Count() > 0;

        if (!hasRegisteredBefore || canParticipateMultipleEvents)
        {
            ServerConnection.Instance.RegisterEvent(eventData.id, eventData.type.number, (events) =>
            {
                OnGetEvents(events);
                OpenEvent(eventData);
            });
            return;
        }

        // Player can't participate in mutliple events
    }

    public void OpenEvent(ESDM.EventItem eventData)
    {
        CloseCurrentEventsPanel();
        eventDetails.LoadEvent(eventData);
    }

    public void LaunchEvent(string eventCode)
    {
        switch (eventCode)
        {
            case "UE_EVENT":
                LaunchUniqueEventedMatch();
                break;
            case "QC_EVENT":
                LaunchQuestionCreation();
                break;
            default:
                break;
        }
    }

    private void LaunchUniqueEventedMatch()
    {
        AudioManager.Instance.StartButtonSFX();

        // GameAnalytics events
        GameAnalytics.NewDesignEvent("Event_StartUniqueEventMatch");

        SaveManager.Set(SaveManager.SELECTED_EVENT_ID, eventDetails.CurrentEventInfo.id);
        SaveManager.Set(SaveManager.UNIQUE_EVENTS_ACTIVE, true);
        LevelLoader.LoadLevel(GameManager.Instance.gameplayScene);
    }

    private void LaunchQuestionCreation()
    {
        eventDetails.CloseDetailsPanel();
        questionCreation.StartEvent();
    }

    private void OpenNotEnoughLevelPanel()
    {
        notEnoughLevelPanel.gameObject.SetActive(true);
    }

    private void CloseNotEnoughLevelPanel()
    {
        notEnoughLevelPanel.gameObject.SetActive(false);
    }

    private void OpenRegisterFirstPanel()
    {
        registerFirstPanel.SetActive(true);
    }

    private void CloseRegisterFirstPanel()
    {
        registerFirstPanel.SetActive(false);
    }
}