
namespace SDM// Server Data Model's
{
    [System.Serializable]
    public class ServerModel
    {
        public bool reportState;
        public string report;
        public string reportTitle;
        public string reportMessage;
        public string reportTime;
    }

    public class CheckVersion
    {
        public bool success;
        public string message;
        public string currentVersion;
        public string allowedVersion;
        public string myketUrl;
        public string bazarUrl;
        public bool allowed;
        public bool updateNeed;

    }

    public class UserExit
    {
        public bool success;
        public string message;
    }

    public class SimpleModel
    {
        public bool success;
        public string message;
    }
}