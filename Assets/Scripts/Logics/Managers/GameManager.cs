using GSDM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[CreateAssetMenu(menuName = "Death Match/New Game Manager")]
public class GameManager : ScriptableObject
{
    [Space(2)]
    [Header("Audio")]
    public SFX SFXBank;
    public Music musics;

    [Space(2)]
    [Header("Scenes")]
    public string gameplayScene;
    public string customizationScene;
    public string mainMenuScene;
    public string loadingScene;

    [Space(2)]
    [Header("Update Log")]
    [TextArea] public string UpdateDescription;
    private const string UPDATE_LOG_PREF = "VERSION_{0}_CHECKED";

    [Space(2)]
    [Header("Database")]
    public TextAsset tutorialQuestionsDatabase;
    public Sprite[] leagueIcons;
    public Sprite[] positionSprites;
    public Sprite[] customizationSprites;
    public Sprite[] questionCategoryIcons;

    [Space(2)]
    [Header("Debug")]
    public bool ShowFPS = false;
    public bool PrintLogs = true;
    public bool InfiniteWealth = false;
    public bool TestAds = false;

    private SaveGame lastGameAchivements = null;
    public Questions LoadedQuestions { get; private set; }
    public int playerLastPosition { get; private set; }
    public int PlayerLevel { get => LatestPlayerInfo != null ? LatestPlayerInfo.data.userScore.level.level : 0; }
    public USDM.UserInfo LatestPlayerInfo { get; set; }

    [System.Serializable]
    public class Music
    {
        public AudioClip menu;
        public float menuVolume;

        public AudioClip gameplay;
        public float gameplayVolume;
    }

    [System.Serializable]
    public class SFX
    {
        public AudioClip lose;
        public AudioClip endGame;
        public AudioClip endPanel;
        public AudioClip correctAnswer;
        public AudioClip wrongAnswer;
        public AudioClip changeCloth;
        public AudioClip startBtn;
        public AudioClip settingPopUp;
        public AudioClip settingClose;
        public AudioClip confirm;
        public AudioClip cancel;
        public AudioClip deleteKey;
        public AudioClip sharkAttack;
        public AudioClip error;
        public AudioClip questionNotification;
        public AudioClip timeOver;
        public AudioClip falling;
        public AudioClip maleScream;
        public AudioClip femaleScream;
        public AudioClip cardFlip;
        public AudioClip rewardsScreenIntro;
        public AudioClip pirateShipEntrance;
        public AudioClip[] buttons;
        public AudioClip[] boosters;
        public AudioClip[] moneyDecreases;
        public AudioClip[] keyboards;
        public AudioClip[] swims;
        public AudioClip[] sharkSwims;
        public AudioClip[] blockRises;
        public AudioClip[] blockFalls;
        public AudioClip[] answerSends;
        public AudioClip[] waterFalls;
        public AudioClip[] fears;
        public AudioClip[] laughs;
    }

    public Sprite GetLeagueSprite(string name) => leagueIcons.SingleOrDefault(x => x.name == name);

    public void SetLastGameAchievemnts(GSDM.SaveGame data, int playerPos)
    {
        playerLastPosition = playerPos;
        lastGameAchivements = data;
    }
    public SaveGame PopLastGameAchievements()
    {
        if (lastGameAchivements == null)
            return null;

        if (lastGameAchivements.success == false)
            return null;

        var data = lastGameAchivements;
        lastGameAchivements = null;

        return data;
    }

    public void SetQuestions(Questions questions)
    {
        LoadedQuestions = questions;
    }

    public List<Question> GetQuestions()
    {
        return GetServerQuestions();
    }

    private List<Question> GetServerQuestions()
    {
        if (LoadedQuestions != null && LoadedQuestions.questions.Count > 0)
            return LoadedQuestions.questions;

        throw new NullReferenceException("GameManager:: GetServerQuestions:: no question to load!");
    }

    public List<Question> GetTutorialQuestions()
    {
        if (tutorialQuestionsDatabase == null)
            throw new NullReferenceException("GameManager::GetTutorialQuestions:: There is no tutorial question to load.");

        var question = JsonUtility.FromJson<Questions>(tutorialQuestionsDatabase.text);

        SetQuestions(question);

        return question.questions;
    }

    public Sprite GetCustomizationSetSprite(string name)
    {
        return customizationSprites.SingleOrDefault(x => x.name == name);
    }

    public string CheckForUpdateLog()
    {
        if (UpdateDescription == null)
            return null;

        var descHash = GetStringSha256Hash(UpdateDescription);

        if (PlayerPrefs.GetString(string.Format(UPDATE_LOG_PREF, Application.version)) != descHash)
        {
            var log = new StringBuilder();
            log.Append($"<i><b>*نسخه {Application.version}</b></i>");
            log.Append("\n");
            log.Append(UpdateDescription);

            PlayerPrefs.SetString(string.Format(UPDATE_LOG_PREF, Application.version), descHash);

            return log.ToString();
        }

        return null;
    }

    private string GetStringSha256Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using (var sha = new System.Security.Cryptography.SHA256Managed())
        {
            var textData = Encoding.UTF8.GetBytes(text);
            var hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }

    [Serializable]
    public class Item
    {
        public string number;
        public string question;
        public string answer;
        public string wrongAnswer;
        public string difficulty;
        public string category;
    }

    [Serializable]
    public class Items
    {
        public Item[] items;
    }

    #region Singleton
    private static GameManager instance;
    public static GameManager Instance { get { if (instance == null) instance = Resources.Load("Game Manager") as GameManager; return instance; } }
    #endregion
}