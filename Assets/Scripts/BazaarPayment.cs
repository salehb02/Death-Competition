#if BAZAAR_SUPPORTED
using Bazaar.Data;
using Bazaar.Poolakey;
using Bazaar.Poolakey.Data;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using DeathMatch;

#if MYKET_SUPPORTED
using MyketPlugin;
#endif

public class BazaarPayment : MonoBehaviour
{
#if BAZAAR_SUPPORTED
    private Payment payment;
    private const string CAFE_RSA_KEY = "MIHNMA0GCSqGSIb3DQEBAQUAA4G7ADCBtwKBrwDAtJ+C0sL4LuDpcydERDELa0cCoB7uo7+Z5QBu35EyqE6LEXLW0QfaMAvTA0owkYf7BiYPm79gXq2JIHC8LAXi3ajTHBO4jqXSUjWDgGiAjqJr+T8eAVzmK/fu4nc01lQTd0buNCIHN9jsxS4ez4rfD94zTE+ED9uKtkr5SgykzCy17SfsrR00O0J4SVNNMgT+M9HMJeF5gPlC7GNeIZ6Vp786szg/CodzJa6J4wECAwEAAQ==";
#endif

#if MYKET_SUPPORTED
    private const string MYKET_PUBLIC_KEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDhtEY06tDbwZHGCuMnl2gDic7cLUQBbQypVUUWkMs8KFxsOFfIJXl1u1uX9f2OWOhX6uvbeuvAgfVFLYPE0VPsB/PHO+a1ObDxM1b5ezvI0pdFSH9tmJjDLzO7hBqVCwIou6X/irb78O7d4JnylDpHwFTzuiS5exqTX0lyDwSg5wIDAQAB";
#endif

    private BazaarPaymentPresentor presentor;
    private PlayerInfo playerInfo;

    public event Action<MSDM.Shope> OnGetShop;

    [Obsolete]
    private void Start()
    {
        presentor = GetComponent<BazaarPaymentPresentor>();
        playerInfo = FindObjectOfType<PlayerInfo>();

        ServerConnection.Instance.GetShopItems((data) =>
        {
            OnGetShop?.Invoke(data);
            presentor.SetupItems(data);
        });

#if MYKET_SUPPORTED
        InitMyket();
#endif

#if BAZAAR_SUPPORTED
        InitBazaar();
#endif
    }

    [Obsolete]
    public void PurchaseItem(string productName, string marketName)
    {
        ServerConnection.Instance.SendMarketPayRequest(productName, marketName, (data) =>
        {
            if (!data.success)
                return;

            Debug.Log("BazaarPayment:: PurchaseItem:: Request send and data receieved. Starting market purchase...");

#if MYKET_SUPPORTED
            MyketIAB.purchaseProduct(data.result.productKey, data.result.developerPayload);
#endif

#if BAZAAR_SUPPORTED
            _ = payment.Purchase(data.result.productKey, SKUDetails.Type.inApp, OnPurchaseStart, OnPurchaseComplete, data.result.developerPayload);
#endif
        });
    }

#if MYKET_SUPPORTED
    private void InitMyket()
    {
        MyketIAB.init(MYKET_PUBLIC_KEY);
        MyketIAB.enableLogging(GameManager.Instance.PrintLogs);

        IABEventManager.purchaseSucceededEvent += OnPurchaseCompleteMyket;
        IABEventManager.purchaseFailedEvent += OnPurchaseFailedMyket;
        IABEventManager.consumePurchaseFailedEvent += OnConsumeCompleteMyket;
        IABEventManager.consumePurchaseFailedEvent += OnConsumeFailedMyket;
    }

    private void OnPurchaseFailedMyket(string result)
    {
        if (GameManager.Instance.PrintLogs)
            Debug.Log($"BazaarPayment:: PnPurchaseFailedMyket:: Bazaar purchase failed!\nMessage: {result}");
    }

    private void OnPurchaseCompleteMyket(MyketPurchase result)
    {
        if (GameManager.Instance.PrintLogs)
            Debug.Log("BazaarPayment:: OnPurchaseCompleteMyket:: Bazaar purchase successful!");

        MyketIAB.consumeProduct(result.ProductId);
        ServerConnection.Instance.SendMarketPaymentCompletion(result.DeveloperPayload, result.PurchaseToken, (data) => WealthManager.Instance.ReloadCoinsFromServer(true));
    }

    private void OnConsumeFailedMyket(string result)
    {
        if (GameManager.Instance.PrintLogs)
        {
            Debug.Log($"BazaarPayment:: OnConsumeComplete:: Consumption complete!\nMessage: {result}");
        }
    }

    private void OnConsumeCompleteMyket(string result)
    {
        if (GameManager.Instance.PrintLogs)
        {
            Debug.Log("BazaarPayment:: OnConsumeComplete:: Consumption complete!");
        }
    }

    private void OnApplicationQuit()
    {
        MyketIAB.unbindService();
    }

    private void OnDisable()
    {
        IABEventManager.purchaseSucceededEvent -= OnPurchaseCompleteMyket;
        IABEventManager.purchaseFailedEvent -= OnPurchaseFailedMyket;
        IABEventManager.consumePurchaseFailedEvent -= OnConsumeCompleteMyket;
        IABEventManager.consumePurchaseFailedEvent -= OnConsumeFailedMyket;
    }
#endif

#if BAZAAR_SUPPORTED
    private void InitBazaar()
    {
        var securityCheck = SecurityCheck.Enable(CAFE_RSA_KEY);
        var paymentConfiguration = new PaymentConfiguration(securityCheck);
        payment = new Payment(paymentConfiguration);

        ConnectAsync();
    }

    private async void ConnectAsync()
    {
        var result = await payment.Connect();

        if (GameManager.Instance.PrintLogs)
        {
            if (result.status == Status.Success)
                Debug.Log("Bazaar payment system connected!");
            else
                Debug.Log($"Bazaar payment system could not connect!\nMessage: {result.message}");
        }
    }

    private void OnPurchaseStart(Result<PurchaseInfo> result)
    {
        if (GameManager.Instance.PrintLogs)
        {
            if (result.status == Status.Success)
                Debug.Log("BazaarPayment:: OnPurchaseStart:: Bazaar purchase started!");
            else
                Debug.Log($"BazaarPayment:: OnPurchaseStart:: Bazaar purchase could not start!\nMessage: {result.message}");
        }
    }

    private void OnPurchaseComplete(Result<PurchaseInfo> result)
    {
        if (GameManager.Instance.PrintLogs)
        {
            if (result.status == Status.Success)
                Debug.Log("BazaarPayment:: OnPurchaseComplete:: Bazaar purchase successful!");
            else
                Debug.Log($"BazaarPayment:: OnPurchaseComplete:: Bazaar purchase failed!\nMessage: {result.message}");
        }

        if (result.status == Status.Success)
        {
            _ = payment.Consume(result.data.purchaseToken, OnConsumeComplete);
            ServerConnection.Instance.SendMarketPaymentCompletion(result.data.payload, result.data.purchaseToken, (data) => WealthManager.Instance.ReloadCoinsFromServer(true));
        }
    }

    private void OnConsumeComplete(Result<bool> result)
    {
        if (GameManager.Instance.PrintLogs)
        {
            if (result.status == Status.Success)
                Debug.Log("BazaarPayment:: OnConsumeComplete:: Consumption complete!");
            else
                Debug.Log($"BazaarPayment:: OnConsumeComplete:: Consumption complete!\nMessage: {result.message}");
        }
    }

    private void OnApplicationQuit()
    {
        payment.Disconnect();
    }
#endif
}