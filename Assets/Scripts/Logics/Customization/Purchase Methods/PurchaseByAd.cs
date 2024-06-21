using GameAnalyticsSDK;
using System;

public class PurchaseByAd : PurchaseMethod
{
    private string setCode;
    private int watched;
    private int toWatch;
    private event Action onPurchase;

    public int Watched { get => watched; }
    public int ToWatch { get => toWatch; }

    public PurchaseByAd(string setCode, int watched, int toWatch, Action onPurchase)
    {
        this.setCode = setCode;
        this.watched = watched;
        this.toWatch = toWatch;
        this.onPurchase = onPurchase;
    }

    public override void Purchase()
    {
        TapsellManager.Instance.RequestRewarded("64e19358f3ef8d186696309d", () =>
        {
            TapsellManager.Instance.ShowRewarded(() =>
            {
                onPurchase?.Invoke();

                // GameAnalytics events
                GameAnalytics.NewDesignEvent($"WatchAdForSet: SetCode={setCode}");
            });
        });
    }
}