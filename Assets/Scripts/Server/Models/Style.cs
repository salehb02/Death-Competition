using System.Collections.Generic;

namespace SSDM
{
    // All Styles Model
    [System.Serializable]
    public class Sets
    {
        public bool success;
        public string message;
        public List<Set> sets;
        public List<MSDM.AdItem> ads;
    }

    //Style Model
    [System.Serializable]
    public class Set
    {
        public string code;
        public string sex;
        public string name;
        public string category;
        public bool bought;
        public bool chosen;
        public VirtualPayment virtualPayment;
        public int adPurchase;
        public bool locked;
        public int adWatched;
        public int level;
    }

    //Virtual Payment Model for StyleColor
    [System.Serializable]
    public class VirtualPayment
    {
        public string name;
        public double price;
        public string currency;
        public bool active;
    }

  
   
}
