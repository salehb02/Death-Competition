using GameAnalyticsSDK;
using UnityEngine.SceneManagement;

namespace DeathMatch
{
    public static class LevelLoader
    {
        public static void LoadLevel(string name)
        {
            // GA
            GameAnalytics.NewDesignEvent($"LoadScene: {name}");

            SceneManager.LoadScene(name);
            AudioManager.Instance.ClickButtonSFX();
        }

        public static void LoadLevelSilence(string name)
        {
            // GA
            GameAnalytics.NewDesignEvent($"LoadScene: {name}");

            SceneManager.LoadScene(name);
        }
    }
}