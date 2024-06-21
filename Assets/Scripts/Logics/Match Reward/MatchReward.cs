using DeathMatch;
using GameAnalyticsSDK;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class MatchReward : MonoBehaviour
{
    [SerializeField] private float showPanelDelay = 2f;

    public bool CanSelectReward { get; private set; } = true;

    private int maxRewardsCount;
    private int selectedRewardsCount;

    private int totalWonExp;
    private int totalWonCoin;
    private int totalWonTrophy;

    private MatchRewardPresentor presentor;
    private Scoring scoring;
    private EventManager eventManager;

    private void Start()
    {
        presentor = GetComponent<MatchRewardPresentor>();
        eventManager = FindObjectOfType<EventManager>();
        scoring = FindObjectOfType<Scoring>();
    }

    public void ShowMatchReward(int rank)
    {
        StartCoroutine(ShowMatchRewardCoroutine(rank));
    }

    private IEnumerator ShowMatchRewardCoroutine(int rank)
    {
        yield return new WaitForSeconds(showPanelDelay);

        AudioManager.Instance.RewardsScreenIntroSFX();

        switch (rank)
        {
            case 1:
                maxRewardsCount = 4;
                break;
            case 2:
                maxRewardsCount = 3;
                break;
            case 3:
                maxRewardsCount = 2;
                break;
            case 4:
                maxRewardsCount = 1;
                break;
            default:
                break;
        }

        presentor.SetRank(rank);
        presentor.ShowPanel();
        presentor.SetChosenRewardsCount(selectedRewardsCount, maxRewardsCount);
        AudioManager.Instance.EndPanelSFX();
    }

    public void SelectReward()
    {
        selectedRewardsCount++;
        presentor.SetChosenRewardsCount(selectedRewardsCount, maxRewardsCount);

        if (selectedRewardsCount >= maxRewardsCount)
        {
            CanSelectReward = false;
            presentor.ShowBackButton();

            Invoke(nameof(AutoReturnToMenu), 5f);
        }
    }

    public void AddWonCoin(int count)
    {
        totalWonCoin += count;
    }

    public void AddWonTrophy(int count)
    {
        totalWonTrophy += count;
    }

    public void AddWonExp(int count)
    {
        totalWonExp += count;
    }

    [Obsolete]
    public void BackToMenu()
    {
        var playerTime = TimeSpan.FromSeconds(eventManager.Player.OverallTimer).ToString(@"mm\:ss");
        var playerRank = scoring.Position(eventManager.Player.Username);

        ServerConnection.Instance.SaveGameData(playerTime, true, playerRank, totalWonCoin, totalWonExp, totalWonTrophy, (saveGameData) =>
        {
            GameManager.Instance.SetLastGameAchievemnts(saveGameData, scoring.Position(eventManager.Player.Username) - 1);

            // GameAnalytics events
            var coinAmount = System.Convert.ToInt32(Regex.Match(saveGameData.data.changeCoin, @"\d+").Value);
            GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, "Coin", coinAmount, "MatchCoinReward", $"Coin{coinAmount}");

            // GameAnalytics events
            GameAnalytics.NewDesignEvent("WinResultsReturnBTN");

            if (FindObjectOfType<TutorialSteps>(true).TutorialEnabledFromStart)
                GameAnalytics.NewDesignEvent("ReturnToMenu_Tutorial");

            LevelLoader.LoadLevel(GameManager.Instance.loadingScene);
        });

        if (SaveManager.Get<bool>(SaveManager.UNIQUE_EVENTS_ACTIVE) == true)
            ServerConnection.Instance.EventSaveGame(playerTime, playerRank, SaveManager.Get<int>(SaveManager.SELECTED_EVENT_ID), null);
    }

    private void AutoReturnToMenu()
    {
        BackToMenu();
    }
}