using DG.Tweening;
using System;
using TapsellPlusSDK;
using TapsellPlusSDK.models;
using TMPro;
using UnityEngine;

public class TapsellManager : MonoBehaviour
{
    #region Singleton
    public static TapsellManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(null);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [SerializeField] private TextMeshProUGUI errorText;

    public bool IsInitialized { get; private set; }

    private const string TAPSELL_KEY = "mlsjqinllcnomerkgnnknhbiegbaoskdelrifbbjjgtjaeqnrcijhnicoegshtmmrkinol";
    private const int MAX_TRY_COUNT = 3;

    private string rewardedResponseId;
    private int currentTry;

    private void Start()
    {
        errorText.color = new Color(1, 1, 1, 0);

        var key = string.Empty;

        if (GameManager.Instance.TestAds)
            key = "alsoatsrtrotpqacegkehkaiieckldhrgsbspqtgqnbrrfccrtbdomgjtahflchkqtqosa";
        else
            key = TAPSELL_KEY;

        TapsellPlus.Initialize(key, OnInitializeSuccess, OnInitializeFail);
    }

    private void OnInitializeSuccess(string message)
    {
        if (GameManager.Instance.PrintLogs)
            Debug.Log(message);

        TapsellPlus.SetGdprConsent(true);
        IsInitialized = true;
    }

    private void OnInitializeFail(TapsellPlusAdNetworkError error)
    {
        if (GameManager.Instance.PrintLogs)
            Debug.Log(error.ToString());
    }

    public void ShowUIError()
    {
        errorText.DOColor(Color.white, 0);
        errorText.DOColor(new Color(1, 1, 1, 0), 2);
    }

    public bool IsRewardedAdAvailable()
    {
        return rewardedResponseId != null;
    }

    public void RequestRewarded(string zone, Action onSuccess = null)
    {
        if (!IsInitialized)
            return;

        var zoneId = string.Empty;

        if (GameManager.Instance.TestAds)
            zoneId = "5cfaa802e8d17f0001ffb28e";
        else
            zoneId = zone;

        TapsellPlus.RequestRewardedVideoAd(zoneId,

                  tapsellPlusAdModel =>
                  {
                      if (GameManager.Instance.PrintLogs)
                          Debug.Log("ReviveAd::Request:: ad loaded. responseId = " + tapsellPlusAdModel.responseId);

                      rewardedResponseId = tapsellPlusAdModel.responseId;

                      if (onSuccess != null)
                          onSuccess?.Invoke();
                  },
                  error =>
                  {
                      if (GameManager.Instance.PrintLogs)
                          Debug.Log("ReviveAd::Request:: error= " + error.message + "\nRetrying...");

                      if (currentTry < MAX_TRY_COUNT)
                      {
                          Invoke(nameof(RequestRewarded), 1f);
                          currentTry++;
                      }
                  }
              );
    }

    public void ShowRewarded(Action onReward)
    {
        if (!IsRewardedAdAvailable())
            return;

        TapsellPlus.ShowRewardedVideoAd(rewardedResponseId,
                  tapsellPlusAdModel =>
                  {
                      if (GameManager.Instance.PrintLogs)
                          Debug.Log("ReviveAd::Show:: ad opened. " + tapsellPlusAdModel.zoneId);
                  },
                  tapsellPlusAdModel =>
                  {
                      if (GameManager.Instance.PrintLogs)
                          Debug.Log("ReviveAd::Show:: ad rewarded. " + tapsellPlusAdModel.zoneId);

                      onReward?.Invoke();
                  },
                  tapsellPlusAdModel =>
                  {
                      if (GameManager.Instance.PrintLogs)
                          Debug.Log("ReviveAd::Show:: ad closed. " + tapsellPlusAdModel.zoneId);
                  },
                  error =>
                  {
                      if (GameManager.Instance.PrintLogs)
                          Debug.Log("ReviveAd::Show:: ad error. " + error.errorMessage);
                  }
              );
    }
}