using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopOffer : MonoBehaviour
{
    [SerializeField] private Image setImage;
    [SerializeField] private TextMeshProUGUI coinAmountText;
    [SerializeField] private TextMeshProUGUI fakePriceText;
    [SerializeField] private TextMeshProUGUI realPriceText;
    [SerializeField] private TextMeshProUGUI discountText;

    private Button button;
    private BazaarPayment shopHandler;

    public void Setup(MSDM.Package package, BazaarPayment payment)
    {
        button = GetComponent<Button>();
        shopHandler = payment;

        coinAmountText.text = package.coinReward.ToString("#,#");
        fakePriceText.text = $"{package.fakePayment.cost.ToString("#,#")} تومان";
        realPriceText.text = $"{package.payment.cost.ToString("#,#")} تومان";

        discountText.text = $"{100 - Mathf.FloorToInt(Convert.ToSingle(100 * package.payment.cost / package.fakePayment.cost))}%";
        setImage.sprite = GameManager.Instance.GetCustomizationSetSprite(package.sets[0].code);

        button.onClick.AddListener(() =>
        {
            // GA
            GameAnalyticsSDK.GameAnalytics.NewDesignEvent($"ShopItem: {package.name}_Clicked");

            shopHandler.PurchaseItem(package.payment.productName, package.payment.market);
        });
    }
}