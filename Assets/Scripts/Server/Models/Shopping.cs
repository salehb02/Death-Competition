using System;
using System.Collections.Generic;

namespace ShSDM
{

    [Serializable]
    class GemsInfo
    {
        public bool success;
        public string message;
        public List<Gem> gems;
    }

    [Serializable]
    public class Gem
    {
        public double gemReward;
        public Payment payment;
    }











    //**********Skins Handels**********
    [Serializable]
    class SkinsInfo
    {
        public bool success;
        public string message;
        public List<Skin> skins;
    }

    [Serializable]
    public class Skin
    {
        public int skinId;
        public string type;
        public bool men;
        public string name;
        public int color;
        public bool bought;
        public Payment payment;
    }

    [Serializable]
    public class Payment
    {
        public string title;
        public string myket;
        public string bazar;
        public double cost;
        public bool active;
    }


    public class UserSkinsInfo
    {

        public bool success;
        public string message;
        public List<UserSkin> skins;
    }

    [Serializable]
    public class UserSkin
    {
        public string type;
        public bool men;
        public string name;
        public int color;
    }



}