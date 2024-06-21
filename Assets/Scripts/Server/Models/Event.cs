using System.Collections.Generic;

namespace ESDM
{
    // Call Back Reward

    [System.Serializable]
    public class Claim
    {
        public bool success;
        public string message;
        public string reward;
        public string numberCall;
    }

    //**********Callbakc Events**********
    [System.Serializable]
    public class Events
    {
        public bool success;
        public string message;
        public int energy;
        public int timeReset;
        public List<EventItem> events;
    }

    [System.Serializable]
    public class EventItem
    {
        public int id;
        public string code;
        public string name;
        public string description;
        public EventType type;
        public bool isRegistered;
        public int cost;
        public int energyCost;
        public string startDate;
        public string endDate;
    }
    [System.Serializable]
    public class EventType
    {
        public int number;
        public string name;
    }

    //**********Callbakc Event Info**********
    [System.Serializable]
    public class EventDetaile
    {
        public bool success;
        public string message;
        public int energy;
        public EventInfo eventInfo;
    }

    [System.Serializable]
    public class EventInfo
    {
        public int id;
        public string code;
        public string name;
        public string description;
        public int limitQuestion;
        public List<EventCategory> categories;
        public List<EventGift> gifts;
        public List<EventMember> members;
    }

    [System.Serializable]
    public class EventCategory
    {
        public int id;
        public string category;
    }

    [System.Serializable]
    public class EventGift
    {
        public int rank;
        public int reward;
        public string name;
        public string type;
    }

    [System.Serializable]
    public class EventMember
    {
        public int id;
        public string userName;
        public int rank;
        public string sex;
        public int score;
        public bool Claim;
    }

    [System.Serializable]
    public class NewEventResponse
    {
        public bool success;
        public string message;
        public int count;
    }

    //For Send Question To Server
    [System.Serializable]
    public class NewQuestionEventElement
    {
        public string Question;
        public List<EventCategory> Categories;
        public string UserName;
        public List<EventAnswer> Answers;
        public int EventId;

        public NewQuestionEventElement(string question, List<EventCategory> cats, List<EventAnswer> answers, int id)
        {
            Question = question;
            Categories = cats;
            Answers = answers;
            EventId = id;
        }
    }

    [System.Serializable]
    public class EventAnswer
    {
        public bool IsCorrect;
        public string Answer;

        public EventAnswer(string answer)
        {
            IsCorrect = true;
            Answer = answer;
        }
    }
}