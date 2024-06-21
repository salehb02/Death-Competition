using DeathMatch;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPresentor : MonoBehaviour
{
    [SerializeField] private GameObject loadingDataText;

    [Space(2)]
    [Header("Tabs")]
    [SerializeField] private Button leaguesTabButton;
    [SerializeField] private Sprite leaguesTabSelected;
    [SerializeField] private Sprite leaguesTabNormal;
    [SerializeField] private GameObject leaguesTabPanel;
    [Space(1)]
    [SerializeField] private Button topPlayersTabButton;
    [SerializeField] private Sprite topPlayersTabSelected;
    [SerializeField] private Sprite topPlayersTabNormal;
    [SerializeField] private GameObject topPlayersTabPanel;

    [Space(2)]
    [Header("Top Players")]
    [SerializeField] private TopPlayer[] top3Players;
    [SerializeField] private GameObject usersDataHolder;
    [SerializeField] private GameObject userDataPrefab;
    [SerializeField] private Sprite othersRankBg;
    [SerializeField] private Sprite playerRankBg;
    [SerializeField] private ScrollRect listScrollRect;

    [Space(2)]
    [Header("Leagues")]
    [SerializeField] private LeagueData[] leagues;

    [System.Serializable]
    public struct TopPlayer
    {
        public Character Character;
        public TextMeshProUGUI UsernameText;
        public TextMeshProUGUI TrophyCountText;
    }

    [System.Serializable]
    public struct LeagueData
    {
        public string Title;
        public int MinTrophy;
        public int MaxTrophy;
        public TextMeshProUGUI TrophyRangeText;
        public GameObject PlayerIndicator;
    }

    private void Start()
    {
        leaguesTabButton.onClick.AddListener(OpenLeaguesTab);
        topPlayersTabButton.onClick.AddListener(OpenTopPlayersTab);

        InitAllElements();
        OpenTopPlayersTab();
    }

    private void InitAllElements()
    {
        foreach (Transform obj in usersDataHolder.transform)
            Destroy(obj.gameObject);

        var loadingText = "<sprite index=0>";
        var noTrophySprite = GameManager.Instance.GetLeagueSprite("No_Trophy");

        loadingDataText.SetActive(true);

        for (int i = 0; i < leagues.Length; i++)
        {
            if (i != leagues.Length - 1)
                leagues[i].TrophyRangeText.text = $"{leagues[i].MinTrophy}-{leagues[i].MaxTrophy}";
            else
                leagues[i].TrophyRangeText.text = $"{leagues[i].MinTrophy}+";

            leagues[i].PlayerIndicator.SetActive(false);
        }

        foreach (var topPlayer in top3Players)
        {
            topPlayer.UsernameText.text = loadingText;
            topPlayer.TrophyCountText.text = loadingText;
            topPlayer.Character.HideAvatar();
        }
    }

    public void SetPlayerTrophy(int rank)
    {
        for (int i = 0; i < leagues.Length; i++)
        {
            if (rank > leagues[i].MinTrophy && rank <= leagues[i].MaxTrophy)
                leagues[i].PlayerIndicator.SetActive(true);
            else
                leagues[i].PlayerIndicator.SetActive(false);
        }
    }

    public void SetOpponents(List<GSDM.LeaderboardUserInfo> users)
    {
        foreach (Transform obj in usersDataHolder.transform)
            Destroy(obj.gameObject);

        var playerIndex = -1;

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];

            if (i < 3)
            {
                top3Players[i].UsernameText.text = user.userName;
                top3Players[i].TrophyCountText.text = user.trophy.ToString();

                if (user.set != null && !string.IsNullOrEmpty(user.set.code))
                    top3Players[i].Character.LoadCustomization(user.set);
                else
                    top3Players[i].Character.LoadRandomCustomization();

                continue;
            }

            var data = Instantiate(userDataPrefab, usersDataHolder.transform);
            var usernameText = data.transform.Find("USERNAME_TEXT").GetComponent<TextMeshProUGUI>();
            var levelText = data.transform.Find("LEVEL_TEXT").GetComponent<TextMeshProUGUI>();
            var trophyText = data.transform.Find("TROPHY_TEXT").GetComponent<TextMeshProUGUI>();
            var rank = user.rank;
            var rankText = data.transform.Find("POSITION_TEXT").GetComponent<TextMeshProUGUI>();
            var rankBg = data.transform.Find("POSITION_IMAGE").GetComponent<Image>();

            if (user.userName == SaveManager.Get<string>(SaveManager.PLAYER_USERNAME))
            {
                usernameText.text = "شما";
                data.GetComponent<Image>().sprite = playerRankBg;
                playerIndex = i;
            }
            else
            {
                usernameText.text = user.userName;
                data.GetComponent<Image>().sprite = othersRankBg;
            }

            levelText.text = $"سطح {user.level}";
            trophyText.text = user.trophy.ToString();
            rankText.text = user.rank.ToString();

            loadingDataText.SetActive(false);
        }

        // check if player wasnt in leaderboard
        if (playerIndex == -1)
        {
            var data = Instantiate(userDataPrefab, usersDataHolder.transform);
            var usernameText = data.transform.Find("USERNAME_TEXT").GetComponent<TextMeshProUGUI>();
            var levelText = data.transform.Find("LEVEL_TEXT").GetComponent<TextMeshProUGUI>();
            var trophyText = data.transform.Find("TROPHY_TEXT").GetComponent<TextMeshProUGUI>();
            var rankText = data.transform.Find("POSITION_TEXT").GetComponent<TextMeshProUGUI>();

            usernameText.text = "شما";
            data.GetComponent<Image>().sprite = playerRankBg;

            var userData = GameManager.Instance.LatestPlayerInfo;

            levelText.text = $"سطح {userData.data.userScore.level.level}";
            trophyText.text = userData.data.userScore.trophy.ToString();
            //rankText.text = "+20";
            rankText.text = userData.data.userScore.rank.ToString();

            playerIndex = users.Count - 3;
        }

        if (playerIndex != -1)
            listScrollRect.DOVerticalNormalizedPos(1 - (playerIndex / (users.Count - 3)), 1);
    }

    private void CloseAllTabs()
    {
        leaguesTabButton.image.sprite = leaguesTabNormal;
        topPlayersTabButton.image.sprite = topPlayersTabNormal;

        topPlayersTabPanel.SetActive(false);
        leaguesTabPanel.SetActive(false);
    }

    private void OpenLeaguesTab()
    {
        CloseAllTabs();

        leaguesTabButton.image.sprite = leaguesTabSelected;
        leaguesTabPanel.SetActive(true);
    }

    private void OpenTopPlayersTab()
    {
        CloseAllTabs();

        topPlayersTabButton.image.sprite = topPlayersTabSelected;
        topPlayersTabPanel.SetActive(true);
    }
}