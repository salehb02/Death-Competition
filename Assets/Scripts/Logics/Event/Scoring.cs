using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace DeathMatch
{
    public class Scoring : MonoBehaviour
    {
        private EventManager _gameManager;
        private EmojiManager _emojiManager;
        private Timer _timer;

        public int ScoreMultiplier { get; set; } = 1;

        private void Start()
        {
            _timer = FindObjectOfType<Timer>();
            _gameManager = FindObjectOfType<EventManager>();
            _emojiManager = FindObjectOfType<EmojiManager>();

            if (_timer)
                _timer.OnTimeOut += FeedbackDelay;
        }

        private void OnDisable()
        {
            if (_timer)
                _timer.OnTimeOut -= FeedbackDelay;
        }

        public void ScoreUp(string playerName)
        {
            var character = _gameManager.Characters.SingleOrDefault(x => x.Player.Username == playerName);

            character.Player.Score += ScoreMultiplier;
         
            for (int i = 0; i < ScoreMultiplier; i++)
                character.Player.IncreaseCorrectAnswersCount();

            _gameManager.EncouragementPlayer(playerName, ScoreMultiplier);
            _emojiManager.ShowEmoji(character.Player.transform, Emoji.EmojiType.OnScoreUp);
        }

        public void ScoreDown(string playerName, bool attackByShip, int overrideMultiplier = -1)
        {
            var character = _gameManager.Characters.SingleOrDefault(x => x.Player.Username == playerName);

            var scoreDownCount = 0;

            if (overrideMultiplier == -1)
                scoreDownCount = ScoreMultiplier;
            else
                scoreDownCount = overrideMultiplier;

            character.Player.Score -= scoreDownCount;

            for (int i = 0; i < scoreDownCount; i++)
                character.Player.IncreaseWrongAnswersCount();

            _gameManager.PunishPlayer(character.Player.Username, scoreDownCount, attackByShip);
            _emojiManager.ShowEmoji(character.Player.transform, Emoji.EmojiType.OnScoreDown);
        }

        public void ResetScoreMultiplier()
        {
            ScoreMultiplier = 1;
        }

        public int Position(string playerName)
        {
            return GetOrderedCharacters().FindIndex(x => x.Player.Username == playerName) + 1;
        }

        public List<EventManager.CharacterInstance> GetOrderedCharacters()
        {
            return _gameManager.Characters.OrderBy(x => x.Player.Dead).ThenByDescending(x => x.Player.Score).ThenByDescending(x => x.Player.OverallTimer).ToList();
        }

        private void FeedbackDelay()
        {
            Invoke(nameof(CheckAnswers), _gameManager.showAnswersTime);
        }

        private void CheckAnswers()
        {
            foreach (var character in _gameManager.Characters)
            {
                if (character.Player.Dead)
                    continue;

                if (IsCorrectAnswer(character))
                    ScoreUp(character.Player.Username);
                else
                    ScoreDown(character.Player.Username, false);
            }

            _gameManager.CheckStatusForNewRound();
        }

        public bool IsCorrectAnswer(EventManager.CharacterInstance character)
        {
            if (!character.Player.IsAnswered)
                return false;

            if (!_gameManager.IsNumericQuestion)
            {
                var pos = _gameManager.CorrectAnswersTextual.IndexOf(_gameManager.CorrectAnswersTextual.FirstOrDefault(x => x.description == character.Player.GetLastTextualAnswer()));

                if (pos > -1)
                    return true;
            }
            else
            {
                if (character.Player.GetLastNumericAnswer() == Convert.ToSingle(_gameManager.CorrectAnswerNumeric[0].description))
                    return true;
            }

            return false;
        }
    }
}