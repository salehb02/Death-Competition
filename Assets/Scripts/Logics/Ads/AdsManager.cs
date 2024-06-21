using DeathMatch;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AdsManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Button bottomPrefab;
    [SerializeField] private Button mediumPrefab;
    [SerializeField] private Button fullscreenPrefab;

    [Header("Placements")]
    [SerializeField] private Transform settingsPanel;
    [SerializeField] private Transform leaderboardPanel;
    [SerializeField] private Transform shopPanel;
    [SerializeField] private Transform exitPanel;
    [SerializeField] private Transform fullScreenBannerHolder;

    private TutorialSteps TutorialManager;
    private List<Button> currentAds = new List<Button>();

    private void Start()
    {
        TutorialManager = FindObjectOfType<TutorialSteps>();

        if (TutorialManager.TutorialMode)
            return;

        LoadAds();
    }

    private void LoadAds()
    {
        foreach (var ad in currentAds)
            Destroy(ad.gameObject);

        ServerConnection.Instance.GetAds("Bottom", 3, OnGetBottomAds);
        ServerConnection.Instance.GetAds("Medium", 1, OnGetMediumAds);
    }

    [Obsolete]
    private void OnGetBottomAds(MSDM.Ads ads)
    {
        if (ads.ads.Count == 0)
            return;

        var settingsAd = ads.ads[Random.Range(0, ads.ads.Count)];
        var settingsBottom = Instantiate(bottomPrefab, settingsPanel);
        settingsBottom.onClick.AddListener(() => ServerConnection.Instance.ClickAd(settingsAd.id, null));
        settingsBottom.onClick.AddListener(() => Application.OpenURL(settingsAd.url));
        settingsBottom.onClick.AddListener(LoadAds);
        settingsBottom.gameObject.SetActive(false);

        ServerConnection.Instance.DownloadImage(settingsAd.img, (img) =>
        {
            if (settingsBottom == null)
                return;

            settingsBottom.image.sprite = img;
            settingsBottom.gameObject.SetActive(true);
        });

        currentAds.Add(settingsBottom);

        var leaderboardAd = ads.ads[Random.Range(0, ads.ads.Count)];
        var leaderboardBottom = Instantiate(bottomPrefab, leaderboardPanel);
        leaderboardBottom.onClick.AddListener(() => ServerConnection.Instance.ClickAd(leaderboardAd.id, null));
        leaderboardBottom.onClick.AddListener(() => Application.OpenURL(leaderboardAd.url));
        leaderboardBottom.onClick.AddListener(LoadAds);
        leaderboardBottom.gameObject.SetActive(false);

        ServerConnection.Instance.DownloadImage(leaderboardAd.img, (img) =>
        {
            if(leaderboardBottom == null) 
                return;

            leaderboardBottom.image.sprite = img;
            leaderboardBottom.gameObject.SetActive(true);
        });

        currentAds.Add(leaderboardBottom);

        var shopAd = ads.ads[Random.Range(0, ads.ads.Count)];
        var shopBottom = Instantiate(bottomPrefab, shopPanel);
        shopBottom.onClick.AddListener(() => ServerConnection.Instance.ClickAd(shopAd.id, null));
        shopBottom.onClick.AddListener(() => Application.OpenURL(shopAd.url));
        shopBottom.onClick.AddListener(LoadAds);
        shopBottom.gameObject.SetActive(false);

        ServerConnection.Instance.DownloadImage(shopAd.img, (img) =>
        {
            if (shopBottom == null)
                return;
            
            shopBottom.image.sprite = img;
            shopBottom.gameObject.SetActive(true);
        });

        currentAds.Add(shopBottom);
    }

    [Obsolete]
    private  void OnGetMediumAds(MSDM.Ads ads)
    {
        if (ads.ads.Count == 0)
            return;

        var exitAd = ads.ads[Random.Range(0, ads.ads.Count)];
        var exitMedium = Instantiate(mediumPrefab, exitPanel);
        exitMedium.onClick.AddListener(() => ServerConnection.Instance.ClickAd(exitAd.id, null));
        exitMedium.onClick.AddListener(() => Application.OpenURL(exitAd.url));
        exitMedium.onClick.AddListener(LoadAds);
        exitMedium.transform.SetAsFirstSibling();
        exitMedium.gameObject.SetActive(false);

        ServerConnection.Instance.DownloadImage(exitAd.img, (img) =>
        {
            if (exitMedium == null)
                return;

            exitMedium.image.sprite = img;
            exitMedium.gameObject.SetActive(true);
        });

        currentAds.Add(exitMedium);
    }

    [Obsolete]
    public void ShowFullscreenAd(Action onFail, Action onSuccess)
    {
        ServerConnection.Instance.GetAds("Big", 1, (ads) =>
        {
            if (ads.ads.Count == 0) {
                onFail?.Invoke();
                return;
            }

            var fullscreenAd = ads.ads[0];
            var fullscreenBanner = Instantiate(fullscreenPrefab, fullScreenBannerHolder);
            fullscreenBanner.onClick.AddListener(() => ServerConnection.Instance.ClickAd(fullscreenAd.id, null));
            fullscreenBanner.onClick.AddListener(() => Application.OpenURL(fullscreenAd.url));
            fullscreenBanner.onClick.AddListener(() =>
            {
                Destroy(fullscreenBanner.gameObject);
            });

            var closeButton = fullscreenBanner.transform.Find("CLOSE").GetComponent<Button>();
            closeButton.gameObject.SetActive(false);
            closeButton.onClick.AddListener(() => onSuccess?.Invoke());
            closeButton.onClick.AddListener(() =>
            {
                Destroy(fullscreenBanner.gameObject);
            });

            StartCoroutine(EnableObjectCoroutine(10, closeButton.gameObject));

            ServerConnection.Instance.DownloadImage(fullscreenAd.img, (img) =>
            {
                if (fullscreenBanner == null)
                    return;

                fullscreenBanner.image.sprite = img;
            });
        });
    }

    private IEnumerator EnableObjectCoroutine(float timer,  GameObject obj)
    {
        yield return new WaitForSeconds(timer);
        obj.SetActive(true);
    }
}