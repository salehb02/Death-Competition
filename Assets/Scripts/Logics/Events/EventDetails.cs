using DeathMatch;
using ESDM;
using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventDetails : MonoBehaviour
{
    [SerializeField] private Transform panel;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private TextMeshProUGUI eventTitleText;
    [SerializeField] private TextMeshProUGUI timeLeftText;
    [SerializeField] private TextMeshProUGUI timeLeftTitleText;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject eventInProgressFooter;
    [SerializeField] private GameObject eventEndedFooter;

    [Header("Prizes")]
    [SerializeField] private Prize firstPrize;
    [SerializeField] private Prize secondPrize;
    [SerializeField] private Prize thirdPrize;

    [Header("Leaderboard")]
    [SerializeField] private Transform playersHolder;
    [SerializeField] private EventUserLeaderboard playerPrefab;

    [Header("Event Description")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Button descOpenButton;
    [SerializeField] private Button descCloseButton;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Start Event")]
    [SerializeField] private TextMeshProUGUI playerPropertyText;
    [SerializeField] private TextMeshProUGUI energyRechargeTimeText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button startButton;

    [Header("Not Enough Credit Panel")]
    [SerializeField] private GameObject notEnoughPanel;
    [SerializeField] private Button[] closeButtons;

    [Header("Reward Panel")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject closeButtonObject;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private TextMeshProUGUI contactNumberText;
    [SerializeField] private Button claimButton;
    private const string RANK_TEXT = "شما {0} شدید!";
    private const string CONTACT_TEXT = "برای دریافت جایزه خود با شماره <b>{0}</b>\r\n تماس بگیرید.";
    private bool isClaiming;

    private EventsManager eventsManager;

    private ESDM.EventItem currentEventItem;
    public ESDM.EventInfo CurrentEventInfo { get; private set; }
    public List<ESDM.EventCategory> CurrentEventCategories { get; private set; }

    [Serializable]
    public class Prize
    {
        public enum PrizeType { Coin, Skin, Booster, Custom }

        public PrizeType _PrizeType;
        public GameObject BoosterTitle;
        public TextMeshProUGUI TextedPrize;
        public Image SkinPrize;

        public string PrizeText;

        public void SetBoosterPrize(string item)
        {
            HideAllPrizes();

            BoosterTitle.SetActive(true);
            TextedPrize.text = item;
            _PrizeType = PrizeType.Booster;
        }

        public void SetCoinPrize(int count)
        {
            HideAllPrizes();

            PrizeText = $"<sprite index=3>{count}";
            TextedPrize.text = PrizeText;
            _PrizeType = PrizeType.Coin;
        }

        public void SetSkinPrize(Sprite sprite)
        {
            HideAllPrizes();

            SkinPrize.gameObject.SetActive(true);
            SkinPrize.sprite = sprite;
            _PrizeType = PrizeType.Skin;
        }

        public void SetCustomPrize(string text)
        {
            HideAllPrizes();

            PrizeText = text;
            TextedPrize.text = PrizeText;
            _PrizeType = PrizeType.Custom;
        }

        public void HideAllPrizes()
        {
            BoosterTitle.gameObject.SetActive(false);
            TextedPrize.text = string.Empty;
            SkinPrize.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        eventsManager = FindObjectOfType<EventsManager>();

        InitUI();
        CloseDetailsPanel();
        CloseNotEnoughCreditPanel(false);

        descOpenButton.onClick.AddListener(ShowDescription);
        descOpenButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        descCloseButton.onClick.AddListener(HideDescription);
        descCloseButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        closeButton.onClick.AddListener(ClosePrizePanel);
        closeButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        foreach (var btn in closeButtons)
        {
            btn.onClick.AddListener(() => CloseNotEnoughCreditPanel());
            btn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }

        closePanelButton.onClick.AddListener(() =>
        {
            CloseDetailsPanel();
            eventsManager.OpenCurrentEventsPanel();
            GameAnalytics.NewDesignEvent($"EVENTID_{CurrentEventInfo.id}_DETAILS_CLOSED");
        });

        closePanelButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            HideDescription();
    }

    private void InitUI()
    {
        RemoveAllPlayers();
        eventTitleText.text = "...";
        timeLeftText.text = string.Empty;

        firstPrize.HideAllPrizes();
        secondPrize.HideAllPrizes();
        thirdPrize.HideAllPrizes();
        HideDescription();

        costText.text = "هزینه: ...";
        playerPropertyText.text = string.Empty;
        energyRechargeTimeText.text = string.Empty;

        ClosePrizePanel();
    }

    public void OpenDetailsPanel()
    {
        panel.gameObject.SetActive(true);
    }

    public void CloseDetailsPanel()
    {
        panel.gameObject.SetActive(false);
    }

    [Obsolete]
    public void LoadEvent(ESDM.EventItem data)
    {
        currentEventItem = data;

        OpenDetailsPanel();
        InitUI();
        loadingPanel.SetActive(true);

        ServerConnection.Instance.GetEventDetails(data.id, (details) =>
        {
            OnLoadEventDetails(data, details);
            loadingPanel.SetActive(false);
        });
    }

    private void OnLoadEventDetails(ESDM.EventItem item, ESDM.EventInfo info)
    {
        CurrentEventInfo = info;
        eventTitleText.text = info.name;

        var timeLeft = DateTime.Parse(item.endDate) - DateTime.Now;

        if (timeLeft.Days > 0)
            timeLeftText.text = $"{timeLeft.Days} روز";
        else
            timeLeftText.text = $"{timeLeft.Hours.ToString("D2")}:{timeLeft.Minutes.ToString("D2")}";

        var isEventEnded = timeLeft.Ticks <= 0;

        eventInProgressFooter.SetActive(!isEventEnded);
        eventEndedFooter.SetActive(isEventEnded);

        if (isEventEnded)
        {
            timeLeftTitleText.text = "پایان رویداد";
            timeLeftText.text = string.Empty;
        }

        descriptionText.text = info.description;

        CurrentEventCategories = info.categories;

        for (int i = 0; i < info.gifts.Count; i++)
        {
            if (i == 0)
            {
                switch (info.gifts[i].type)
                {
                    case "Coin":
                        firstPrize.SetCoinPrize(info.gifts[i].reward);
                        break;
                    case "Booster":
                        // Not implemented
                        break;
                    case "Set":
                        // Not implemented
                        break;
                    case "RealReward":
                        firstPrize.SetCustomPrize(info.gifts[i].name);
                        break;
                    default:
                        break;
                }
            }
            else if (i == 1)
            {
                switch (info.gifts[i].type)
                {
                    case "Coin":
                        secondPrize.SetCoinPrize(info.gifts[i].reward);
                        break;
                    case "Booster":
                        // Not implemented
                        break;
                    case "Set":
                        // Not implemented
                        break;
                    case "RealReward":
                        secondPrize.SetCustomPrize(info.gifts[i].name);
                        break;
                    default:
                        break;
                }
            }
            else if (i == 2)
            {
                switch (info.gifts[i].type)
                {
                    case "Coin":
                        thirdPrize.SetCoinPrize(info.gifts[i].reward);
                        break;
                    case "Booster":
                        // Not implemented
                        break;
                    case "Set":
                        // Not implemented
                        break;
                    case "RealReward":
                        thirdPrize.SetCustomPrize(info.gifts[i].name);
                        break;
                    default:
                        break;
                }
            }
        }

        foreach (var player in info.members)
        {
            var instance = Instantiate(playerPrefab, playersHolder);
            instance.Setup(player);

            if (isEventEnded)
            {
                if (player.userName == SaveManager.Get<string>(SaveManager.PLAYER_USERNAME) && !player.Claim)
                    ShowPrize(player, currentEventItem.id);
            }
        }

        if (item.cost == -1)
        {
            if (item.energyCost != 0)
                costText.text = $"هزینه: {item.energyCost} <sprite index=2>";
            else
                costText.text = $"هزینه: رایگان!";

            playerPropertyText.text = $"موجودی: {eventsManager.GetAllEvents.energy} <sprite index=2>";
            energyRechargeTimeText.text = $"{Mathf.Clamp(eventsManager.GetAllEvents.timeReset, 1, Mathf.Infinity)} ساعت تا افزایش شارژ...";
        }
        else
        {
            if (item.cost != 0)
                costText.text = $"هزینه: {item.cost} <sprite index=3>";
            else
                costText.text = $"هزینه: رایگان!";

            playerPropertyText.text = $"موجودی: {GameManager.Instance.LatestPlayerInfo.data.userScore.coin.ToString("#,#")} <sprite index=3>";
        }

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            if (item.cost == -1)
            {
                if (eventsManager.GetAllEvents.energy < item.energyCost)
                {
                    OpenNotEnoughCreditPanel();
                    return;
                }
            }
            else
            {
                if (!WealthManager.Instance.IsEnoughCoin(item.cost))
                {
                    OpenNotEnoughCreditPanel();
                    return;
                }
            }

            GameAnalytics.NewDesignEvent($"EVENTID_{currentEventItem.id}_STARTED");
            eventsManager.LaunchEvent(info.code);
        });
        startButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    private void RemoveAllPlayers()
    {
        foreach (Transform obj in playersHolder)
            Destroy(obj.gameObject);
    }

    private void ShowDescription()
    {
        descriptionPanel.SetActive(true);
        GameAnalytics.NewDesignEvent($"EVENTID_{currentEventItem.id}_DESC_OPENED");
    }

    private void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }

    private void OpenNotEnoughCreditPanel()
    {
        notEnoughPanel.SetActive(true);
    }

    private void CloseNotEnoughCreditPanel(bool playSfx = true)
    {
        notEnoughPanel.SetActive(false);

        if (playSfx)
            AudioManager.Instance.ClickButtonSFX();
    }

    private void ShowPrize(EventMember player, int eventId)
    {
        if (player.rank > 3)
            return;

        switch (player.rank)
        {
            case 1:
                rankText.text = string.Format(RANK_TEXT, "اول");

                switch (firstPrize._PrizeType)
                {
                    case Prize.PrizeType.Skin:
                        // Not Implemented
                        break;
                    case Prize.PrizeType.Booster:
                        // Not Implemented
                        break;
                    case Prize.PrizeType.Coin:
                    case Prize.PrizeType.Custom:
                        rewardText.text = firstPrize.PrizeText;
                        break;
                    default:
                        break;
                }

                break;
            case 2:
                rankText.text = string.Format(RANK_TEXT, "دوم");

                switch (secondPrize._PrizeType)
                {
                    case Prize.PrizeType.Skin:
                        // Not Implemented
                        break;
                    case Prize.PrizeType.Booster:
                        // Not Implemented
                        break;
                    case Prize.PrizeType.Coin:
                    case Prize.PrizeType.Custom:
                        rewardText.text = secondPrize.PrizeText;
                        break;
                    default:
                        break;
                }

                break;
            case 3:
                rankText.text = string.Format(RANK_TEXT, "سوم");

                switch (thirdPrize._PrizeType)
                {
                    case Prize.PrizeType.Skin:
                        // Not Implemented
                        break;
                    case Prize.PrizeType.Booster:
                        // Not Implemented
                        break;
                    case Prize.PrizeType.Coin:
                    case Prize.PrizeType.Custom:
                        rewardText.text = thirdPrize.PrizeText;
                        break;
                    default:
                        break;
                }

                break;
            default:
                break;
        }

        contactNumberText.gameObject.SetActive(false);

        claimButton.gameObject.SetActive(true);
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() => ClaimPrize(eventId));
        claimButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        closeButtonObject.SetActive(false);
        closeButton.enabled = false;
        isClaiming = false;

        rewardPanel.SetActive(true);
    }

    private void ClosePrizePanel()
    {
        rewardPanel.SetActive(false);
    }

    private void ClaimPrize(int eventId)
    {
        if (isClaiming)
            return;

        isClaiming = true;

        ServerConnection.Instance.ClaimEventPrize(eventId, (data) =>
        {
            isClaiming = false;

            if (!string.IsNullOrEmpty(data.numberCall))
            {
                claimButton.gameObject.SetActive(false);
                closeButtonObject.SetActive(true);
                closeButton.enabled = true;
                contactNumberText.text = string.Format(CONTACT_TEXT, data.numberCall);
                contactNumberText.gameObject.SetActive(true);
            }
            else
            {
                WealthManager.Instance.ReloadCoinsFromServer(true);
                ClosePrizePanel();
            }
        });

        GameAnalytics.NewDesignEvent($"{SaveManager.Get<string>(SaveManager.PLAYER_USERNAME)}_CLAIMED_EVENT[id:{currentEventItem.id}]_PRIZE");
    }
}