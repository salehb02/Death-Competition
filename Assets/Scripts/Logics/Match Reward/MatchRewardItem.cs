using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Button))]
public class MatchRewardItem : MonoBehaviour
{
    public enum RewardType { Coin, Exp, Trophy }

    [SerializeField] private GameObject rewardPanel;

    [Header("Trophy")]
    [SerializeField] private GameObject trophyReward;
    [SerializeField] private TextMeshProUGUI trophyCountText;

    [Header("Coin")]
    [SerializeField] private GameObject coinReward;
    [SerializeField] private TextMeshProUGUI coinCountText;

    [Header("Exp")]
    [SerializeField] private GameObject expReward;
    [SerializeField] private TextMeshProUGUI expCountText;

    private RewardType rewardType;
    private int rewardCount;

    private Button button;
    private MatchReward matchReward;

    private void Start()
    {
        matchReward = FindObjectOfType<MatchReward>();
        button = GetComponent<Button>();

        button.onClick.AddListener(SelectReward);

        SetReward();

        rewardPanel.SetActive(false);
        coinReward.SetActive(false);
        expReward.SetActive(false);
        trophyReward.SetActive(false);
    }

    private void SetReward()
    {
        // Select random reward type
        var values = Enum.GetValues(typeof(RewardType));
        rewardType = (RewardType)values.GetValue(Random.Range(0, values.Length));

        while (true)
        {
            var chance = Random.Range(0f, 1f);

            if (rewardType == RewardType.Coin)
            {
                if (chance < 0.1f)
                {
                    rewardCount = 75;
                    break;
                }
                else if (chance < 0.2f)
                {
                    rewardCount = 50;
                    break;
                }
                else if (chance < 0.3f)
                {
                    rewardCount = 25;
                    break;
                }
            }

            if (rewardType == RewardType.Trophy)
            {
                if (chance < 0.2f)
                {
                    rewardCount = 30;
                    break;
                }
                else if (chance < 0.3f)
                {
                    rewardCount = 15;
                    break;
                }
            }

            if (rewardType == RewardType.Exp)
            {
                if (chance < 0.1f)
                {
                    rewardCount = 40;
                    break;
                }
                else if (chance < 0.2f)
                {
                    rewardCount = 20;
                    break;
                }
            }
        }
    }

    private void DisplayReward()
    {
        transform.DOLocalRotate(new Vector3(0, 90, 0), 0.25f).OnComplete(() =>
        {
            switch (rewardType)
            {
                case RewardType.Coin:
                    coinReward.SetActive(true);
                    break;
                case RewardType.Trophy:
                    trophyReward.SetActive(true);
                    break;
                case RewardType.Exp:
                    expReward.SetActive(true);
                    break;
            }

            rewardPanel.SetActive(true);
            transform.DOLocalRotate(Vector3.zero, 0.25f);
        });
    }

    private void SelectReward()
    {
        if (!matchReward.CanSelectReward)
            return;

        DisplayReward();

        switch (rewardType)
        {
            case RewardType.Coin:
                matchReward.AddWonCoin(rewardCount);
                break;
            case RewardType.Exp:
                matchReward.AddWonExp(rewardCount);
                break;
            case RewardType.Trophy:
                matchReward.AddWonTrophy(rewardCount);
                break;
            default:
                break;
        }

        button.interactable = false;
        matchReward.SelectReward();
        AudioManager.Instance.FlipCardSFX();
    }
}