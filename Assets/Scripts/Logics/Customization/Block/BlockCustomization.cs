using DeathMatch;
using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using USDM;
using static DeathMatch.PlayerCustomization;
using Random = UnityEngine.Random;

public class BlockCustomization : MonoBehaviour
{
    [SerializeField] private GameObject blockBase;
    [SerializeField] private BlockCustomizableItem[] blockStyles;
    [SerializeField] private bool isPlayer;

    public bool IsPlayer { get => isPlayer; set => isPlayer = value; }

    private List<SSDM.Set> serverStyles = new List<SSDM.Set>();

    public event Action OnLoadComplete;
    public event Action OnPurchaseCustomization;

    public void StartCustomization()
    {
        HideBase();

        if (SceneManager.GetActiveScene().name == GameManager.Instance.customizationScene)
            LoadBlock();
        else
        {
            if (isPlayer)
            {
                if (GameManager.Instance.LatestPlayerInfo == null)
                    throw new System.NullReferenceException("LatestPlayerInfo is null");

                ApplyStyleNoCheck(GameManager.Instance.LatestPlayerInfo.data.userPlatform);
            }
        }
    }

    public List<UIInfo> GetUIInfos()
    {
        var items = new List<UIInfo>();

        foreach (var style in serverStyles)
        {
            if (!IsServerSetsContains(style.code))
                continue;

            var icon = blockStyles.SingleOrDefault(x => x.name == style.code).GetIcon();
            var info = new UIInfo(style.code, icon, style);
            items.Add(info);
        }

        return items;
    }

    public void LoadRandomStyle()
    {
        DisableAllParts();

        blockStyles[Random.Range(0, blockStyles.Length)].gameObject.SetActive(true);

        ShowBase();
    }

    public GSDM.UserSet GetRandomSetModel()
    {
        var model = new GSDM.UserSet();
        model.code = blockStyles[Random.Range(0, blockStyles.Length)].name;

        return model;
    }

    public void PreviewStyle(string setCode, bool invokeAction = true)
    {
        HideBase();
        DisableAllParts();

        var currentStyle = blockStyles.SingleOrDefault(x => x.name == setCode);

        currentStyle.gameObject.SetActive(true);

        if (invokeAction)
            OnPurchaseCustomization?.Invoke();

        ShowBase();
    }

    private void DisableAllParts()
    {
        foreach (var part in blockStyles)
            part.gameObject.SetActive(false);

        LoadingCharacterIndicator.Instance.ShowLoadingIndicator(blockBase.transform);
    }

    public void HideBase()
    {
        blockBase.SetActive(false);
    }

    private void ShowBase()
    {
        blockBase.SetActive(true);
        LoadingCharacterIndicator.Instance.HideLoadingIndicator();
    }

    public void ApplyStyleNoCheck(SetModel set)
    {
        if (set == null)
            return;

        DisableAllParts();

        var selectedStyle = blockStyles.SingleOrDefault(x => x.name == set.code);

        if (selectedStyle == null)
            return;

        selectedStyle.gameObject.SetActive(true);

        ShowBase();
    }

    public void ApplyStyles(List<SSDM.Set> allStyles)
    {
        DisableAllParts();

        foreach (var style in allStyles)
        {
            if (!IsStylePurchased(style.code))
                continue;

            // Load styles
            var selectedStyle = blockStyles.SingleOrDefault(x => x.name == style.code);

            if (selectedStyle == null)
                continue;

            selectedStyle.gameObject.SetActive(IsStyleSelected(style.code));
        }

        ShowBase();
    }

    public void SelectStyle(string setCode)
    {
        ServerConnection.Instance.UpdatePlayerSet(setCode, OnUpdateBlockStyle);

        // GameAnalytics events
        GameAnalytics.NewDesignEvent($"SelectBlockSet:{setCode}");
    }

    private void OnUpdateBlockStyle(List<SSDM.Set> sets)
    {
        LoadBlock();

        if (GameManager.Instance.PrintLogs)
            Debug.Log("Player block style updated successfuly!");
    }

    public PurchaseMethod GetStylePurchaseMethod(string setCode)
    {
        if (!IsServerSetsContains(setCode))
            throw new NullReferenceException();

        var set = serverStyles.SingleOrDefault(x => x.code == setCode);

        if (set.adPurchase > 0)
        {
            return new PurchaseByAd(setCode, set.adWatched, set.adPurchase, () => WatchAdForBlockSet(setCode));
        }
        else if (set.virtualPayment.active)
        {
            return new PurchaseByCoin(setCode, set.virtualPayment.price, () => PurchaseBlockSet(setCode));
        }

        throw new Exception("No purchase method available");
    }

    [Obsolete]
    public void WatchAdForBlockSet(string setCode, bool invokeAction = true)
    {
        ServerConnection.Instance.WatchAdForSet(setCode, (data) =>
        {
            if (!data.success)
                return;

            SelectStyle(setCode);

            if (invokeAction)
                OnPurchaseCustomization?.Invoke();
        });
    }

    [Obsolete]
    public void PurchaseBlockSet(string setCode, bool invokeAction = true)
    {
        ServerConnection.Instance.PurchaseSet(setCode, (data) =>
        {
            if (!data.success)
                return;

            SelectStyle(setCode);

            if (invokeAction)
                OnPurchaseCustomization?.Invoke();
        });
    }

    public bool IsStyleSelected(string setCode)
    {
        if (!IsServerSetsContains(setCode))
            return false;

        return serverStyles.SingleOrDefault(x => x.code == setCode).chosen;
    }

    private bool IsServerSetsContains(string setCode)
    {
        if (!blockStyles.Contains(blockStyles.SingleOrDefault(x => x.name == setCode)))
            return false;

        return serverStyles.Contains(serverStyles.SingleOrDefault(x => x.code == setCode));
    }

    public bool IsStyleLocked(string setCode, out int neededLevel)
    {
        if (!IsServerSetsContains(setCode))
            throw new NullReferenceException("Block style not exist");

        var currentServerStyle = serverStyles.SingleOrDefault(x => x.code == setCode);

        neededLevel = currentServerStyle.level;
        return currentServerStyle.locked;
    }

    public bool IsStylePurchased(string setCode)
    {
        if (!IsServerSetsContains(setCode))
            return false;

        return serverStyles.SingleOrDefault(x => x.code == setCode).bought;
    }

    private void LoadBlock()
    {
        HideBase();

        LoadingCharacterIndicator.Instance.ShowLoadingIndicator(blockBase.transform);

        ServerConnection.Instance.GetAllSets(OnLoadStyles);

        void OnLoadStyles(List<SSDM.Set> styles)
        {
            serverStyles = styles.Where(x => x.category == "pla").ToList();

            ApplyStyles(serverStyles);
            OnLoadComplete?.Invoke();
        }
    }

    public void ResetPreview()
    {
        ApplyStyles(serverStyles);
    }
}