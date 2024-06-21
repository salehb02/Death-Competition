using SSDM;
using System;
using System.Collections.Generic;

namespace MSDM
{
    //**********Callbakc Shope**********
    [System.Serializable]
    public class Shope
    {
        public bool success;
        public string message;
        public List<Coin> coins;
        public FreeCoin freeCoin;
        public List<Package> packages;
        public List<Booster> boosters;
    }
    //Coin Model
    [System.Serializable]
    public class Coin
    {
        public int coinReward;
        public string name;
        public PaymentInfo payment;
    }
    //Peyment Model
    [System.Serializable]
    public  class PaymentInfo
    {
        public string productName;
        public string productKey;
        public string market;
        public double cost;
        public string currency;
        public string description;
        public bool active;
    }
    //Free Coin
    [Serializable]
    public class FreeCoin
    {
        public int crL1;
        public int crL2;
        public int crL3;
        public double resetTime;
    }
    //Package
    [System.Serializable]
    public class Package
    {
        public int coinReward;
        public string name;
        public DateTime expierdDate;
        public int coefficientCoin;
        public int coefficientExp;
        public int coefficientTrophy;
        public PaymentInfo payment;
        public PaymentInfo fakePayment;
        public List<Set> sets;

    }
    //Set
    [System.Serializable]
    public  class Set
    {
        public string code;
        public string sex;
        public string name;
    }
    //**********Callbakc Payment Request**********
    [System.Serializable]
    public class PaymentRequest
    {
        public bool success;
        public string message;
        public RequestInfo result;
    }
    //Request Info Model
    [System.Serializable]
    public class RequestInfo
    {
        public string developerPayload;
        public string productName;
        public string productKey;
        public string market;
        public double cost;
    }
    //**********Callbakc Payment Completed Result**********
    [System.Serializable]
    public class PaymentCompleted
    {
        public bool success;
        public string message;
    }
    //**********Callbakc Free Coin Result**********
    [System.Serializable]
    public class RewardCoin
    {
        public bool success;
        public string message;
        public int coin;
    }

    //**********Callbakc Get Ads Result**********
    [System.Serializable]
    public class Ads
    {
        public bool success;
        public string message;
        public List<AdItem> ads;
    }
    [System.Serializable]
    public class AdItem
    {
        public int id;
        public string name;
        public string description;
        public string url;
        public string img;
        public string position;
        public string size;
    }

    [System.Serializable]
    public class Boosters
    {
        public bool success;
        public string message;
        public List<Booster> boosters;
    }

    [System.Serializable]
    public class Booster
    {
        public int id;
        public string name;
        public string type;
        public int level;
        public VirtualPayment virtualPayment;
        public int buyCount;
    }


    //Virtual Payment Model
    [System.Serializable]
    public class VirtualPayment
    {
        public string name;
        public double price;
        public string currency;
        public bool active;
    }

}