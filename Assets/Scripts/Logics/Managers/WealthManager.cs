using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeathMatch
{
    public class WealthManager : MonoBehaviour
    {
        #region Singlton
        public static WealthManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            transform.SetParent(null);
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion

        [SerializeField] private float transitionDuration = 1f;
        private int currentCoinsVisual;

        public int CoinsCount { get; private set; }
        public bool CanUpdateVisual { get; set; } = false;

        public event Action<int> OnUpdateCoins;

        [Obsolete]
        private void Start()
        {
            if (GameManager.Instance.LatestPlayerInfo != null)
            {
                OnGetCoins(GameManager.Instance.LatestPlayerInfo);
            }
            else
            {
                ServerConnection.Instance.GetPlayerInfo((info) =>
                {
                    OnGetCoins(info);
                });
            }

            SceneManager.activeSceneChanged += (prev, next) =>
            {
                ReloadCoinsFromServer(false);
            };

            void OnGetCoins(USDM.UserInfo info)
            {
                var coinCount = Convert.ToInt32(info.data.userScore.coin);
                UpdateCoins(coinCount);

                currentCoinsVisual = coinCount;
            }
        }

        private void Update()
        {
            if (!CanUpdateVisual)
                return;

            DOVirtual.Int(currentCoinsVisual, CoinsCount, transitionDuration, (value) =>
            {
                currentCoinsVisual = value;
                UpdateUI();
            });

            CanUpdateVisual = false;
        }

        [Obsolete]
        public void ReloadCoinsFromServer(bool updateVisuals)
        {
            ServerConnection.Instance.GetPlayerInfo((info) =>
            {
                UpdateCoins(Convert.ToInt32(info.data.userScore.coin));

                if (updateVisuals)
                    CanUpdateVisual = true;
            });
        }

        private void UpdateCoins(int coins)
        {
            CoinsCount = coins;

            if (GameManager.Instance.InfiniteWealth)
                CoinsCount = 999999;

            UpdateUI();
        }

        public void UseCoins(int value, bool requestToServer)
        {
            CoinsCount -= value;

            if (requestToServer)
                ServerConnection.Instance.DecreaseUserCoin(value, null);

            AudioManager.Instance.MoneyDecreaseSFX();

            CanUpdateVisual = true;
        }

        public void UpdateUI() => OnUpdateCoins?.Invoke(Convert.ToInt32(currentCoinsVisual));

        public bool IsEnoughCoin(int value) => CoinsCount >= value ? true : false;
    }
}