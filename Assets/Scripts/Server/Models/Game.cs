using System.Collections.Generic;

namespace GSDM
{
    // Get Question List for Game Start ( Get Random Question)
    [System.Serializable]
    public class Questions
    {
        public bool success;
        public string message;
        public int countQuestions;
        public int responseTime;

        public int guide1Cost;
        public int guide2Cost;
        public int guide3Cost;

        public List<Question> questions;
    }

    [System.Serializable]
    public class Question
    {
        public int id;
        public int number;
        public string question;
        public string type;
        public string difficulty;
        public string userName;
        public List<Answer> answers;
        public List<Answer> wrongAnswers;
        public List<Category> categories;

        public string guide1;
        public string guide2;
        public string guide3;
    }

    [System.Serializable]
    public class Answer
    {
        public int answerId;
        public string description;
    }

    [System.Serializable]
    public class Category
    {
        public int categoryId;
        public string title;
    }

    // Match Making
    [System.Serializable]
    public class MatchMaking
    {
        public bool success;
        public string message;
        public List<MatchMakingUser> users;
    }

    [System.Serializable]
    public class MatchMakingUser
    {
        public string userName;
        public string sex;
        public string avatar;
        public MatchMakingUserScore score;
        public UserSet set;
        public UserSet platfrom;
    }

    // Match Making User Score
    [System.Serializable]
    public class MatchMakingUserScore
    {
        public int coin;
        public int exp;
        public int trophy;
        public MatchMakingUserLevel level;
        public MatchMakingUserLeague league;
    }

    [System.Serializable]
    public class MatchMakingUserLevel
    {
        public int level;
        public int minExp;
        public int maxExp;
    }

    [System.Serializable]
    public class MatchMakingUserLeague
    {
        public string leagueName;
        public string leagueAvatar;
        public int minimumTrophy;
        public int maximumTrophy;    
    }

    [System.Serializable]
    public class UserSet
    {
        public string code;
        public string sex;
        public string name;
    }

   

    // Leder Board
    [System.Serializable]
    public class LeaderBoard
    {
        public bool success;
        public string message;
        public List<LeaderBoardLegue> leagues;
        public LeaderboardUserInfo myInfo;
        //public int countUser;
        public List<LeaderboardUserInfo> topUsersInfo;
        public List<MSDM.AdItem> ads;

    }

    [System.Serializable]
    public class LeaderBoardLegue
    {
        public string leagueName;
        public string leagueAvatar;
        public string minimumTrophy;
        public string maximumTrophy;
        public bool thisLeague;
    }

    [System.Serializable]
    public class LeaderboardUserInfo
    {
        public string userName;
        public string avatar;
        public int level;
        public int trophy;
        public int rank;
        public UserSet set;
        public UserSet platfrom;
    }


    // Save Game Result's
    [System.Serializable]
    public class SaveGame
    {
        public bool success;
        public string message;
        public Data data;
    }
    [System.Serializable]
    public  class Data
    {
        public int rank;
        public string changeCoin;
        public string changeTrophy;
        public string changeExp;
        public LevelUpModel levelUp;
        public LeagueUpModel leagueUP;
    }

    [System.Serializable]
    public class LevelUpModel
    {
        public bool status;
        public string message;

        public LevelUpViewModel level;
    }

    [System.Serializable]
public class LevelUpViewModel
    {
        public int level;
        public int coinReward;
    }

    [System.Serializable]
    public class LeagueUpModel 
    {
        public bool status;
        public string message;
        public LeagueUpViewModel league;
    }

    [System.Serializable]
    public class LeagueUpViewModel
    {
        public string leagueName;
        public string leagueAvatar;
        public int coinReward;
        public int expReward;
        public int trophyReward;
    }

    // Request To Increas Coin
    class SixRequest
    {
        public string success;
        public string message;
    }


}