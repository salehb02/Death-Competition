using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using DeathMatch;

public class LastGameResults : MonoBehaviour
{
    [SerializeField] private GameObject LastAchievementsPanel;
    [SerializeField] private TextMeshProUGUI ExpText;
    [SerializeField] private TextMeshProUGUI CoinText;
    [SerializeField] private TextMeshProUGUI TrophyText;
    [SerializeField] private Image RankImage;
    [SerializeField] private Button ConfirmButton;

    [SerializeField] private TweenItems coinsTween;
    [SerializeField] private TweenItems expTweens;
    [SerializeField] private TweenItems trophyTweens;

    private PlayerInfoPresentor playerInfoPresentor;

    private void Start()
    {
        playerInfoPresentor = FindObjectOfType<PlayerInfoPresentor>();

        CheckData();
        ConfirmButton.onClick.AddListener(() => LastAchievementsPanel.SetActive(false));
    }

    private void CheckData()
    {
        var data = GameManager.Instance.PopLastGameAchievements();

        if (data == null)
        {
            LastAchievementsPanel.SetActive(false);
            return;
        }

        //LastAchievementsPanel.SetActive(true);
        //RankImage.sprite = GameManager.Instance.positionSprites[GameManager.Instance.playerLastPosition];

        var coinAmount = System.Convert.ToInt32(Regex.Match(data.data.changeCoin, @"\d+").Value);
        var expAmount = System.Convert.ToInt32(Regex.Match(data.data.changeExp, @"\d+").Value);
        var trophyAmount = System.Convert.ToInt32(Regex.Match(data.data.changeTrophy, @"\d+").Value);

        //CoinText.DOCounter(0, coinAmount, 2f).OnComplete(() =>
        //{
        //    ExpText.DOCounter(0, expAmount, 2f).OnComplete(() =>
        //    {
        //        TrophyText.DOCounter(0, trophyAmount, 2f);
        //    });
        //});

        if (coinAmount > 0)
        {
            coinsTween.PlayTween(() =>
            {
                WealthManager.Instance.CanUpdateVisual = true;
            });
        }

        if (expAmount > 0)
        {
            playerInfoPresentor.DecreaseExperience(expAmount);

            expTweens.PlayTween(() =>
            {
                playerInfoPresentor.IncreaseExperienceAnimated(expAmount);
            });
        }

        if (trophyAmount != 0)
        {
            playerInfoPresentor.DecreaseTrophy(trophyAmount);

            trophyTweens.PlayTween(() =>
            {
                playerInfoPresentor.IncreaseTrophyAnimated(trophyAmount);
            });
        }
    }
}