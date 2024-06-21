using GameAnalyticsSDK;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DeathMatch
{
    public class HourlyPrizeManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI resetDate;
        [SerializeField] private DailyCoin[] dailyCoins;

        [Header("Falling Coins")]
        [SerializeField] private GameObject fallingCoin;
        [SerializeField] private AudioClip[] collectCoinSFX;
        [SerializeField] private int fallingCoinIntensity = 20;
        [SerializeField] private Transform fallingCoinParent;

        private BazaarPayment shopManager;
        private MSDM.Shope shopData;
        private AdsManager adsManager;

        [Serializable]
        public class DailyCoin
        {
            public Button CollectButton;
            public TextMeshProUGUI PrizeText;
            public GameObject Lock;
            public TextMeshProUGUI InfoText;
        }

        private void Start()
        {
            shopManager = FindObjectOfType<BazaarPayment>();
            adsManager = FindObjectOfType<AdsManager>();

            shopManager.OnGetShop += OnGetShop;
        }

        private void OnDisable()
        {
            shopManager.OnGetShop -= OnGetShop;
        }

        private void OnGetShop(MSDM.Shope shop)
        {
            shopData = shop;

            CheckTime();
            LoadDailyCoins();
        }

        private void ShowVFXAndPlaySFX()
        {
            StartCoroutine(DelayedSFXCoroutine());

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < fallingCoinIntensity; j++)
                {
                    var coin = Instantiate(fallingCoin, transform.position, Quaternion.identity, fallingCoinParent).GetComponent<RectTransform>();
                    var anchor = new Vector2(Random.Range(0f, 1f), 1);
                    coin.anchorMin = anchor;
                    coin.anchorMax = anchor;
                    coin.anchoredPosition = new Vector2(0, j * 20f);
                }
            }
        }

        private IEnumerator DelayedSFXCoroutine()
        {
            for (int i = 0; i < fallingCoinIntensity / 1.3f; i++)
            {
                AudioManager.Instance.CustomPlayOneShot(collectCoinSFX[Random.Range(0, collectCoinSFX.Length)]);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }
        }

        private void CheckTime()
        {
            if (!SaveManager.HasKey(SaveManager.DAILY_COIN_START))
                ResetDailyCoins();

            var timeLeft = SaveManager.Get<DateTime>(SaveManager.DAILY_COIN_END) - DateTime.Now;

            resetDate.text = $"اعتبار تا: {timeLeft.Hours} ساعت و {timeLeft.Minutes} دقیقه";

            if (timeLeft.Ticks < 0)
                ResetDailyCoins();
        }

        private void ResetDailyCoins()
        {
            SaveManager.Remove(SaveManager.WATCHED_DAILY_COINS);
            SaveManager.Set(SaveManager.DAILY_COIN_START, DateTime.Now);
            SaveManager.Set(SaveManager.DAILY_COIN_END, DateTime.Now.AddHours(shopData.freeCoin.resetTime));

            LoadDailyCoins();
        }

        private void LoadDailyCoins()
        {
            for (int i = 0; i < dailyCoins.Length; i++)
            {
                if (SaveManager.Get<int>(SaveManager.WATCHED_DAILY_COINS) == i)
                {
                    dailyCoins[i].Lock.gameObject.SetActive(false);
                    dailyCoins[i].CollectButton.interactable = true;
                    dailyCoins[i].InfoText.text = "مشاهده تبلیغ";
                }
                else if (SaveManager.Get<int>(SaveManager.WATCHED_DAILY_COINS) < i)
                {
                    dailyCoins[i].Lock.gameObject.SetActive(true);
                    dailyCoins[i].CollectButton.interactable = false;
                    dailyCoins[i].InfoText.text = "مشاهده تبلیغ";
                }
                else
                {
                    dailyCoins[i].Lock.gameObject.SetActive(false);
                    dailyCoins[i].CollectButton.interactable = false;
                    dailyCoins[i].InfoText.text = "دریافت\nشده!";
                }

                dailyCoins[i].CollectButton.onClick.RemoveAllListeners();
                dailyCoins[i].CollectButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
                dailyCoins[i].CollectButton.onClick.AddListener(CollectDailyCoinReward);

                if (i == 0)
                    dailyCoins[i].PrizeText.text = shopData.freeCoin.crL1.ToString();
                else if (i == 1)
                    dailyCoins[i].PrizeText.text = shopData.freeCoin.crL2.ToString();
                else if (i == 2)
                    dailyCoins[i].PrizeText.text = shopData.freeCoin.crL3.ToString();
            }
        }

        private void CollectDailyCoinReward()
        {
            const string FIRST_AD = "64f4c5a928b2543b3cada6fc";
            const string SECOND_AD = "64f4c5b328b2543b3cada6fd";
            const string THIRD_AD = "64f4c5bdb8c2e8295be32267";

            CheckTime();

            var selectedADId = string.Empty;

            switch (SaveManager.Get<int>(SaveManager.WATCHED_DAILY_COINS))
            {
                case 0:
                    selectedADId = FIRST_AD;
                    GameAnalytics.NewDesignEvent("DailyAd1_Clicked");

                    adsManager.ShowFullscreenAd(() =>
                    {
                        ShowTapsellAd(selectedADId);
                    }, () =>
                    {
                        ServerConnection.Instance.GetRewardCoin((data) =>
                        {
                            OnReward();
                        });
                    });

                    break;
                case 1:
                    selectedADId = SECOND_AD;
                    ShowTapsellAd(selectedADId);
                    GameAnalytics.NewDesignEvent("DailyAd2_Clicked");
                    break;
                case 2:
                    selectedADId = THIRD_AD;
                    ShowTapsellAd(selectedADId);
                    GameAnalytics.NewDesignEvent("DailyAd3_Clicked");
                    break;
                default:
                    break;
            }

            void ShowTapsellAd(string selectedADId)
            {
                TapsellManager.Instance.RequestRewarded(selectedADId, () =>
                {
                    TapsellManager.Instance.ShowRewarded(() =>
                    {
                        ServerConnection.Instance.GetRewardCoin((data) =>
                        {
                            OnReward();
                        });
                    });
                });
            }

            void OnReward()
            {
                ShowVFXAndPlaySFX();
                WealthManager.Instance.ReloadCoinsFromServer(true);

                GameAnalytics.NewDesignEvent($"DailyAd{SaveManager.Get<int>(SaveManager.WATCHED_DAILY_COINS) + 1}_WATCHED");

                SaveManager.Set(SaveManager.WATCHED_DAILY_COINS, SaveManager.Get<int>(SaveManager.WATCHED_DAILY_COINS) + 1);
                LoadDailyCoins();
            }
        }
    }
}