using DeathMatch;
using MSDM;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterPurchase : MonoBehaviour
{
    [SerializeField] private BoosterItem[] boosterItems;

    [Header("Purchase Confirm Panel")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Image boosterIcon;
    [SerializeField] private TextMeshProUGUI boosterTitle;
    [SerializeField] private TextMeshProUGUI boosterCount;
    [SerializeField] private TextMeshProUGUI boosterPrice;
    [SerializeField] private GameObject notEnoughMoneyText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeConfirmationPanelButton;

    private BoosterSelection boosterSelection;
    private bool isTransactionPending;
    private int? confirmationPendingBoosterId;
    private int pendingBoosterPrice;

    [Serializable]
    public class BoosterItem
    {
        public TextMeshProUGUI PriceText;
        public Button PurchaseButton;
        public Sprite Icon;
        public string Title;
    }

    private void Start()
    {
        boosterSelection = FindObjectOfType<BoosterSelection>();

        CloseConfirmationPanel();

        confirmButton.onClick.AddListener(ConfirmPurchase);

        closeConfirmationPanelButton.onClick.AddListener(CloseConfirmationPanel);
        closeConfirmationPanelButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    public void LoadBoosters(MSDM.Boosters boosters)
    {
        for (int i = 0; i < boosterItems.Length; i++)
        {
            boosterItems[i].PriceText.text = boosters.boosters[i].virtualPayment.price.ToString("#,#");

            var index = i;
            boosterItems[i].PurchaseButton.onClick.RemoveAllListeners();
            boosterItems[i].PurchaseButton.onClick.AddListener(() => PurchaseBooster(boosterItems[index], boosters.boosters[index]));
            boosterItems[i].PurchaseButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }
    }

    private void PurchaseBooster(BoosterItem info, Booster booster)
    {
        OpenConfirmationPanel(info.Icon, info.Title, 10, (int)booster.virtualPayment.price, booster.id);
    }

    private void OpenConfirmationPanel(Sprite boosterIcon, string boosterTitle, int count, int price, int boosterId)
    {
        confirmationPendingBoosterId = boosterId;
        pendingBoosterPrice = price;

        this.boosterIcon.sprite = boosterIcon;
        this.boosterTitle.text = boosterTitle;
        boosterPrice.text = price.ToString("#,#");
        boosterCount.text = $"x{count}";

        notEnoughMoneyText.SetActive(false);
        confirmPanel.SetActive(true);
    }

    private void CloseConfirmationPanel()
    {
        confirmPanel.SetActive(false);
        confirmationPendingBoosterId = null;
    }

    private void ConfirmPurchase()
    {
        if (isTransactionPending)
            return;

        if (!confirmationPendingBoosterId.HasValue)
            return;

        if (!WealthManager.Instance.IsEnoughCoin(pendingBoosterPrice))
        {
            notEnoughMoneyText.SetActive(true);
            AudioManager.Instance.ErrorSFX();
            return;
        }

        AudioManager.Instance.MoneyDecreaseSFX();

        isTransactionPending = true;

        ServerConnection.Instance.BuyBooster(confirmationPendingBoosterId.Value, (boosters) =>
        {
            boosterSelection.LoadAllBoosters(boosters);
            isTransactionPending = false;
            CloseConfirmationPanel();
        });
    }
}