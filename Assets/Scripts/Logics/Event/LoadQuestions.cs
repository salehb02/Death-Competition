using System;
using UnityEngine;

namespace DeathMatch
{
    public class LoadQuestions : MonoBehaviour
    {
        private TutorialSteps tutorialSteps;

        public bool IsLoaded { get; private set; }

        public event Action OnQuestionsLoaded;

        [System.Obsolete]
        private void Start()
        {
            tutorialSteps = FindObjectOfType<TutorialSteps>();

            if (tutorialSteps.TutorialMode)
                return;

            // Request server to get question
            ServerConnection.Instance.GetQuestions((questions) =>
            {
                // Save recieved questions
                GameManager.Instance.SetQuestions(questions);
                IsLoaded = true;
                OnQuestionsLoaded?.Invoke();
            });
        }
    }
}