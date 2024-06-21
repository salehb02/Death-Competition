using DeathMatch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUserLeaderboard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI likesCountText;

    [Header("Background")]
    [SerializeField] private Image background;
    [SerializeField] private Sprite playerBackground;
    [SerializeField] private Sprite notPlayerBackground;

    public void Setup(ESDM.EventMember member)
    {
        rankText.text = member.rank.ToString();

        if (member.userName != SaveManager.Get<string>(SaveManager.PLAYER_USERNAME))
            userNameText.text = member.userName;
        else
            userNameText.text = "شما";

        likesCountText.text = member.score.ToString();

        var isPlayer = member.userName == SaveManager.Get<string>(SaveManager.PLAYER_USERNAME);

        if (isPlayer)
            background.sprite = playerBackground;
        else
            background.sprite = notPlayerBackground;
    }
}