using RTLTMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using GameAnalyticsSDK;
using System.Text.RegularExpressions;
using GSDM;

namespace DeathMatch
{
    public class EventManagerPresentor : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private RTLTextMeshPro countDownText;
        [SerializeField] private RTLTextMeshPro timerTextualText;
        [SerializeField] private RTLTextMeshPro timerNumericText;
        [SerializeField] private RTLTextMeshPro roundText;
        [SerializeField] private Button exit;

        [Space(2)]
        [Header("Question Box")]
        [SerializeField] private RectTransform questionBox;
        [SerializeField] private RTLTextMeshPro questionText;
        [SerializeField] private TextMeshProUGUI autherText;
        [SerializeField] private float hidePosition;
        [SerializeField] private float showPosition;
        [SerializeField] private float questionBoxTransitionTime = 1;

        [Space(2)]
        [Header("Answers Panel")]
        [SerializeField] private GameObject answersHolder;
        [SerializeField] private GameObject answerPrefab;
        [SerializeField] private Sprite correctAnswerSprite;
        [SerializeField] private Sprite wrongAnswerSprite;
        [SerializeField] private Sprite noAnswerSprite;

        [Space(2)]
        [Header("Loading Results Panel")]
        [SerializeField] private GameObject loadingResultsPanel;

        [Space(2)]
        [Header("Final Results")]
        [SerializeField] private GameObject finalResultsPanel;
        [SerializeField] private CharacterResult characterResultPrefab;
        [SerializeField] private GameObject resultsHolder;
        [SerializeField] private Button toMenuButton;
        [SerializeField] private TextMeshProUGUI wonCoinsText;
        [SerializeField] private TextMeshProUGUI wonTrophiesText;
        [SerializeField] private TextMeshProUGUI wonExpsText;

        [Space(2)]
        [Header("Exit Panel")]
        [SerializeField] private GameObject exitPanel;
        [SerializeField] private Button confirmExit;
        [SerializeField] private Button cancelExit;
        [SerializeField] private Button panelCancelExit;
        private bool _isInExitPanel = false;

        [Space(2)]
        [Header("Instant Result")]
        [SerializeField] private GameObject instantResultPanel;
        [SerializeField] private Image instantResultImage;
        [SerializeField] private Sprite correctAnswerIResult;
        [SerializeField] private Sprite wrongAnswerIResult;

        private EventManager _gameManager;
        private Scoring _scoring;
        private KeyboardsManager _keyboardsManager;
        private Boosters _boosters;

        private void Start()
        {
            // find needed components
            _gameManager = FindObjectOfType<EventManager>();
            _scoring = FindObjectOfType<Scoring>();
            _keyboardsManager = FindObjectOfType<KeyboardsManager>();
            _boosters = FindObjectOfType<Boosters>();

            // subscribe keyboards
            _keyboardsManager.textualKeyboard.OnSubmitAnswer += SubmitTextualAnswer;
            _keyboardsManager.numericKeyboard.OnSubmitAnswer += SubmitNumericAnswer;

            // subscribe buttons
            toMenuButton.onClick.AddListener(() =>
            {
                LevelLoader.LoadLevel(GameManager.Instance.loadingScene);

                // GameAnalytics events
                GameAnalytics.NewDesignEvent("WinResultsReturnBTN", 1f);

                if (FindObjectOfType<TutorialSteps>(true).TutorialEnabledFromStart)
                    GameAnalytics.NewDesignEvent("ReturnToMenu_Tutorial", 1f);
            });

            //........
            exit.onClick.AddListener(() => ActiveExitPanel(true));
            confirmExit.onClick.AddListener(ConfirmExit);
            cancelExit.onClick.AddListener(() => ActiveExitPanel(false));
            panelCancelExit.onClick.AddListener(() => ActiveExitPanel(false));

            // initialize
            HideAllUI();
        }

        private void OnDisable()
        {
            _keyboardsManager.textualKeyboard.OnSubmitAnswer -= SubmitTextualAnswer;
            _keyboardsManager.numericKeyboard.OnSubmitAnswer -= SubmitNumericAnswer;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ActiveExitPanel(!_isInExitPanel);
        }

        public void HideAllUI()
        {
            HideFinalResults();
            StartCoroutine(HideAnswersCoroutine(true));
            StartCoroutine(HideInstantResultsCoroutine(true));
            HideQuestionBox(true);
            HideBoosters();
            HideKeyboards();
            HideLoadingResultsPanel();
            SetRoundText(1.ToString());
            _boosters.HideBoosters(true);
            _keyboardsManager.HideKeyboard(true);
        }

        public void SetCountDownText(string value) => countDownText.text = value;

        // question box
        public void ShowQuestionBox(Question question)
        {
            questionText.text = question.question;

            if (string.IsNullOrEmpty(question.userName))
                autherText.text = "رقابت مرگ";
            else
                autherText.text = question.userName;

            questionBox.DOAnchorPosY(showPosition, questionBoxTransitionTime);
            AudioManager.Instance.QuestionNotificationSFX();
        }

        public void HideQuestionBox(bool force = false)
        {
            questionText.text = string.Empty;
            questionBox.DOAnchorPosY(hidePosition, force ? 0 : questionBoxTransitionTime);
        }

        // timer text
        public void SetTimerText(string value)
        {
            timerNumericText.text = value;
            timerTextualText.text = value;
        }

        // round text
        public void SetRoundText(string value) => roundText.text = value;

        // keyboards
        public void HideKeyboards() => _keyboardsManager.HideKeyboard();
        public void ShowKeyboards(float delay) => _keyboardsManager.ShowKeyboards(delay);
        public void AddLetterToKeyboard(char letter) => _keyboardsManager.CurrentKeyboard.AddLetter(letter.ToString());
        private void SubmitTextualAnswer(string text) => _gameManager.SubmitAnswerPlayer(text, false);
        private void SubmitNumericAnswer(string text) => _gameManager.SubmitAnswerPlayer(text, true);
        public void ClearKeyboards() => _keyboardsManager.CurrentKeyboard.SetAnswerInputText(null);
        public void SetKeyboardText(string text) => _keyboardsManager.CurrentKeyboard.SetAnswerInputText(text);

        // boosters
        public void HideBoosters() => _boosters.HideBoosters();
        public void ShowBoosters(float delay) => _boosters.ShowBoosters(delay);

        // answers panel
        public void ShowAnswers(List<EventManager.CharacterInstance> Characters, bool IsNumericQuestion)
        {
            foreach (Transform transform in answersHolder.transform)
                Destroy(transform.gameObject);

            var orderedCharacters = Characters.OrderBy(x => x.Pivot.transform.localPosition.x).ToList();

            foreach (var character in orderedCharacters)
            {
                var answer = Instantiate(answerPrefab, answersHolder.transform);

                var answerText = answer.transform.Find("ANSWER_TEXT").GetComponent<RTLTextMeshPro>();
                var usernameText = answer.transform.Find("USERNAME_TEXT").GetComponent<TextMeshProUGUI>();
                var correctOrNotImage = answer.transform.Find("CORRECT_IMAGE").GetComponent<Image>();
                //var answerTimeText = answer.transform.Find("TIME_TEXT").GetComponent<TextMeshProUGUI>();
                var scoreText = answer.transform.Find("SCORE_TEXT").GetComponent<TextMeshProUGUI>();
                var removedCover = answer.transform.Find("DEAD_COVER").gameObject;

                usernameText.text = character.Player.Username;
                // answerTimeText.text = character.player.AnswerTimer.ToString("0.00") + "s";

                if (!character.Player.Dead)
                {
                    removedCover.gameObject.SetActive(false);

                    if (character.Player.IsAnswered)
                    {
                        if (IsNumericQuestion)
                        {
                            answerText.text = character.Player.GetLastNumericAnswer().ToString();
                        }
                        else
                        {
                            answerText.text = character.Player.GetLastTextualAnswer();
                        }

                        if (_scoring.IsCorrectAnswer(character))
                        {
                            correctOrNotImage.sprite = correctAnswerSprite;
                            scoreText.text = "+1";
                        }
                        else
                        {
                            correctOrNotImage.sprite = wrongAnswerSprite;
                            scoreText.text = "-1";
                        }
                    }
                    else
                    {
                        correctOrNotImage.sprite = noAnswerSprite;
                        answerText.text = "بدون جواب";
                        scoreText.text = "-1";
                    }
                }
                else
                {
                    removedCover.gameObject.SetActive(true);
                }
            }

            StartCoroutine(HideAnswersCoroutine());
            answersHolder.SetActive(true);
        }

        public IEnumerator HideAnswersCoroutine(bool force = false)
        {
            yield return new WaitForSeconds(force ? 0 : _gameManager.showAnswersTime);
            answersHolder.SetActive(false);
        }

        // final results
        public void ShowFinalResults(GSDM.SaveGame data)
        {
            StartCoroutine(HideAnswersCoroutine(true));

            for (int i = 0; i < resultsHolder.transform.childCount; i++)
                Destroy(resultsHolder.transform.GetChild(i).gameObject);

            toMenuButton.interactable = false;

            var playersByPosition = _scoring.GetOrderedCharacters();
            var playerPosition = _scoring.Position(_gameManager.Player.Username);

            for (int i = 0; i < playersByPosition.Count; i++)
            {
                var result = Instantiate(characterResultPrefab, resultsHolder.transform);

                var isFinished = false;

                if (playersByPosition[i].Player.Dead)
                    isFinished = true;

                if (_gameManager.IsGameStarted == false)
                    isFinished = true;

                var player = playersByPosition[i].Player;
                result.Setup(player.Username, i, player.GetCorrectAnswersCount(), player.GetWrongAnswersCount(), isFinished);
            }

            var coinAmount = System.Convert.ToInt32(Regex.Match(data.data.changeCoin, @"-?\d+").Value);
            var expAmount = System.Convert.ToInt32(Regex.Match(data.data.changeExp, @"-?\d+").Value);
            var trophyAmount = System.Convert.ToInt32(Regex.Match(data.data.changeTrophy, @"-?\d+").Value);

            //var coinFrom = 0;
            //var coinTo = coinAmount;

            //var expFrom = 0;
            //var expTo = expAmount;

            //var trophyFrom = 0;
            //var trophyTo = trophyAmount;

            //DOVirtual.Int(coinFrom, coinTo, 2f, (value) =>
            //{
            //    if (value > 0)
            //        wonCoinsText.text = $"+{value}";
            //    else
            //        wonCoinsText.text = value.ToString();
            //}).OnComplete(() =>
            //{
            //    DOVirtual.Int(expFrom, expTo, 2f, (value) =>
            //    {
            //        if (value > 0)
            //            wonExpsText.text = $"+{value}";
            //        else
            //            wonExpsText.text = value.ToString();
            //    }).OnComplete(() =>
            //    {
            //        DOVirtual.Int(trophyFrom, trophyTo, 2f, (value) =>
            //        {
            //            if (value > 0)
            //                wonTrophiesText.text = $"+{value}";
            //            else
            //                wonTrophiesText.text = value.ToString();
            //        });
            //    });
            //});

            if (coinAmount > 0)
                wonCoinsText.text = $"+{coinAmount}";
            else
                wonCoinsText.text = coinAmount.ToString();

            if (expAmount > 0)
                wonExpsText.text = $"+{expAmount}";
            else
                wonExpsText.text = expAmount.ToString();

            if (trophyAmount > 0)
                wonTrophiesText.text = $"+{trophyAmount}";
            else
                wonTrophiesText.text = trophyAmount.ToString();

            finalResultsPanel.SetActive(true);
            AudioManager.Instance.EndPanelSFX();
            StartCoroutine(HideInstantResultsCoroutine(true));
            StartCoroutine(ToMenuButtonCoroutine());
        }

        public void HideFinalResults() => finalResultsPanel.SetActive(false);

        private IEnumerator ToMenuButtonCoroutine()
        {
            //yield return new WaitForSeconds(7f);
            yield return new WaitForSeconds(0);
            toMenuButton.interactable = true;
        }

        // exit panel
        private void ActiveExitPanel(bool active)
        {
            exitPanel.SetActive(active);
            _isInExitPanel = active;

            if (active)
                AudioManager.Instance.ClickButtonSFX();
            else
                AudioManager.Instance.CancelSFX();
        }
        private void ConfirmExit()
        {
            // GameAnalytics events
            GameAnalytics.NewDesignEvent("ExitMatchBTN", 1f);

            if (TutorialSteps.IsTutorialPlaying())
                GameAnalytics.NewDesignEvent("ExitMatchBTN_Tutorial", 1f);

            LevelLoader.LoadLevel(GameManager.Instance.loadingScene);
            AudioManager.Instance.ConfirmSFX();
        }

        // instant result
        public void ShowInstantResult(bool correctAnswer)
        {
            if (correctAnswer)
            {
                instantResultImage.sprite = correctAnswerIResult;
                AudioManager.Instance.CorrectAnswerSFX();
            }
            else
            {
                instantResultImage.sprite = wrongAnswerIResult;
                AudioManager.Instance.WrongAsnwerSFX();
            }

            instantResultPanel.gameObject.SetActive(true);
            StartCoroutine(HideInstantResultsCoroutine(false));
        }

        public IEnumerator HideInstantResultsCoroutine(bool force)
        {
            yield return new WaitForSeconds(force ? 0 : _gameManager.instantResultTime);
            instantResultPanel.gameObject.SetActive(false);
        }

        public void ShowLoadingResultsPanel()
        {
            loadingResultsPanel.SetActive(true);
        }

        public void HideLoadingResultsPanel()
        {
            loadingResultsPanel.SetActive(false);
        }
    }
}