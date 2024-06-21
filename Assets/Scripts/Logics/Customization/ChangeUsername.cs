using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DeathMatch
{
    public class ChangeUsername : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentUsername;
        [SerializeField] private TMP_InputField newUsername;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI pricetText;
        [SerializeField] private int minimumNameLength = 3;
        [SerializeField] private int maximumNameLength = 20;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;

        [SerializeField] private KeyboardsManager keyboardsManager;

        private MainUI _mainUI;
        private int price;

        private void Start()
        {
            _mainUI = FindObjectOfType<MainUI>();

            CheckNewNameLength(string.Empty);
            resultText.text = null;

            submitButton.onClick.AddListener(ChangeName);
            submitButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

            cancelButton.onClick.AddListener(() => newUsername.text = null);

            newUsername.onValueChanged.AddListener(CheckNewNameLength);

            keyboardsManager.ActiveTextualKeyboard();
            keyboardsManager.textualKeyboard.OnSubmitAnswer += OnSubmitNewUsername;

            currentUsername.text = SaveManager.Get<string>(SaveManager.PLAYER_USERNAME);

            ServerConnection.Instance.GetChangeUsernamePrice((price) =>
            {
                this.price = price;
                pricetText.text = price.ToString();
            });
        }

        private void Update()
        {
            submitButton.interactable = WealthManager.Instance.IsEnoughCoin(price);
        }

        private void OnDisable()
        {
            keyboardsManager.textualKeyboard.OnSubmitAnswer -= OnSubmitNewUsername;
        }

        private void OnSubmitNewUsername(string obj)
        {
            CheckNewNameLength(obj);
            newUsername.text = obj;
        }

        [System.Obsolete]
        private void ChangeName()
        {
            ServerConnection.Instance.UpdateUsername(newUsername.text, (userInfo) =>
             {
                 resultText.text = userInfo.message;

                 if (userInfo.success == false)
                     return;

                 SaveManager.Set(SaveManager.PLAYER_USERNAME, userInfo.data.user.userName);
                 currentUsername.text = SaveManager.Get<string>(SaveManager.PLAYER_USERNAME);
                 newUsername.text = null;
                 resultText.text = null;

                 FindObjectOfType<PlayerInfo>().UpdateUserInfo(userInfo);
                 _mainUI.BackToMainMenu();

                 ServerConnection.Instance.GetChangeUsernamePrice((price) =>
                 {
                     this.price = price;
                     pricetText.text = price.ToString();
                 });
             });
        }

        private void CheckNewNameLength(string text)
        {
            submitButton.interactable = text.Length > minimumNameLength && text.Length <= maximumNameLength ? true : false;
        }
    }
}