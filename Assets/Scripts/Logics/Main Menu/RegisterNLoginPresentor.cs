using DeathMatch;
using SDM;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using USDM;

public class RegisterNLoginPresentor : MonoBehaviour
{
    [SerializeField] private Button continueWithGuest;

    [Header("Options")]
    [SerializeField] private Button registerFromOptions;
    [SerializeField] private Sprite loggedInSprite;
    [SerializeField] private Sprite needRegistrationSprite;

    [Header("Register")]
    [SerializeField] private TMP_InputField registerCurrentUsernameIF;
    [SerializeField] private GameObject registerNewUsername;
    [SerializeField] private TMP_InputField registerNewUsernameIF;
    [SerializeField] private GameObject registerNewUsernameHint;
    [SerializeField] private TMP_InputField registerPasswordIF;
    [SerializeField] private TMP_InputField registerPasswordConfirmIF;
    [SerializeField] private Button registerShowPasswordBtn;
    [SerializeField] private TMP_InputField registerPhoneNumberIF;
    [SerializeField] private TMP_InputField registerPhoneNumberConfirmIF;
    [SerializeField] private Button registerSendPhoneNumberConfirmBtn;
    [SerializeField] private TextMeshProUGUI registerPhoneNumberError;
    [SerializeField] private Button registerSendBtn;
    [SerializeField] private TextMeshProUGUI registerErrorText;
    private bool registerSent;
    private bool registerPhoneNumberSent;

    [Header("Login")]
    [SerializeField] private TMP_InputField loginUsernameIF;
    [SerializeField] private TMP_InputField loginPasswordIF;
    [SerializeField] private Button loginShowPasswordBtn;
    [SerializeField] private TextMeshProUGUI loginErrorText;
    [SerializeField] private Button loginSendBtn;
    private bool loginSent;

    [Header("Login With Phone Number")]
    [SerializeField] private TMP_InputField phoneNumberField;
    [SerializeField] private TextMeshProUGUI phoneNumberLoginErrorText;
    [SerializeField] private Button sendPhoneNumberBtn;
    private bool phoneNumberLoginSent;

    private MainUI mainUI;

    private void Start()
    {
        mainUI = FindObjectOfType<MainUI>();

        EmptyFields();

        SetRegisterError(null);
        SetRegisterPhoneNumberError(null);
        SetPhoneNumberLoginError(null);
        SetLoginError(null);

        Init();

        registerSendBtn.onClick.AddListener(Register);
        registerSendBtn.onClick.AddListener(SubmitPhoneNumber);
        registerSendBtn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        registerShowPasswordBtn.onClick.AddListener(ToggleShowPasswordRegister);
        registerShowPasswordBtn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        registerSendPhoneNumberConfirmBtn.onClick.AddListener(SendConfirmCode);
        registerSendPhoneNumberConfirmBtn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        loginSendBtn.onClick.AddListener(Login);
        loginSendBtn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        loginShowPasswordBtn.onClick.AddListener(ToggleShowPasswordLogin);
        loginShowPasswordBtn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        sendPhoneNumberBtn.onClick.AddListener(SendUserRecoveryViaPhoneNumber);
        sendPhoneNumberBtn.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        registerFromOptions.onClick.AddListener(mainUI.OpenRegister);
        registerFromOptions.onClick.AddListener(ResetPasswordContentTypeRegister);
        registerFromOptions.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        continueWithGuest.onClick.AddListener(ContinueAsGuest);
        continueWithGuest.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    private void EmptyFields()
    {
        registerNewUsernameIF.text = null;
        registerCurrentUsernameIF.text = null;
        registerPasswordIF.text = null;
        registerPasswordConfirmIF.text = null;
        registerPhoneNumberConfirmIF.text = null;
        loginUsernameIF.text = null;
        loginPasswordIF.text = null;
        registerPhoneNumberIF.text = null;
        phoneNumberField.text = null;
    }

    private void Init()
    {
        mainUI.CanPlaySFX = false;

        registerCurrentUsernameIF.text = SaveManager.Get<string>(SaveManager.PLAYER_USERNAME);

        if (!SaveManager.Get<bool>(SaveManager.IS_REGISTERED))
        {
            registerFromOptions.image.sprite = needRegistrationSprite;
            registerNewUsername.SetActive(true);
            registerNewUsernameHint.SetActive(true);
        }
        else
        {
            registerFromOptions.image.sprite = loggedInSprite;
            registerNewUsername.SetActive(false);
            registerNewUsernameHint.SetActive(false);
        }

        if (!SaveManager.Get<bool>(SaveManager.PLAYER_LOGIN))
        {
            var _tutorial = FindObjectOfType<TutorialSteps>();

            if (!_tutorial.TutorialMode)
                mainUI.OpenRegisterOrLogin();
        }

        if (GameManager.Instance.LatestPlayerInfo != null)
        {
            var playerInfo = GameManager.Instance.LatestPlayerInfo;

            if (playerInfo.data.user.phone != string.Empty)
            {
                var isConfimed = !playerInfo.data.user.phone.Contains('!');
                var phoneNumber = playerInfo.data.user.phone;

                if (!isConfimed)
                    phoneNumber = phoneNumber.Remove(phoneNumber.Length - 1);

                registerPhoneNumberIF.text = phoneNumber;
                registerPhoneNumberIF.interactable = !isConfimed;
                registerSendPhoneNumberConfirmBtn.interactable = !isConfimed;
            }
            else
            {
                registerPhoneNumberIF.interactable = true;
                registerSendPhoneNumberConfirmBtn.interactable = true;
            }
        }

        mainUI.CanPlaySFX = true;
    }

    private void Register()
    {
        if (registerSent)
            return;

        if (SaveManager.Get<bool>(SaveManager.IS_REGISTERED))
            return;

        if (registerNewUsernameIF.text.Length < 4 || registerNewUsernameIF.text.Length > 12)
        {
            SetRegisterError("نام کاربری حداقل 4 و حداکثر 12 حرف");
            return;
        }

        if (registerPasswordIF.text.Length == 0 || registerPasswordConfirmIF.text.Length == 0)
        {
            SetRegisterError("رمزعبور دلخواه را وارد کنید");
            return;
        }

        if (registerPasswordIF.text != registerPasswordConfirmIF.text)
        {
            SetRegisterError("رمزعبور های ورودی با یک دیگر برابر نیستند");
            return;
        }

        if (SaveManager.Get<bool>(SaveManager.IS_GUEST))
            ServerConnection.Instance.CheckUsername(registerNewUsernameIF.text, OnCheck);
        else
            OnCheck(new SimpleModel() { success = true, message = null });

        registerSent = true;
        SetRegisterError("درحال بررسی نام کاربری...");

        void OnCheck(SimpleModel model)
        {
            registerSent = false;

            if (!model.success)
            {
                SetRegisterError(model.message);
                return;
            }

            ServerConnection.Instance.RegisterUser(registerNewUsernameIF.text, registerPasswordIF.text, OnRegister);
            registerSent = true;
            SetRegisterError("درحال ثبت نام...");

            void OnRegister(UserInfo userInfo)
            {
                registerSent = false;
                SetRegisterError(userInfo.message);

                if (!userInfo.success)
                    return;

                SaveManager.Set(SaveManager.PLAYER_USERNAME, userInfo.data.user.userName);
                SaveManager.Set(SaveManager.PLAYER_LOGIN, true);
                SaveManager.Set(SaveManager.IS_REGISTERED, true);
                SaveManager.Set(SaveManager.IS_GUEST, false);

                SetLoginError(userInfo.message);

                LevelLoader.LoadLevel(GameManager.Instance.loadingScene);
            }
        }
    }

    private void ToggleShowPasswordRegister()
    {
        if(registerPasswordIF.contentType == TMP_InputField.ContentType.Password)
        {
            registerPasswordIF.contentType = TMP_InputField.ContentType.Standard;
            registerPasswordConfirmIF.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            registerPasswordIF.contentType = TMP_InputField.ContentType.Password;
            registerPasswordConfirmIF.contentType = TMP_InputField.ContentType.Password;
        }

        registerPasswordIF.ForceLabelUpdate();
        registerPasswordConfirmIF.ForceLabelUpdate();
    }

    private void ToggleShowPasswordLogin()
    {
        if (loginPasswordIF.contentType == TMP_InputField.ContentType.Password)
        {
            loginPasswordIF.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            loginPasswordIF.contentType = TMP_InputField.ContentType.Password;
        }

        loginPasswordIF.ForceLabelUpdate();
    }

    private void ResetPasswordContentTypeRegister()
    {
        registerPasswordIF.contentType = TMP_InputField.ContentType.Password;
        registerPasswordConfirmIF.contentType = TMP_InputField.ContentType.Password;

        registerPasswordIF.ForceLabelUpdate();
        registerPasswordConfirmIF.ForceLabelUpdate();
    }

    private void ContinueAsGuest()
    {
        SaveManager.Set(SaveManager.PLAYER_LOGIN, true);
        LevelLoader.LoadLevel(GameManager.Instance.loadingScene);
    }

    private void SubmitPhoneNumber()
    {
        if (registerPhoneNumberSent)
            return;

        if (registerPhoneNumberIF.text.Length == 0)
            return;

        if (registerPhoneNumberConfirmIF.text.Length == 0)
        {
            SetRegisterPhoneNumberError("کد تایید را وارد کنید");
            return;
        }

        registerPhoneNumberSent = true;
        ServerConnection.Instance.ActivePhoneNumber(registerPhoneNumberConfirmIF.text, OnActivatePhone);

        void OnActivatePhone(UserPhone userPhone)
        {
            registerPhoneNumberSent = false;
            SetRegisterPhoneNumberError(userPhone.message);
        }
    }

    private void SendConfirmCode()
    {
        if (registerPhoneNumberIF.text.Length != 11 || (registerPhoneNumberIF.text[0] != '0' || registerPhoneNumberIF.text[1] != '9'))
        {
            SetRegisterPhoneNumberError("شماره وارد شده صحیح نمیباشد");
            return;
        }

        StartCoroutine(ReactiveSendConfirmCodeCoroutine());
        ServerConnection.Instance.SetPhoneNumber(registerPhoneNumberIF.text, OnSendPhone);

        void OnSendPhone(UserPhone userPhone)
        {
            SetRegisterPhoneNumberError(userPhone.message);
        }
    }

    private IEnumerator ReactiveSendConfirmCodeCoroutine()
    {
        registerSendPhoneNumberConfirmBtn.interactable = false;
        yield return new WaitForSeconds(30);
        registerSendPhoneNumberConfirmBtn.interactable = true;
    }

    private void SetRegisterError(string error)
    {
        registerErrorText.text = error;
    }

    private void SetRegisterPhoneNumberError(string error)
    {
        registerPhoneNumberError.text = error;
    }

    private void Login()
    {
        if (loginSent)
            return;

        if (loginUsernameIF.text.Length == 0 && loginPasswordIF.text.Length == 0)
        {
            SetLoginError("نام کاربری و رمزعبور را پر کنید");
            return;
        }
        else if (loginUsernameIF.text.Length == 0)
        {
            SetLoginError("نام کاربری را پر کنید");
            return;
        }
        else if (loginPasswordIF.text.Length == 0)
        {
            SetLoginError("رمزعبور را پر کنید");
            return;
        }

        SetLoginError(null);
        ServerConnection.Instance.LoginUser(loginUsernameIF.text, loginPasswordIF.text, OnUserLogin);
        loginSent = true;

        void OnUserLogin(UserInfo userInfo)
        {
            loginSent = false;
            SetLoginError(userInfo.message);

            if (!userInfo.success)
                return;

            SaveManager.Set(SaveManager.PLAYER_USERNAME, userInfo.data.user.userName);
            SaveManager.Set(SaveManager.PLAYER_LOGIN, true);
            SaveManager.Set(SaveManager.IS_REGISTERED, true);
            SaveManager.Set(SaveManager.IS_GUEST, false);

            SetLoginError(userInfo.message);

            LevelLoader.LoadLevel(GameManager.Instance.loadingScene);
        }
    }

    private void SetLoginError(string error)
    {
        loginErrorText.text = error;
    }

    private void SendUserRecoveryViaPhoneNumber()
    {
        if (phoneNumberLoginSent)
            return;

        if (phoneNumberField.text.Length != 11 || (phoneNumberField.text[0] != '0' || phoneNumberField.text[1] != '9'))
        {
            SetPhoneNumberLoginError("شماره وارد شده صحیح نمیباشد");
            return;
        }

        ServerConnection.Instance.SendUserInfoViaPhoneNumber(phoneNumberField.text, OnPhoneNumberLoginSend);
        phoneNumberLoginSent = true;
        SetPhoneNumberLoginError(null);

        void OnPhoneNumberLoginSend(UserPhone userPhone)
        {
            phoneNumberLoginSent = false;

            if (userPhone.success)
            {
                mainUI.OpenLogin();
                SetLoginError(userPhone.message);
                return;
            }

            SetPhoneNumberLoginError(userPhone.message);
        }
    }

    private void SetPhoneNumberLoginError(string error)
    {
        phoneNumberLoginErrorText.text = error;
    }
}