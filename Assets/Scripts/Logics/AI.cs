using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeathMatch
{
    public class AI : MonoBehaviour
    {
        public enum KnowledgeLevel { Idiot, Average, Smart }

        private EventManager _gameManager;
        private Character _character;
        private KnowledgeLevel _knowledgeLevel;

        private int wrongAnswersLeft;

        private bool isScripted;
        private List<bool> scriptedAnswers = new List<bool>();

        private void Start()
        {
            _gameManager = GetComponentInParent<EventManager>();
            _character = GetComponent<Character>();

            _gameManager.OnRoundStart += ProcessAI;
        }

        private void ProcessAI()
        {
            if (_character.IsAnswered || _character.Dead)
                return;

            StartCoroutine(ProcessAICoroutine());
        }

        public void SetScripted(TutorialSteps.ScriptedBot bot)
        {
            isScripted = true;
            scriptedAnswers = bot.Answers;
        }

        public void SetKnowledgeLevel(KnowledgeLevel level)
        {
            if (GameManager.Instance.PlayerLevel < 5)
                _knowledgeLevel = KnowledgeLevel.Idiot;
            else
                _knowledgeLevel = level;

            wrongAnswersLeft = GetWrongAnswersCount();
        }

        private IEnumerator ProcessAICoroutine()
        {
            if (_character.Dead)
                yield break;

            var willAnswer = Random.value < ProbabilityOfAnswering() ? true : false;

            if (wrongAnswersLeft == 0)
                willAnswer = Random.value < ProbabilityOfCorrectAnsweringWithNoWrongAnswer() ? true : false;

            if (isScripted)
                willAnswer = true;

            if (!willAnswer)
                yield break;

            if (isScripted)
                yield return null;
            else
                yield return new WaitForSeconds(ThinkTime());

            var willAnswerCorrect = Random.value < ProbabilityOfCorrectAnswering() ? true : false;

            if (wrongAnswersLeft == 0)
                willAnswerCorrect = Random.value < ProbabilityOfCorrectAnsweringWithNoWrongAnswer() ? true : false;

            if (isScripted)
            {
                willAnswerCorrect = scriptedAnswers[0];

                if (GameManager.Instance.PrintLogs)
                    Debug.Log(_character.Username + " | " + willAnswerCorrect);
            }

            if (_gameManager.IsNumericQuestion)
                SubmitNumericAnswer(willAnswerCorrect);
            else
                SubmitTextualAnswer(willAnswerCorrect);

            if (isScripted)
                scriptedAnswers.RemoveAt(0);
        }

        private void SubmitTextualAnswer(bool correct)
        {
            var answer = string.Empty;

            if (correct)
            {
                answer = _gameManager.CorrectAnswersTextual[Random.Range(0, _gameManager.CorrectAnswersTextual.Count)].description;
                wrongAnswersLeft = GetWrongAnswersCount();
            }
            else
            {
                answer = _gameManager.WrongAnswersTextual[Random.Range(0, _gameManager.WrongAnswersTextual.Count)].description;
                wrongAnswersLeft--;
            }

            _gameManager.SubmitAnswerAI(_character.Username, answer, false);
        }

        private void SubmitNumericAnswer(bool correct)
        {
            var accurateAnswer = Random.value < ProbabilityOfAccurateAnswering() ? true : false;

            if (wrongAnswersLeft == 0)
                accurateAnswer = Random.value < ProbabilityOfCorrectAnsweringWithNoWrongAnswer() ? true : false;

            float answer;

            if (correct)
            {
                answer = System.Convert.ToSingle(_gameManager.CorrectAnswerNumeric[0].description);
                wrongAnswersLeft = GetWrongAnswersCount();
            }
            else
            {
                answer = System.Convert.ToSingle(_gameManager.WrongAnswerNumeric[Random.Range(0, _gameManager.WrongAnswerNumeric.Count)].description);
                wrongAnswersLeft--;
            }

            answer += accurateAnswer ? 0 : NotAccurateAnswerRange();

            _gameManager.SubmitAnswerAI(_character.Username, answer.ToString(), true);
        }

        private int GetWrongAnswersCount() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => Random.Range(1, 3),
            KnowledgeLevel.Average => Random.Range(4, 6),
            KnowledgeLevel.Smart => Random.Range(3, 5),
            _ => throw new System.Exception("Level not exist!")
        };

        private float ProbabilityOfAnswering() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => 0.75f,
            KnowledgeLevel.Average => Random.Range(0.75f, 0.95f),
            KnowledgeLevel.Smart => 0.95f,
            _ => throw new System.Exception("Level not exist!")
        };

        private float ThinkTime() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => Random.Range(3, 9),
            KnowledgeLevel.Average => Random.Range(1, 9),
            KnowledgeLevel.Smart => Random.Range(1, 4),
            _ => throw new System.Exception("Level not exist!")
        };

        private float ProbabilityOfCorrectAnswering() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => 0.45f,
            KnowledgeLevel.Average => Random.Range(0.45f, 0.9f),
            KnowledgeLevel.Smart => 0.9f,
            _ => throw new System.Exception("Level not exist!")
        };

        private float ProbabilityOfAccurateAnswering() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => 0.6f,
            KnowledgeLevel.Average => Random.Range(0.6f, 0.9f),
            KnowledgeLevel.Smart => 0.9f,
            _ => throw new System.Exception("Level not exist!")
        };

        private float ProbabilityOfCorrectAnsweringWithNoWrongAnswer() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => 1,
            KnowledgeLevel.Average => Random.Range(0.3f, 0.45f),
            KnowledgeLevel.Smart => Random.Range(0.1f, 0.2f),
            _ => throw new System.Exception("Level not exist!")
        };

        private float NotAccurateAnswerRange() => _knowledgeLevel switch
        {
            KnowledgeLevel.Idiot => Random.Range(-50, 50),
            KnowledgeLevel.Average => Random.Range(-25, 25),
            KnowledgeLevel.Smart => Random.Range(-10, 10),
            _ => throw new System.Exception("Level not exist!")
        };
    }
}