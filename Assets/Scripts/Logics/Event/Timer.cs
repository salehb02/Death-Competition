using System;
using UnityEngine;
using UnityEngine.Events;

namespace DeathMatch
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private float tutorialTime = 60f;

        [Header("Sounds")]
        public AudioSource _slowTicTac;
        public AudioSource _fastTicTac;
        public AudioSource _normalTicTac;

        public event UnityAction OnTimeOut;

        private float timer = 15f;
        private float currentTimer = 0;
        private bool countDown = false;

        private DateTime endRoundTime;

        private EventManagerPresentor gameplayUIManager;
        private TutorialSteps tutorial;
        private LoadQuestions loadQuestions;

        private void Start()
        {
            tutorial = FindObjectOfType<TutorialSteps>();
            gameplayUIManager = FindObjectOfType<EventManagerPresentor>();
            loadQuestions = FindObjectOfType<LoadQuestions>();

            loadQuestions.OnQuestionsLoaded += LoadTime;
        }

        private void OnDisable()
        {
            loadQuestions.OnQuestionsLoaded -= LoadTime;
        }

        private void Update()
        {
            if (!countDown)
                return;

            currentTimer = GetTimeLeft().Seconds;
            gameplayUIManager.SetTimerText(Convert.ToInt32(currentTimer).ToString("D2"));

            if (currentTimer > 10)
                PlaySlowTimerSFX();
            else if (currentTimer <= 10 && currentTimer > 5)
                PlayNormalTimerSFX();
            else if (currentTimer <= 5)
                PlayFastTimerSFX();

            if (currentTimer <= 0)
                StopTimer();
        }

        private void LoadTime()
        {
            timer = GameManager.Instance.LoadedQuestions.responseTime;
            gameplayUIManager.SetTimerText((Convert.ToInt32(tutorial.TutorialMode ? tutorialTime : timer)).ToString("D2"));
        }

        public void StartTimer()
        {
            if (!tutorial.TutorialMode)
                endRoundTime = DateTime.Now.AddSeconds(timer);
            else
                endRoundTime = DateTime.Now.AddSeconds(tutorialTime);

            countDown = true;
        }

        public void StopTimer()
        {
            countDown = false;
            currentTimer = 0;
            StopAllTimerSFX();
            AudioManager.Instance.TimeOverSFX();
            OnTimeOut?.Invoke();
        }

        private TimeSpan GetTimeLeft()
        {
            return endRoundTime - DateTime.Now;
        }

        public void EndGame()
        {
            countDown = false;
        }

        private void PlaySlowTimerSFX()
        {
            if (_slowTicTac.isPlaying)
                return;

            _slowTicTac.Play();
            _normalTicTac.Stop();
            _fastTicTac.Stop();
        }

        private void PlayNormalTimerSFX()
        {
            if (_normalTicTac.isPlaying)
                return;

            _slowTicTac.Stop();
            _normalTicTac.Play();
            _fastTicTac.Stop();
        }

        private void PlayFastTimerSFX()
        {
            if (_fastTicTac.isPlaying)
                return;

            _slowTicTac.Stop();
            _normalTicTac.Stop();
            _fastTicTac.Play();
        }

        private void StopAllTimerSFX()
        {
            _slowTicTac.Stop();
            _normalTicTac.Stop();
            _fastTicTac.Stop();
        }
    }
}