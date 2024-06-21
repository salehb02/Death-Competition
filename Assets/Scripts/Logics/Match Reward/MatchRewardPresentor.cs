using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchRewardPresentor : MonoBehaviour
{
    [SerializeField] private GameObject matchRewardsPanel;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI rankDescText;
    [SerializeField] private TextMeshProUGUI chosenRewardsText;
    [SerializeField] private MatchRewardItem[] rewardItems;
    [SerializeField] private Button backToMenuButton;

    private MatchReward matchReward;

    private void Start()
    {
        matchReward = GetComponent<MatchReward>();
        backToMenuButton.onClick.AddListener(matchReward.BackToMenu);

        HidePanel();
    }

    public void ShowPanel()
    {
        matchRewardsPanel.SetActive(true);
    }

    public void HidePanel() 
    {
        matchRewardsPanel.SetActive(false);
        backToMenuButton.gameObject.SetActive(false);
    }

    public void SetRank(int rank)
    {
        rankText.text = rank.ToString();

        switch (rank)
        {
            case 1:
                rankDescText.text = "اول شدی!";
                break;
            case 2:
                rankDescText.text = "دوم شدی!";
                break;
            case 3:
                rankDescText.text = "سوم شدی!";
                break;
            case 4:
                rankDescText.text = "چهارم شدی!";
                break;
            default:
                break;
        }
    }

    public void SetChosenRewardsCount(int selected, int max)
    {
        chosenRewardsText.text = $"{selected} از {max}";
    }

    public void ShowBackButton()
    {
        backToMenuButton.gameObject.SetActive(true);
    }
}
