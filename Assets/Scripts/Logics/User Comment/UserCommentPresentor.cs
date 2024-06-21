using DeathMatch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserCommentPresentor : MonoBehaviour
{
    [SerializeField] private GameObject askToRateInMarketPanel;

    [Header("Rating Panel")]
    [SerializeField] private GameObject ratingPanel;
    [SerializeField] private Slider ratingSlider;
    [SerializeField] private Button sendRatingButton;
    [SerializeField] private Button cancelRatingButton;

    [Header("Feedback Panel")]
    [SerializeField] private TMP_InputField feedbackInputField;
    [SerializeField] private Button sendFeedbackButton;
    [SerializeField] private TextMeshProUGUI messageText;

    private MainUI mainUI;
    private UserComment userComment;

    private void Start()
    {
        CloseAskRatingInMarket();
        CloseRatingPanel();
        SetMessageText(null);

        mainUI = FindObjectOfType<MainUI>();
        userComment = GetComponent<UserComment>();

        ratingSlider.value = 0;

        cancelRatingButton.onClick.AddListener(mainUI.BackToMainMenu);
        sendRatingButton.onClick.AddListener(CheckRating);
        sendFeedbackButton.onClick.AddListener(CheckComment);
    }

    private void Update()
    {
        sendRatingButton.interactable = ratingSlider.value > 0;
    }

    private void CheckRating()
    {
        if (ratingSlider.value <= 3)
        {
            mainUI.OpenFeedback();
        }
        else
        {
            userComment.SendRating((int)ratingSlider.value, string.Empty);
        }
    }

    private void CheckComment()
    {
        if (string.IsNullOrEmpty(feedbackInputField.text))
            return;

        userComment.SendRating((int)ratingSlider.value, feedbackInputField.text);
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

    public void SetSuccessfulMessage()
    {
        SetMessageText("نظر شما برای ما ارسال شد");
        Invoke(nameof(BackToMenu), 2f);
    }

    public void BackToMenu()
    {
        SetMessageText(null);
        mainUI.BackToMainMenu();
    }

    public void OpenAskRatingInMarket()
    {
        askToRateInMarketPanel.SetActive(true);
    }

    public void CloseAskRatingInMarket()
    {
        askToRateInMarketPanel.SetActive(false);
    }

    public void OpenRatingPanel()
    {
        ratingPanel.SetActive(true);
    }

    public void CloseRatingPanel()
    {
        ratingPanel.SetActive(false);
    }
}