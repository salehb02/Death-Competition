namespace DeathMatch
{
    public class SaveManager
    {
        private static bool isInitialized;

        public const string PLAYER_USERNAME = "PLAYER_USERNAME";
        public const string PLAYER_LOGIN = "PLAYER_LOGIN";
        public const string IS_GUEST = "IS_GUEST";
        public const string IS_REGISTERED = "IS_REGISTERED";
        public const string OPEN_SHOP_IN_MENU = "OPEN_SHOP_IN_MENU";
        public const string WATCHED_DAILY_COINS = "WATCHED_DAILY_COINS";
        public const string DAILY_COIN_START = "DAILY_COIN_START";
        public const string DAILY_COIN_END = "DAILY_COIN_END1";
        public const string SELECTED_BOOSTER = "SELECTED_BOOSTER";
        public const string UNIQUE_EVENTS_ACTIVE = "UNIQUE_EVENTS_ACTIVE";
        public const string SELECTED_EVENT_ID = "SELECTED_EVENT_ID";
        public const string SHIELD_BOOSTER_MIN_LEVEL = "SHIELD_BOOSTER_MIN_LEVEL";
        public const string PIRATE_SHIP_BOOSTER_MIN_LEVEL = "PIRATE_SHIP_BOOSTER_MIN_LEVEL";

        // use custom es3settings to setup save cache for load and save speedup
        private static ES3Settings _es3Settings;

        private static void InitializeSave()
        {
            if (!ES3.FileExists())
                ES3.Save("__InitData__", 1);

            ES3.CacheFile();
            _es3Settings = new ES3Settings(ES3.Location.Cache);

            isInitialized = true;
        }

        private static void Save()
        {
            if (!isInitialized)
                InitializeSave();

            ES3.StoreCachedFile(_es3Settings);
        }

        public static bool HasKey(string key)
        {
            if (!isInitialized)
                InitializeSave();

            return ES3.KeyExists(key, _es3Settings);
        }

        public static void Set<T>(string key, T value)
        {
            if (!isInitialized)
                InitializeSave();

            ES3.Save(key, value, _es3Settings);
            Save();
        }

        public static T Get<T>(string key)
        {
            if (!isInitialized)
                InitializeSave();

            if (!HasKey(key))
                return default;

            return ES3.Load<T>(key, _es3Settings);
        }

        public static void Remove(string key)
        {
            if (!isInitialized)
                InitializeSave();

            if (!HasKey(key))
                return;

            ES3.DeleteKey(key, _es3Settings);
        }
    }
}