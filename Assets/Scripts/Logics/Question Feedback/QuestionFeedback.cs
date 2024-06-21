using DeathMatch;
using System.Linq;
using UnityEngine;

public class QuestionFeedback : MonoBehaviour
{
    public enum ReportType { WrongQuestion, IncompleteAnswer, BadHint }

    private JSONFileReader questionManager;
    private QuestionFeedbackPresentor presentor;
    private EventManager eventManager;
    private Scoring scoring;

    private bool questionReported;
    private bool questionLiked;
    

    private void Start()
    {
        questionManager = FindObjectOfType<JSONFileReader>();
        presentor = GetComponent<QuestionFeedbackPresentor>();
        eventManager = FindObjectOfType<EventManager>();
        scoring = FindObjectOfType<Scoring>();

        eventManager.OnRoundStart += OnNewRound;
    }

    private void OnDisable()
    {
        eventManager.OnRoundStart -= OnNewRound;
    }

    public void LikeQuestion()
    {
        if (questionLiked)
            return;

        if (questionManager.GetCurrentQuestion == null)
            return;

        questionLiked = true;
        ServerConnection.Instance.QuestionLikeDislike(questionManager.GetCurrentQuestion.id, true);
    }

    public void DislikeQuestion()
    {
        if (questionLiked)
            return;

        if (questionManager.GetCurrentQuestion == null)
            return;

        questionLiked = true;
        ServerConnection.Instance.QuestionLikeDislike(questionManager.GetCurrentQuestion.id, false);
    }

    public void Report(ReportType reportType)
    {
        if (questionReported)
            return;

        if (questionManager.GetCurrentQuestion == null)
            return;

        var playerAnswer = string.Empty;
        var reportTyp = 0;
        var isAnsweredCorrect = scoring.IsCorrectAnswer(eventManager.Characters.FirstOrDefault(x => x.Player.Username == eventManager.Player.Username));

        if (eventManager.Player.IsAnswered)
        {
            switch (questionManager.GetCurrentQuestion.type)
            {
                case "متنی":
                    playerAnswer = eventManager.Player.GetLastTextualAnswer();
                    break;
                case "ریاضی":
                    playerAnswer = eventManager.Player.GetLastNumericAnswer().ToString();
                    break;
                default:
                    break;
            }
        }

        switch (reportType)
        {
            case ReportType.WrongQuestion:
                reportTyp = 1;
                break;
            case ReportType.IncompleteAnswer:
                reportTyp = 2;
                break;
            case ReportType.BadHint:
                reportTyp = 3;
                break;
            default:
                break;
        }

        questionReported = true;
        ServerConnection.Instance.ReportQuestion(questionManager.GetCurrentQuestionId(), isAnsweredCorrect, playerAnswer, reportTyp);
        presentor.CloseReportTypes();
    }

    private void OnNewRound()
    {
        presentor.CloseReportTypes();
        questionLiked = false;
        questionReported = false;
    }
}