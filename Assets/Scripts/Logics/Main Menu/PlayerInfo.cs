using UnityEngine;

namespace DeathMatch
{
    public class PlayerInfo : MonoBehaviour
    {
        private PlayerInfoPresentor _presentor;

        private void Start()
        {
            _presentor = FindObjectOfType<PlayerInfoPresentor>();
            WealthManager.Instance.OnUpdateCoins += OnUpdateCoins;
            WealthManager.Instance.UpdateUI();

            if (GameManager.Instance.LatestPlayerInfo != null)
                UpdateUserInfo(GameManager.Instance.LatestPlayerInfo);
            else
                ServerConnection.Instance.GetPlayerInfo(UpdateUserInfo);
        }

        private void OnDisable()
        {
            WealthManager.Instance.OnUpdateCoins -= OnUpdateCoins;
        }

        public void UpdateUserInfo(USDM.UserInfo userInfo)
        {
            _presentor.SetUsernameText(userInfo.data.user.userName);
            _presentor.SetUserLevelText(userInfo.data.userScore.level.level);
            _presentor.SetUserExp(userInfo.data.userScore.level.inProgress, userInfo.data.userScore.level.maxProgress);
            _presentor.SetUserTrophy(userInfo.data.userScore.trophy);
            _presentor.SetUserLeagueNameText(userInfo.data.userScore.league.leagueName);
            _presentor.SetUserLeagueAvatar(userInfo.data.userScore.league.leagueAvatar);
        }

        private void OnUpdateCoins(int coins)
        {
            _presentor.SetUserCoin(coins);
        }
    }
}