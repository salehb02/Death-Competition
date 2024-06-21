using GameAnalyticsSDK;
using UnityEngine;

namespace DeathMatch
{
    public class GameplayEntrance : MonoBehaviour
    {
        [SerializeField] private int entranceCost = 100;

        private GameplayEntrancePresentor presentor;
        private TutorialSteps tutorial;
        private MainUI mainUI;

        private void Start()
        {
            tutorial = FindObjectOfType<TutorialSteps>();
            presentor = FindObjectOfType<GameplayEntrancePresentor>();
            mainUI = FindObjectOfType<MainUI>();

            presentor.SetEntrancePrice(entranceCost);
        }

        public void CheckCoin()
        {
            if (!tutorial.TutorialMode)
            {
                if (!WealthManager.Instance.IsEnoughCoin(entranceCost))
                {
                    mainUI.OpenNotEnoughCoin();
                    TapsellManager.Instance.RequestRewarded("64c6c7801bec232e92093d67");
                    AudioManager.Instance.ErrorSFX();
                    return;
                }
            }

            if (!tutorial.TutorialMode)
                mainUI.OpenBoosterSelectionPanel();
            else
                EnterGame();
        }

        public void EnterGame()
        {
            AudioManager.Instance.StartButtonSFX();
            
            if (!tutorial.TutorialMode)
                WealthManager.Instance.UseCoins(entranceCost, true);

            // GameAnalytics events
            GameAnalytics.NewDesignEvent("StartBTN", 1f);
            GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", entranceCost, "StartGameEntrance", "StartGameEntrance");

            LevelLoader.LoadLevel(GameManager.Instance.gameplayScene);
        }

        public void WatchAd()
        {
            if (TapsellManager.Instance.IsRewardedAdAvailable())
            {
                TapsellManager.Instance.ShowRewarded(mainUI.OpenBoosterSelectionPanel);
            }
            else
            {
                AudioManager.Instance.ErrorSFX();
                TapsellManager.Instance.ShowUIError();
                TapsellManager.Instance.RequestRewarded("64c6c7801bec232e92093d67");
            }
        }
    }
}