using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DeathMatch
{
    public class MatchmakingPlayerInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI IndexText;
        [SerializeField] private TextMeshProUGUI UsernameText;
        [SerializeField] private TextMeshProUGUI LevelText;
        [SerializeField] private TextMeshProUGUI TrophyText;
        [SerializeField] private Image LeagueImage;

        public void Setup(int index, string username, int level, int trophy, Sprite leagueIcon = null)
        {
            IndexText.text = index.ToString();
            UsernameText.text = username;
            LevelText.text = $"سطح {level}";
            TrophyText.text = trophy.ToString();
            LeagueImage.sprite = leagueIcon;
        }
    }
}