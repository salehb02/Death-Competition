using UnityEngine;
using UnityEngine.UI;
using GameAnalyticsSDK;
using ESDM;

namespace DeathMatch
{
    public class MainUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button openOptions;
        [SerializeField] private Button closeOptions;
        [SerializeField] private Button customize;
        [SerializeField] private Button[] openStore;
        [SerializeField] private Button closeStore;
        [SerializeField] private Button leaderboard;
        [SerializeField] private Button support;
        [SerializeField] private Button playerFeedback;
        [SerializeField] private Button changeUsername;
        [SerializeField] private Button login;
        [SerializeField] private Button register;
        [SerializeField] private Button loginWithPhoneNumber;
        [SerializeField] private Button openBoostersHint;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject supportPanel;
        [SerializeField] private GameObject playerFeedbackPanel;
        [SerializeField] private GameObject changeUsernamePanel;
        [SerializeField] private GameObject storePanel;
        [SerializeField] private GameObject registerOrLoginPanel;
        [SerializeField] private GameObject registerPanel;
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject loginWithPhoneNumberPanel;
        [SerializeField] private GameObject notEnoughCoinForEntrancePanel;
        [SerializeField] private GameObject boosterSelectionPanel;
        [SerializeField] private GameObject boostersHintPanel;

        [Header("Exit Panel")]
        [SerializeField] private GameObject exitPanel;
        [SerializeField] private Button confirmExit;
        [SerializeField] private Button cancelExit;
        [SerializeField] private Button panelCancelExit;
        [SerializeField] private Button leaderboardExit;
        [SerializeField] private Button supportExit;
        [SerializeField] private Button creditsExit;
        [SerializeField] private Button changeUsernameExit;
        [SerializeField] private Button registerExit;
        [SerializeField] private Button loginWithPhoneNumberExit;
        [SerializeField] private Button loginExit;
        [SerializeField] private Button notEnoughCoinExit;
        [SerializeField] private Button closeBoosterSelection;
        [SerializeField] private Button closeBoostersHint;

        private bool _isInMenu = false;
        private bool _isInOptions = false;
        private bool _isInStore;
        private bool _isInExit = false;
        private bool _isInLeaderboard = false;
        private bool _isInSupport = false;
        private bool _isInCredits = false;
        private bool _isInChangeUsername = false;
        private bool _isInRegisterOrLogin = false;
        private bool _isInRegister = false;
        private bool _isInLogin = false;
        private bool _isInLoginWithPhoneNumber = false;
        private bool _isInNotEnoughCoin = false;
        private bool _isInBoosterSelection = false;
        private bool _isInBoostersHint = false;

        public bool CanPlaySFX { get; set; }

        private EventsManager eventsManager;
        private EventDetails eventDetails;
        private QuestionCreation questionCreation;

        private void Start()
        {
            eventsManager = FindObjectOfType<EventsManager>();
            eventDetails = FindObjectOfType<EventDetails>();
            questionCreation = FindObjectOfType<QuestionCreation>();

            BackToMainMenu();
            CanPlaySFX = true;

            openOptions.onClick.AddListener(OpenOptions);
            closeOptions.onClick.AddListener(BackToMainMenu);

            for (int i = 0; i < openStore.Length; i++)
                openStore[i].onClick.AddListener(OpenStore);

            closeStore.onClick.AddListener(BackToMainMenu);
            customize.onClick.AddListener(EnterCustomization);
            leaderboard.onClick.AddListener(OpenLeaderboard);
            support.onClick.AddListener(OpenSupport);
            playerFeedback.onClick.AddListener(OpenFeedback);
            changeUsername.onClick.AddListener(OpenChangeUsername);

            cancelExit.onClick.AddListener(BackToMainMenu);
            panelCancelExit.onClick.AddListener(BackToMainMenu);
            confirmExit.onClick.AddListener(Quit);
            leaderboardExit.onClick.AddListener(BackToMainMenu);
            supportExit.onClick.AddListener(OpenOptions);
            creditsExit.onClick.AddListener(OpenOptions);
            changeUsernameExit.onClick.AddListener(OpenOptions);

            login.onClick.AddListener(OpenLogin);
            register.onClick.AddListener(OpenRegister);
            loginWithPhoneNumber.onClick.AddListener(OpenLoginWithPhoneNumber);
            registerExit.onClick.AddListener(OpenRegisterOrLogin);
            loginWithPhoneNumberExit.onClick.AddListener(OpenLogin);
            loginExit.onClick.AddListener(OpenRegisterOrLogin);
            notEnoughCoinExit.onClick.AddListener(BackToMainMenu);
            closeBoosterSelection.onClick.AddListener(BackToMainMenu);
            openBoostersHint.onClick.AddListener(OpenBoostersHint);
            closeBoostersHint.onClick.AddListener(CloseBoostersHint);

            if (SaveManager.Get<bool>(SaveManager.OPEN_SHOP_IN_MENU) == true)
            {
                OpenStore();
                SaveManager.Set(SaveManager.OPEN_SHOP_IN_MENU, false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isInMenu)
                    OpenExitPanel();
                else if (_isInSupport || _isInCredits || _isInChangeUsername)
                    OpenOptions();
                else if (_isInOptions || _isInExit || _isInLeaderboard || _isInStore || _isInNotEnoughCoin || _isInBoosterSelection)
                    BackToMainMenu();
                else if (_isInBoostersHint)
                    OpenBoosterSelectionPanel();

                if (_isInLogin)
                    OpenRegisterOrLogin();

                else if (_isInRegister)
                {
                    if (!SaveManager.Get<bool>(SaveManager.PLAYER_LOGIN))
                        OpenRegisterOrLogin();
                    else
                        OpenOptions();
                }
                else if (_isInLoginWithPhoneNumber)
                    OpenLogin();
            }
        }

        public void BackToMainMenu()
        {
            _isInMenu = true;
            _isInOptions = false;
            _isInExit = false;
            _isInLeaderboard = false;
            _isInSupport = false;
            _isInCredits = false;
            _isInChangeUsername = false;
            _isInStore = false;

            CloseExitPanel();
            ExitLeaderboard();
            CloseSupport();
            CloseFeedback();
            CloseOptions();
            CloseStore();
            CloseChangeUsername();
            CloseNotEnoughCoin();
            CloseBoostersHint();
            CloseBoosterSelection();

            // Events
            eventsManager.CloseCurrentEventsPanel();
            eventDetails.CloseDetailsPanel();
            questionCreation.CloseNewQuestionPanel();
            questionCreation.CloseCategorySelectionPanel();
        }

        private void OpenOptions()
        {
            optionsPanel.SetActive(true);
            CloseSupport();
            CloseFeedback();
            CloseRegister();
            CloseChangeUsername();
            _isInMenu = false;
            _isInOptions = true;
            _isInSupport = false;
            _isInCredits = false;
            _isInChangeUsername = false;

            if (CanPlaySFX)
                AudioManager.Instance.SettingPopUpSFX();
        }

        private void CloseOptions()
        {
            optionsPanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.SettingCloseSFX();
        }

        private void OpenChangeUsername()
        {
            changeUsernamePanel.SetActive(true);
            CloseOptions();
            _isInOptions = false;
            _isInChangeUsername = true;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseChangeUsername()
        {
            changeUsernamePanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void OpenSupport()
        {
            supportPanel.SetActive(true);
            CloseOptions();
            _isInOptions = false;
            _isInSupport = true;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseSupport()
        {
            supportPanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void OpenFeedback()
        {
            playerFeedbackPanel.SetActive(true);
            CloseOptions();
            _isInOptions = false;
            _isInCredits = true;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseFeedback()
        {
            playerFeedbackPanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void OpenStore()
        {
            BackToMainMenu();
            storePanel.SetActive(true);
            _isInMenu = false;
            _isInStore = true;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseStore()
        {
            storePanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void OpenExitPanel()
        {
            exitPanel.SetActive(true);
            _isInMenu = false;
            _isInExit = true;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseExitPanel()
        {
            exitPanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.CancelSFX();
        }

        private void OpenLeaderboard()
        {
            leaderboardPanel.SetActive(true);
            FindObjectOfType<Leaderboard>().OpenLeaderboard();
            mainPanel.SetActive(false);
            _isInMenu = false;
            _isInLeaderboard = true;

            if (CanPlaySFX)
                AudioManager.Instance.SettingPopUpSFX();
        }

        private void ExitLeaderboard()
        {
            leaderboardPanel.SetActive(false);
            mainPanel.SetActive(true);

            if (CanPlaySFX)
                AudioManager.Instance.CancelSFX();
        }

        private void EnterCustomization()
        {
            LevelLoader.LoadLevel(GameManager.Instance.customizationScene);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();

            // GameAnalytics events
            GameAnalytics.NewDesignEvent("EnterSkinShopBTN", 1f);
        }

        private void Quit()
        {
            if (CanPlaySFX)
                AudioManager.Instance.ConfirmSFX();

            Application.Quit();
        }

        public void OpenRegisterOrLogin()
        {
            if (!SaveManager.Get<bool>(SaveManager.PLAYER_LOGIN))
            {
                CloseLogin();
                CloseRegister();

                _isInMenu = false;
                _isInRegisterOrLogin = true;
                registerOrLoginPanel.SetActive(true);
            }
            else
            {
                OpenOptions();
            }
        }

        private void CloseRegisterOrLogin()
        {
            registerOrLoginPanel.SetActive(false);
            _isInRegisterOrLogin = false;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void OpenLogin()
        {
            CloseRegisterOrLogin();
            CloseLoginWithPhoneNumber();

            _isInLogin = true;
            loginPanel.SetActive(true);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseLogin()
        {
            loginPanel.SetActive(false);
            _isInLogin = false;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void OpenLoginWithPhoneNumber()
        {
            CloseLogin();
            _isInLoginWithPhoneNumber = true;

            loginWithPhoneNumberPanel.SetActive(true);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseLoginWithPhoneNumber()
        {
            loginWithPhoneNumberPanel.SetActive(false);
            _isInLoginWithPhoneNumber = false;

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void OpenRegister()
        {
            CloseRegisterOrLogin();

            _isInRegister = true;
            registerPanel.SetActive(true);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        private void CloseRegister()
        {
            _isInRegister = false;
            registerPanel.SetActive(false);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void OpenNotEnoughCoin()
        {
            _isInMenu = false;
            _isInNotEnoughCoin = true;
            notEnoughCoinForEntrancePanel.SetActive(true);
        }

        public void CloseNotEnoughCoin()
        {
            _isInNotEnoughCoin = false;
            notEnoughCoinForEntrancePanel.SetActive(false);
        }

        public void OpenBoosterSelectionPanel()
        {
            CloseBoostersHint();

            _isInMenu = false;
            _isInBoosterSelection = true;

            boosterSelectionPanel.SetActive(true);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void CloseBoosterSelection()
        {
            _isInMenu = true;
            _isInBoosterSelection = false;

            boosterSelectionPanel.SetActive(false);
        }

        public void OpenBoostersHint()
        {
            _isInBoosterSelection = false;
            _isInBoostersHint = true;

            boostersHintPanel.SetActive(true);

            if (CanPlaySFX)
                AudioManager.Instance.ClickButtonSFX();
        }

        public void CloseBoostersHint()
        {
            _isInBoosterSelection = true;
            _isInBoostersHint = false;

            boostersHintPanel.SetActive(false);
        }

        public void OpenEventDetails()
        {

        }
    }
}