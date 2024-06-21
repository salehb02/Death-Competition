using UnityEngine;
using TMPro;

namespace DeathMatch
{
    public class MatchmakingPresentor : MonoBehaviour
    {
        [SerializeField] private GameObject matchmakingUI;
        [SerializeField] private TextMeshProUGUI JoinedPlayersCountText;
        [SerializeField] private TextMeshProUGUI ElapsedTimeText;
        [SerializeField] private TextMeshProUGUI StateText;

        private void Start()
        {
            matchmakingUI.SetActive(true);
        }

        public void SetJoinedPlayersCountText(int joinedPlayers, int capacity)
        {
            JoinedPlayersCountText.text = $"{joinedPlayers} از {capacity} بازیکن آماده شدند";
        }

        public void SetElapsedTimeText(int time) => ElapsedTimeText.text = time.ToString();

        public void SetStateText(Matchmaking.MatchmakingState state)
        {
            switch (state)
            {
                case Matchmaking.MatchmakingState.Joining:
                    StateText.text = "در حال پیوستن...";
                    break;
                case Matchmaking.MatchmakingState.WaitingForPlayers:
                    StateText.text = "منتظر بازیکن باشید...";
                    break;
                case Matchmaking.MatchmakingState.StartingGame:
                    StateText.text = "در حال شروع بازی...";
                    break;
                default:
                    break;
            }
        }

        public void DisableUI()
        {
            matchmakingUI.SetActive(false);
        }
    }
}