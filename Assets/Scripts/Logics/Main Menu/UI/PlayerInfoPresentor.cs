using UnityEngine;
using TMPro;
using UnityEngine.UI;
using USDM;
using DG.Tweening;
using Unity.Notifications.Android;
using System.Collections.Generic;

namespace DeathMatch
{
    public class PlayerInfoPresentor : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private TextMeshProUGUI userLeagueNameText;
        [SerializeField] private TextMeshProUGUI userLevelText;
        [SerializeField] private TextMeshProUGUI userCoinText;
        [SerializeField] private Image userExpImage;
        [SerializeField] private TextMeshProUGUI userExpText;
        [SerializeField] private TextMeshProUGUI userTrophyText;

        [SerializeField] private Image userAvatarImage;
        [SerializeField] private Image userLeagueAvatarImage;
        [SerializeField] private TextMeshProUGUI currentLeagueName;
        [SerializeField] private Image currentLeagueAvatar;

        private int currentExperience;
        private int maxExperience;
        private int currentTrophy;

        private readonly Dictionary<string,string> ENGLISH_TO_PERSIAN_LEAGUE = new Dictionary<string, string>()
        {
            {"Unranked","بدون رتبه" },
            {"Bronze1","لیگ برنز 1" },
            {"Bronze2","لیگ برنز 2" },
            {"Bronze3","لیگ برنز 3" },
            {"Silver1","لیگ نقره 1" },
            {"Silver2","لیگ نقره 2" },
            {"Silver3","لیگ نقره 3" },
            {"Gold1","لیگ طلا 1" },
            {"Gold2","لیگ طلا 2" },
            {"Gold3","لیگ طلا 3" },
            {"Crystal1","لیگ الماس 4" },
            {"Crystal2","لیگ الماس 5" },
            {"Crystal3","لیگ الماس 6" },
            {"Master1","لیگ الماس 1" },
            {"Master2","لیگ الماس 2" },
            {"Master3","لیگ الماس 3" },
            {"Champion1","لیگ اترنیوم 1" },
            {"Champion2","لیگ اترنیوم 2" },
            {"Champion3","لیگ اترنیوم 3" },
            {"Titan1","لیگ تیتانیوم 1" },
            {"Titan2","لیگ تیتانیوم 2" },
            {"Titan3","لیگ تیتانیوم 3" },
            {"Legend","لیگ افسانه" },
        };

        private void Awake()
        {
            SetToLoading();
        }

        private void SetToLoading()
        {
            var loadingText = "<sprite index=0>";

            if (userNameText)
                userNameText.text = loadingText;

            if (userLeagueNameText)
                userLeagueNameText.text = loadingText;

            if (userLevelText)
                userLevelText.text = loadingText;

            if (userCoinText)
                userCoinText.text = loadingText;

            if (userTrophyText)
                userTrophyText.text = loadingText;

            if (userExpText)
                userExpText.text = loadingText;

            if (userExpImage)
                userExpImage.fillAmount = 0;
        }

        public void SetUsernameText(string text)
        {
            if (!userNameText)
                return;

            userNameText.text = text;
        }

        public void SetUserLeagueNameText(string leagueName)
        {
            if (ENGLISH_TO_PERSIAN_LEAGUE.ContainsKey(leagueName))
            {
                if (userLeagueNameText)
                    userLeagueNameText.text = ENGLISH_TO_PERSIAN_LEAGUE[leagueName];

                if (currentLeagueName)
                    currentLeagueName.text = ENGLISH_TO_PERSIAN_LEAGUE[leagueName];
            }
            else
            {
                if (userLeagueNameText)
                    userLeagueNameText.text = leagueName;

                if (currentLeagueName)
                    currentLeagueName.text = leagueName;
            }
        }

        public void SetUserLevelText(int level)
        {
            if (!userLevelText)
                return;

            userLevelText.text = level.ToString();
        }

        public void SetUserCoin(int coin)
        {
            if (!userCoinText)
                return;

            userCoinText.text = coin.ToString(coin == 0 ? null : "#,#");
        }

        public void SetUserTrophy(int trophy)
        {
            if (!userTrophyText)
                return;

            currentTrophy = trophy;
            UpdateTrophyText();
        }

        public void SetUserExp(int exp, int maxExp)
        {
            if (!userExpImage || !userExpText)
                return;

            currentExperience = exp;
            maxExperience = maxExp;

            UpdateExperienceBar();
        }

        public void SetUserLeagueAvatar(string leagueAvatar)
        {
            if (userLeagueAvatarImage)
                userLeagueAvatarImage.sprite = GameManager.Instance.GetLeagueSprite(leagueAvatar);

            if (currentLeagueAvatar)
                currentLeagueAvatar.sprite = GameManager.Instance.GetLeagueSprite(leagueAvatar);
        }

        public void DecreaseExperience(int amount)
        {
            currentExperience -= amount;
            currentExperience = Mathf.Clamp(currentExperience, 0, maxExperience);
            UpdateExperienceBar();
        }

        public void IncreaseExperienceAnimated(int amount)
        {
            var from = currentExperience;
            var to = currentExperience + amount;

            DOVirtual.Int(from, to, 1, (value) =>
            {
                currentExperience = value;
                UpdateExperienceBar();
            });
        }

        public void DecreaseTrophy(int amount)
        {
            currentTrophy -= amount;
            UpdateTrophyText();
        }

        public void IncreaseTrophyAnimated(int amount)
        {
            var from = currentTrophy;
            var to = currentTrophy + amount;

            DOVirtual.Int(from, to, 1, (value) =>
            {
                currentTrophy = value;
                UpdateTrophyText();
            });
        }

        private void UpdateExperienceBar()
        {
            userExpText.text = $"{currentExperience}/{maxExperience}";
            userExpImage.fillAmount = currentExperience / (float)maxExperience;
        }

        private void UpdateTrophyText()
        {
            userTrophyText.text = currentTrophy.ToString();
        }
    }
}