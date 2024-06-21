using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GSDM;
using Random = UnityEngine.Random;
using GameAnalyticsSDK;

namespace DeathMatch
{
    public class JSONFileReader : MonoBehaviour
    {
        public enum QuestionsType { Textual, Numeric, All }

        private Question currentQuestion;

        private List<Question> _allQuestions = new List<Question>();
        private List<Question> _textualQuestions = new List<Question>();
        private List<Question> _numericQuestions = new List<Question>();

        private EventManager _eventManager;
        private KeyboardsManager _keyboardsManager;
        private TutorialSteps _tutorial;
        private Boosters boosters;
        private LoadQuestions _loadQuestions;

        public Question GetCurrentQuestion { get => currentQuestion; }

        private void Awake()
        {
            _eventManager = FindObjectOfType<EventManager>();
            _keyboardsManager = FindObjectOfType<KeyboardsManager>();
            _tutorial = FindObjectOfType<TutorialSteps>();
            boosters = FindObjectOfType<Boosters>();
            _loadQuestions = FindObjectOfType<LoadQuestions>();

            _loadQuestions.OnQuestionsLoaded += ReadJSONFile;

            if (_tutorial.TutorialMode)
                ReadJSONFile();
        }

        private void OnDisable()
        {
            _loadQuestions.OnQuestionsLoaded -= ReadJSONFile;
        }

        public void ReadJSONFile()
        {
            if (_tutorial.TutorialMode)
                _allQuestions = GameManager.Instance.GetTutorialQuestions();
            else
                _allQuestions = GameManager.Instance.GetQuestions();

            ReadTexturalJSON();
            ReadNumericJSON();
        }

        private void ReadTexturalJSON()
        {
            _textualQuestions = _allQuestions.Where(x => x.type == "متنی").ToList();

            var temp = _textualQuestions;

            foreach (var item in temp.ToList())
            {
                if (item.answers.Count == 0 || item.wrongAnswers.Count == 0 || string.IsNullOrEmpty(item.guide1) || string.IsNullOrEmpty(item.guide2) || string.IsNullOrEmpty(item.guide3))
                {
                    temp.Remove(item);
                    GameAnalytics.NewErrorEvent(GAErrorSeverity.Warning, $"Question with id of {item.id} is incomplete");
                    ServerConnection.Instance.ReportQuestion(item.id, false, "CLIENT AUTO REPORT", 4);
                }
            }

            _textualQuestions = temp;
        }

        private void ReadNumericJSON()
        {
            _numericQuestions = _allQuestions.Where(x => x.type == "ریاضی").ToList();

            var temp = _numericQuestions;

            foreach (var item in temp.ToList())
            {
                if (item.answers.Count == 0 || item.wrongAnswers.Count == 0)
                {
                    temp.Remove(item);
                    ServerConnection.Instance.ReportQuestion(item.id, false, "CLIENT AUTO REPORT", 4);
                }
            }

            _numericQuestions = temp;
        }

        public void GenerateQuestion()
        {
            if (Random.value > 0.5f)
                GenerateQuestion(QuestionsType.Textual);
            else
                GenerateQuestion(QuestionsType.Numeric);

            boosters.SetHelps(currentQuestion);
        }

        private void GenerateQuestion(QuestionsType type)
        {
            var questions = new List<Question>();
            var isNumeric = false;

            if (type == QuestionsType.Numeric)
            {
                if (_numericQuestions.Count == 0)
                    ReadNumericJSON();

                questions = _numericQuestions;

                isNumeric = true;
                _keyboardsManager.ActiveNumericKeyboard();

            }
            else if (type == QuestionsType.Textual)
            {
                if (_textualQuestions.Count == 0)
                    ReadTexturalJSON();

                questions = _textualQuestions;

                isNumeric = false;
                _keyboardsManager.ActiveTextualKeyboard();
            }

            var questionIndex = Random.Range(0, questions.Count);
            currentQuestion = questions[questionIndex];

            if (type == QuestionsType.Numeric)
                _numericQuestions.Remove(currentQuestion);
            else if (type == QuestionsType.Textual)
                _textualQuestions.Remove(currentQuestion);

            _eventManager.SetQuestion(currentQuestion, isNumeric);
        }

        public int GetCurrentQuestionId()
        {
            return currentQuestion.id;
        }
    }
}