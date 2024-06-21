using DeathMatch;
using GameAnalyticsSDK;
using System;

public class PurchaseByCoin : PurchaseMethod
{
    public const string NOT_ENOUGH_COUNT = "NOT_ENOUGH_COIN";

    private string setCode;
    private double price;
    private event Action onPurchase;
    public event Action<string> onPurchaseFailed;

    public int Price { get => Convert.ToInt32(price); }

    public PurchaseByCoin(string setCode, double price, Action onPurchase)
    {
        this.setCode = setCode;
        this.price = price;
        this.onPurchase = onPurchase;
    }

    public override void Purchase()
    {
        if (!WealthManager.Instance.IsEnoughCoin(Price))
        {
            onPurchaseFailed?.Invoke(NOT_ENOUGH_COUNT);
            return;
        }

        onPurchase?.Invoke();
        WealthManager.Instance.UseCoins(Price, false);

        // GameAnalytics events
        GameAnalytics.NewDesignEvent($"PurchaseSetByCoin: SetCode={setCode}");
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", (float)price, "Style", setCode);
    }
}