using GameAnalyticsSDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BazaarPaymentPresentor : MonoBehaviour
{
    [Header("Special Offers")]
    [SerializeField] private Transform specialOffersHolder;
    [SerializeField] private ShopOffer specialOfferPrefab;
    [SerializeField] private TextMeshProUGUI specialOfferExpireTimeText;

    [Space(2)]
    [SerializeField] private Item[] items;

    private BazaarPayment payment;

    [System.Serializable]
    public class Item
    {
        public string Title;

        [Space(2)]
        [Header("UI")]
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI PriceText;
        public Button Btn;
    }

    private void Start()
    {
        specialOfferExpireTimeText.text = $"اعتبار تا: <sprite index=0>";
    }

    public void SetupItems(MSDM.Shope shopItems)
    {
        payment = GetComponent<BazaarPayment>();

        // Load special offers
        foreach (Transform obj in specialOffersHolder)
            Destroy(obj.gameObject);

        for (int i = 0; i < shopItems.packages.Count; i++)
        {
            var offerInstance = Instantiate(specialOfferPrefab, specialOffersHolder);
            offerInstance.Setup(shopItems.packages[i], payment);

            if (shopItems.packages[i].expierdDate.Hour != 0)
                specialOfferExpireTimeText.text = $"اعتبار تا: {shopItems.packages[i].expierdDate.Day} روز و {shopItems.packages[i].expierdDate.Hour} ساعت";
            else
                specialOfferExpireTimeText.text = $"اعتبار تا: {shopItems.packages[i].expierdDate.Day} روز";
        }

        // Load coins
        for (int i = 0; i < items.Length; i++)
        {
            if (i > shopItems.coins.Count - 1 )
                continue;

            var index = i;

            items[i].TitleText.text = $"{shopItems.coins[i].coinReward} سکه";
            items[i].PriceText.text = $"{shopItems.coins[i].payment.cost.ToString("#,#")}\nتومان";
            items[index].Btn.onClick.AddListener(() =>
            {
                // GA 
                GameAnalytics.NewDesignEvent($"ShopItem: {shopItems.coins[index].name}_Clicked");

                payment.PurchaseItem(shopItems.coins[index].payment.productName, shopItems.coins[index].payment.market);
            });
        }
    }
}
