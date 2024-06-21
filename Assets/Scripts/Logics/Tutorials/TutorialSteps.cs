using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DeathMatch;
using GameAnalyticsSDK;

public class TutorialSteps : MonoBehaviour
{
    public Step[] steps;
    public bool startTutorialAutomatically;
    public bool endTutorialOnFinishSteps;

    public int CurrentStep { get; private set; }

    public const string SAVE_PREFS = "GAME_TUTORIAL_DONE";

    public bool TutorialMode { get; private set; }

    public bool TutorialEnabledFromStart { get; private set; }


    private static List<ScriptedBot> SCRIPTED_BOTS = new List<ScriptedBot>()
    {
        new ScriptedBot("بت من", new List<bool>{true,true,false ,false,false,true,false,true, false,true }),
        new ScriptedBot("کامبیز", new List<bool>{false, false ,false }),
        new ScriptedBot("رجب", new List<bool>{false, true,true,false,false,false,true,true,true,true })
    };

    public class ScriptedBot
    {
        public string Name;
        public List<bool> Answers;

        public ScriptedBot(string name, List<bool> answers)
        {
            Name = name;
            Answers = answers;
        }
    }

    public static ScriptedBot GetBot
    {
        get
        {
            if (SCRIPTED_BOTS.Count == 0)
                return null;

            var bot = SCRIPTED_BOTS[0];
            SCRIPTED_BOTS.RemoveAt(0);
            return bot;
        }
    }

    [System.Serializable]
    public class Step
    {
        public string title;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToGetOnTop;
        public Button[] toNextStepButtons;

        public UnityEvent onLoadStep;
        public UnityEvent onPassStep;

        internal int[] savedObjectsToGetOnTopInitIndex;
    }

    private void Awake()
    {
        if (SaveManager.HasKey(SAVE_PREFS))
        {
            TutorialMode = false;
            DisableTutorial();
        }
        else
        {
            TutorialMode = true;
            TutorialEnabledFromStart = true;

            if (startTutorialAutomatically)
                StartTutorial();
        }

        InitTopObjects();
    }

    private void InitTopObjects()
    {
        foreach (var step in steps)
        {
            step.savedObjectsToGetOnTopInitIndex = new int[step.objectsToGetOnTop.Length];

            for (int i = 0; i < step.objectsToGetOnTop.Length; i++)
            {
                step.savedObjectsToGetOnTopInitIndex[i] = step.objectsToGetOnTop[i].transform.GetSiblingIndex();
            }
        }
    }

    public void NextStep()
    {
        steps[CurrentStep].onPassStep?.Invoke();

        if (CurrentStep == steps.Length - 1)
        {
            EndTutorial();
            return;
        }

        CurrentStep++;
        LoadStep(CurrentStep);
    }

    public void EndTutorial()
    {
        Time.timeScale = 1f;

        if (endTutorialOnFinishSteps)
            SaveManager.Set(SAVE_PREFS, 1);

        DisableTutorial();
    }

    public void StartTutorial()
    {
        LoadStep(0);
        GameAnalytics.NewDesignEvent("Start_Tutorial", 1f);
    }

    public static bool IsTutorialPlaying()
    {
        return SaveManager.HasKey(SAVE_PREFS);
    }

    private void LoadStep(int index)
    {
        var itemsOnTop = new List<string>();

        for (int i = 0; i < steps.Length; i++)
        {
            if (i > CurrentStep)
                continue;

            var step = steps[i];

            foreach (var objs in step.objectsToActivate)
                objs.gameObject.SetActive(i == index ? true : false);

            if (i == index)
            {
                foreach (var button in steps[i].toNextStepButtons)
                    button.onClick.AddListener(NextStep);

                for (int j = 0; j < step.objectsToGetOnTop.Length; j++)
                {
                    step.objectsToGetOnTop[j].transform.SetAsLastSibling();
                    itemsOnTop.Add(step.objectsToGetOnTop[j].name);
                }

                // To execute 'onLoadStep'
                Time.timeScale = 1f;
                step.onLoadStep?.Invoke();
            }
            else
            {
                foreach (var button in steps[i].toNextStepButtons)
                    button.onClick.RemoveListener(NextStep);

                for (int j = 0; j < step.objectsToGetOnTop.Length; j++)
                {
                    if (!itemsOnTop.Contains(step.objectsToGetOnTop[j].name))
                        step.objectsToGetOnTop[j].transform.SetSiblingIndex(step.savedObjectsToGetOnTopInitIndex[j]);
                }
            }
        }
    }

    private void DisableTutorial()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            var step = steps[i];

            foreach (var objs in step.objectsToActivate)
                objs.gameObject.SetActive(false);
        }
    }

    #region GameAnalytics
    public void ClickOnKeyboard_GA()
    {
        GameAnalytics.NewDesignEvent("FirstClickOnKeyboard_Tutorial", 1f);
    }

    public void Understand1_GA()
    {
        GameAnalytics.NewDesignEvent("Understand1_Tutorial", 1f);
    }

    public void Understand2_GA()
    {
        GameAnalytics.NewDesignEvent("Understand2_Tutorial", 1f);
    }

    public void UseHelpBooster_GA()
    {
        GameAnalytics.NewDesignEvent("HelpBTN_Tutorial", 1f);
    }

    public void UseSkipBooster_GA()
    {
        GameAnalytics.NewDesignEvent("SkipBTN_Tutorial", 1f);
    }
    #endregion
}