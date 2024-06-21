using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using GameAnalyticsSDK;
using System;

namespace DeathMatch
{
    public class CustomizationUI : MonoBehaviour
    {
        [SerializeField] private GameObject loadingDataText;

        [Header("Skins To Buy")]
        [SerializeField] private Transform vipItemsHolder;
        [SerializeField] private Transform epicItemsHolder;
        [SerializeField] private Transform rareItemsHolder;
        [SerializeField] private ItemButton partPrefab;
        [SerializeField] private RectTransform forceRebuildLayout;

        [Header("Blocks To Buy")]
        [SerializeField] private Transform vipItemsHolderB;
        [SerializeField] private Transform epicItemsHolderB;
        [SerializeField] private Transform rareItemsHolderB;

        [Header("Purchased Skins")]
        [SerializeField] private Transform vipItemsHolderPS;
        [SerializeField] private Transform epicItemsHolderPS;
        [SerializeField] private Transform rareItemsHolderPS;

        [Header("Player Main Info")]
        [SerializeField] private Button changeGender;
        [SerializeField] private Sprite maleSprite;
        [SerializeField] private Sprite femaleSprite;

        [Header("Buttons")]
        [SerializeField] private Button purchaseByCoin;
        //[SerializeField] private TextMeshProUGUI purchaseByCoinText;
        [SerializeField] private Button purchaseByAd;
        //[SerializeField] private TextMeshProUGUI purchaseByAdText;
        [SerializeField] private GameObject unlockAsYouPlay;
        [SerializeField] private Button selectStyle;

        [Header("Main")]
        [SerializeField] private Button exitToMenu;
        [SerializeField] private TextMeshProUGUI lockText;

        [Header("Change Gender Panel")]
        [SerializeField] private GameObject ChangeGenderPanel;
        [SerializeField] private GameObject NoCreditText;
        [SerializeField] private Button ChangeGenderAccept;
        [SerializeField] private Button ChangeGenderCancel;
        [SerializeField] private TextMeshProUGUI ChangeGenderPriceText;

        [Header("Not Enough Coin")]
        [SerializeField] private GameObject notEnoughCoinPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button toShopButton;

        private Customizer customizer;
        private int changeGenderPrice;

        private void OnEnable()
        {
            customizer = FindObjectOfType<Customizer>();

            customizer.Avatar.OnLoadComplete += InitializeUI;
            customizer.Block.OnLoadComplete += LoadBlockSets;

            customizer.Avatar.OnPurchaseCustomization += LoadAvatarSets;
            customizer.Block.OnPurchaseCustomization += LoadBlockSets;
        }

        private void OnDisable()
        {
            customizer.Avatar.OnLoadComplete -= InitializeUI;
            customizer.Block.OnLoadComplete -= LoadBlockSets;

            customizer.Avatar.OnPurchaseCustomization -= LoadAvatarSets;
            customizer.Block.OnPurchaseCustomization -= LoadBlockSets;
        }

        private void Start()
        {
            ChangeGenderPriceText.text = ServerConnection.LOADING_DATA_TEXT;
            loadingDataText.SetActive(true);

            GetChangeGenderPrice();
            HideLock();
            InitButtons();
            CloseChangeGenderPanel();
            CloseNotEnoughCoin();
            OnChangeTab();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                BackToMenu();
        }

        private void InitializeUI()
        {
            LoadAvatarSets();
            SetGenderSprite(customizer.Avatar.Gender);

            loadingDataText.SetActive(false);
        }

        private void InitButtons()
        {
            exitToMenu.onClick.AddListener(BackToMenu);
            exitToMenu.onClick.AddListener(AudioManager.Instance.SettingPopUpSFX);

            changeGender.onClick.AddListener(OpenChangeGenderPanel);
            changeGender.onClick.AddListener(AudioManager.Instance.SettingPopUpSFX);

            ChangeGenderAccept.onClick.AddListener(OnAcceptChangeGender);
            ChangeGenderAccept.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

            ChangeGenderCancel.onClick.AddListener(CloseChangeGenderPanel);
            ChangeGenderCancel.onClick.AddListener(AudioManager.Instance.SettingCloseSFX);

            closeButton.onClick.AddListener(CloseNotEnoughCoin);
            closeButton.onClick.AddListener(AudioManager.Instance.SettingCloseSFX);

            toShopButton.onClick.AddListener(ToShop);
            toShopButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }

        [Obsolete]
        private void GetChangeGenderPrice()
        {
            changeGender.interactable = false;

            ServerConnection.Instance.GetChangeGenderPrice((price) =>
            {
                changeGenderPrice = price;
                ChangeGenderPriceText.text = price == 0 ? price.ToString() : price.ToString("#,#");
                changeGender.interactable = true;
            });
        }

        private void ChangeGender()
        {
            customizer.Avatar.ChangeGender();
            customizer.Avatar.UpdateCustomization();
            LoadAvatarSets();
            SetGenderSprite(customizer.Avatar.Gender);

            // GameAnalytics events
            GameAnalytics.NewDesignEvent("ShopChangeGenderBTN", 1f);
        }

        private void OnAcceptChangeGender()
        {
            if (WealthManager.Instance.IsEnoughCoin(changeGenderPrice))
            {
                ChangeGender();
                CloseChangeGenderPanel();
            }
            else
            {
                NoCreditText.SetActive(true);
            }
        }

        public void SetGenderSprite(Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    changeGender.image.sprite = maleSprite;
                    break;
                case Gender.Female:
                    changeGender.image.sprite = femaleSprite;
                    break;
                default:
                    break;
            }
        }

        private void OpenChangeGenderPanel()
        {
            ChangeGenderPanel.SetActive(true);
            NoCreditText.SetActive(false);
        }

        private void CloseChangeGenderPanel()
        {
            ChangeGenderPanel.SetActive(false);
        }

        private void LoadAvatarSets()
        {
            foreach (Transform child in vipItemsHolder)
                Destroy(child.gameObject);

            foreach (Transform child in epicItemsHolder)
                Destroy(child.gameObject);

            foreach (Transform child in rareItemsHolder)
                Destroy(child.gameObject);

            foreach (Transform child in vipItemsHolderPS)
                Destroy(child.gameObject);

            foreach (Transform child in epicItemsHolderPS)
                Destroy(child.gameObject);

            foreach (Transform child in rareItemsHolderPS)
                Destroy(child.gameObject);

            var allInfos = customizer.Avatar.GetUIInfos();

            // All Skins
            foreach (var info in allInfos.OrderBy(x => !customizer.Avatar.IsStylePurchased(x.Code)).ToList())
            {
                var holder = rareItemsHolder;
                var itemName = info.Style.code.ToLower();

                if (itemName.Contains("vip"))
                    holder = vipItemsHolder;
                else if (itemName.Contains("epic"))
                    holder = epicItemsHolder;
                else if (itemName.Contains("rare"))
                    holder = rareItemsHolder;

                var partButton = Instantiate(partPrefab, holder);
                partButton.Setup(info, () => customizer.PreviewAvatarSet(info.Code));
            }

            var purchasedAllInfos = allInfos.Where(x => customizer.Avatar.IsStylePurchased(x.Code));

            // Purchased Skins
            foreach (var info in purchasedAllInfos.OrderBy(x => !customizer.Avatar.IsStylePurchased(x.Code)).ToList())
            {
                var holder = rareItemsHolderPS;
                var itemName = info.Style.code.ToLower();

                if (itemName.Contains("vip"))
                    holder = vipItemsHolderPS;
                else if (itemName.Contains("epic"))
                    holder = epicItemsHolderPS;
                else if (itemName.Contains("rare"))
                    holder = rareItemsHolderPS;

                var partButton = Instantiate(partPrefab, holder);
                partButton.Setup(info, () => customizer.PreviewAvatarSet(info.Code));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(forceRebuildLayout);
        }

        private void LoadBlockSets()
        {
            foreach (Transform child in vipItemsHolderB)
                Destroy(child.gameObject);

            foreach (Transform child in epicItemsHolderB)
                Destroy(child.gameObject);

            foreach (Transform child in rareItemsHolderB)
                Destroy(child.gameObject);

            var allInfos = customizer.Block.GetUIInfos();

            // All Skins
            foreach (var info in allInfos.OrderBy(x => !customizer.Block.IsStylePurchased(x.Code)).ToList())
            {
                var holder = rareItemsHolderB;
                var itemName = info.Style.code.ToLower();

                if (itemName.Contains("vip"))
                    holder = vipItemsHolderB;
                else if (itemName.Contains("epic"))
                    holder = epicItemsHolderB;
                else if (itemName.Contains("rare"))
                    holder = rareItemsHolderB;

                var partButton = Instantiate(partPrefab, holder);
                partButton.Setup(info, () => customizer.PreviewBlockSet(info.Code));
            }
        }

        private void BackToMenu()
        {
            if (GameManager.Instance.LatestPlayerInfo == null)
                LevelLoader.LoadLevel(GameManager.Instance.loadingScene);
            else
                LevelLoader.LoadLevel(GameManager.Instance.mainMenuScene);
        }

        public void ShowLock(int level)
        {
            lockText.text = $"سطح مورد نیاز: {level}";
            lockText.gameObject.SetActive(true);

            DisableAllPurchaseButtons();

            unlockAsYouPlay.gameObject.SetActive(true);
        }

        public void HideLock()
        {
            lockText.gameObject.SetActive(false);
            unlockAsYouPlay.gameObject.SetActive(false);
        }

        public void DisableAllPurchaseButtons()
        {
            unlockAsYouPlay.SetActive(false);
            selectStyle.gameObject.SetActive(false);
            purchaseByAd.gameObject.SetActive(false);
            purchaseByCoin.gameObject.SetActive(false);
        }

        public void SetPurchaseByCoin(int price, Action onClick)
        {
            DisableAllPurchaseButtons();

            purchaseByCoin.gameObject.SetActive(true);
            // purchaseByCoinText.text = price.ToString();

            purchaseByCoin.onClick.RemoveAllListeners();
            purchaseByCoin.onClick.AddListener(() => onClick?.Invoke());
            purchaseByCoin.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }

        public void SetPurchaseByAd(int watched, int toWatch, Action onClick)
        {
            DisableAllPurchaseButtons();

            purchaseByAd.gameObject.SetActive(true);
            // purchaseByAdText.text = $"{watched}/{toWatch}";

            purchaseByAd.onClick.RemoveAllListeners();
            purchaseByAd.onClick.AddListener(() => onClick?.Invoke());
            purchaseByAd.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }

        public void SetSelectStyle(Action onClick)
        {
            DisableAllPurchaseButtons();

            selectStyle.interactable = true;
            selectStyle.gameObject.SetActive(true);
            selectStyle.onClick.RemoveAllListeners();
            selectStyle.onClick.AddListener(() => onClick?.Invoke());
            selectStyle.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
        }

        public void OpenNotEnoughCoinPanel()
        {
            notEnoughCoinPanel.SetActive(true);
        }

        private void CloseNotEnoughCoin()
        {
            notEnoughCoinPanel.SetActive(false);
        }

        private void ToShop()
        {
            SaveManager.Set(SaveManager.OPEN_SHOP_IN_MENU, true);
            BackToMenu();
        }

        public void OnChangeTab()
        {
            DisableAllPurchaseButtons();
            SetSelectStyle(null);

            selectStyle.interactable = false;
        }
    }
}