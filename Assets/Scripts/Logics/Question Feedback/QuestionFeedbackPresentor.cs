using UnityEngine;
using UnityEngine.UI;

public class QuestionFeedbackPresentor : MonoBehaviour
{
    [SerializeField] private Button likeQuestion;
    [SerializeField] private Button dislikeQuestion;
    [SerializeField] private Button openReportTypes;
    [SerializeField] private GameObject reportTypesHolder;
    [SerializeField] private Button wrongQuestionReport;
    [SerializeField] private Button incompleteAnwerReport;
    [SerializeField] private Button badHintReport;

    private QuestionFeedback questionFeedback;

    private void Start()
    {
        questionFeedback = GetComponent<QuestionFeedback>();

        InitButtons();
        CloseReportTypes();
    }

    private void InitButtons()
    {
        openReportTypes.onClick.AddListener(OpenReportTypes);
        openReportTypes.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        likeQuestion.onClick.AddListener(questionFeedback.LikeQuestion);
        likeQuestion.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        dislikeQuestion.onClick.AddListener(questionFeedback.DislikeQuestion);
        dislikeQuestion.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        wrongQuestionReport.onClick.AddListener(() => questionFeedback.Report(QuestionFeedback.ReportType.WrongQuestion));
        wrongQuestionReport.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        incompleteAnwerReport.onClick.AddListener(() => questionFeedback.Report(QuestionFeedback.ReportType.IncompleteAnswer));
        incompleteAnwerReport.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        badHintReport.onClick.AddListener(() => questionFeedback.Report(QuestionFeedback.ReportType.BadHint));
        badHintReport.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    private void OpenReportTypes()
    {
        reportTypesHolder.SetActive(true);
    }

    public void CloseReportTypes()
    {
        reportTypesHolder.SetActive(false);
    }
}