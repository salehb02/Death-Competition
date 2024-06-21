using UnityEngine;

namespace DeathMatch
{
    public class Leaderboard : MonoBehaviour
    {
        private LeaderboardPresentor presentor;

        private bool isLoadingLeaderboard;

        private void Start()
        {
            presentor = GetComponent<LeaderboardPresentor>();
        }

        public void OpenLeaderboard()
        {
            if (isLoadingLeaderboard)
                return;

            isLoadingLeaderboard = true;
            ServerConnection.Instance.GetLeaderboard(OnGetLeaderboard);
        }

        private void OnGetLeaderboard(GSDM.LeaderBoard leaderboard)
        {
            presentor.SetPlayerTrophy(leaderboard.myInfo.trophy);
            presentor.SetOpponents(leaderboard.topUsersInfo);
        }
    }
}