using System.Collections.Generic;

namespace USDM//User Server Data Model
{
    // ***** CallBack User Info For get UserInfo & Create User
    [System.Serializable]
    public class UserInfo
    {
        public bool success;
        public string message;
        public UserData data;
        public ExtraData extraData;
        public List<MSDM.AdItem> ads;
        //User Data Model
    }
    [System.Serializable]
    public class UserData
    {
        public User user;
        public Score userScore;
        public SetModel userStyle;
        public SetModel userPlatform;
    }
    //User Data Model

    public class ExtraData
    {
        public int matchMakingCost;
    }
    [System.Serializable]
    //user Model
    public class User
    {
        long userId;
        public string userName;
        public string userAvatar;
        public string sex;
        public string phone;
    }
    // ***** CallBack User Style For get setStyle & UserStyle
    [System.Serializable]
    public class UserSet
    {
        public bool success;
        public string message;
        public string username;
        public SetModel style;
    }

    [System.Serializable]
    public class SetModel
    {
        public string code;
        public string name;
        public string sex;
    }



    // ***** CallBack User Score For get UserScore
    [System.Serializable]
    public class UserScore
    {
        public bool success;
        public string message;
        public UserScoreData data;
    }
    //Score Model
    [System.Serializable]
    public class UserScoreData
    {
        string userName;
        public int rank;
        public Score score;
    }
    //Score Field
    [System.Serializable]
    public class Score
    {
        public double gem;
        public int exp;
        public int userScoreId;
        public Level level;
        public double coin;
        public int trophy;
        public Laegue league;
        public int rank;
    }
    [System.Serializable]
    //level model
    public class Level
    {
        public int level;
        public int minExp;
        public int maxExp;
        public int inProgress;
        public int maxProgress;
    }
    //League Model
    [System.Serializable]
    public class Laegue
    {
        public string leagueName;
        public string leagueAvatar;
    }
    //Coin in Special Time Requst Model (Coin Request)
    [System.Serializable]
    public class CoinRequest
    {
        public bool success;
        public string message;
        public int coin;
    }
    //Get Change Username Tariff For User (Update Username Tariff)
    [System.Serializable]
    public class UserTariff
    {
        public bool success;
        public string message;
        public int cost;
    }
    //Get Change Sex Tariff For User (Update Sex Tariff)
    [System.Serializable]
    public class SexTariff
    {
        public bool success;
        public string message;
        public int cost;
    }
    // ***** CallBack User Phone & Rest By Code Response
    [System.Serializable]
    public class UserPhone
    {
        public bool success;
        public string message;
        public string user;
        public string phone;
    }

    
}
