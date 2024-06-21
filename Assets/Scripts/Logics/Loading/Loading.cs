using DeathMatch;
using GameAnalyticsSDK;
using System.Collections;
using UnityEngine;

public class Loading : MonoBehaviour
{
    private LoadingPresentor presentor;
    private RegisterGuest registerGuest;
    private CheckVersionClient checkVersion;
    private bool isWaitingToCheckVersion;

    private void Start()
    {
        presentor = GetComponent<LoadingPresentor>();
        registerGuest = FindObjectOfType<RegisterGuest>();
        checkVersion = FindObjectOfType<CheckVersionClient>();

        StartCoroutine(LoadingCoroutine());
    }

    [System.Obsolete]
    private IEnumerator LoadingCoroutine()
    {
        // GA
        GameAnalytics.NewDesignEvent("Loading_Start");

        /// Init progress bar
        var progress = 0f;
        presentor.SetBarValue(progress);
        ///

        if (SaveManager.Get<bool>(InitScene.CHECK_SERVER_PREFS))
        {
            SaveManager.Set(InitScene.CHECK_SERVER_PREFS, false);

            /// Check server status
            var isServerOK = true;
            var isCheckingServerStatus = true;

            ServerConnection.Instance.GetServerStatues((data) =>
            {
                if (data.reportState == true)
                    isServerOK = false;

                isCheckingServerStatus = false;
            });

            presentor.SetLoadingSituation("بررسی وضعیت سرور");

            yield return new WaitWhile(() => isCheckingServerStatus);

            if (!isServerOK)
                yield break;
            ///

            /// Check version
            var isVersionChecked = false;
            isWaitingToCheckVersion = false;

            ServerConnection.Instance.CheckVersion((check) =>
            {
                isWaitingToCheckVersion = check.updateNeed;
                checkVersion.ShowUpdatePanel(check);
                isVersionChecked = true;
            });

            presentor.SetLoadingSituation("بررسی نسخه");

            yield return new WaitUntil(() => isVersionChecked == true);
            yield return new WaitUntil(() => isWaitingToCheckVersion == false);
            ///
        }

        /// Create guest user if needed
        registerGuest.Register();
        presentor.SetLoadingSituation("ایجاد حساب مهمان");
        yield return new WaitWhile(() => registerGuest.WaitingForGuestCreation == true);
        ///

        /// Load player info
        GameManager.Instance.LatestPlayerInfo = null;

        ServerConnection.Instance.GetPlayerInfo((date) =>
        {
            if (GameManager.Instance.LatestPlayerInfo != null)
                return;

            GameManager.Instance.LatestPlayerInfo = date;

            progress += 0.5f;
            presentor.SetBarValue(progress);
        });

        presentor.SetLoadingSituation("دریافت اطلاعات کاربر");

        yield return new WaitWhile(() => GameManager.Instance.LatestPlayerInfo == null);
        ///

        /// Add some juicy final percents
        yield return new WaitForSeconds(0.5f);

        presentor.SetLoadingSituation("ورود به بازی");

        progress += 0.5f;
        progress = Mathf.Clamp01(progress);
        presentor.SetBarValue(progress);

        yield return new WaitForSeconds(0.7f);
        ///

        // GA
        GameAnalytics.NewDesignEvent("Loading_End");

        /// Load menu
        LevelLoader.LoadLevelSilence(GameManager.Instance.mainMenuScene);
        ///
    }

    public void PassCheckVersion()
    {
        isWaitingToCheckVersion = false;
    }
}