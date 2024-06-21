using DeathMatch;
using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionCreation : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private Transform categorySelectionPanel;
    [SerializeField] private Transform newQuestionPanel;

    [Header("Category Selection")]
    [SerializeField] private CategoryItem categoryItemPrefab;
    [SerializeField] private Transform categoriesHolder;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeCategoriesButton;

    [Header("New Question")]
    [SerializeField] private TMP_InputField questionTitle;
    [SerializeField] private TextMeshProUGUI questionCharacterLimitText;
    [SerializeField] private TextMeshProUGUI selectedCategoryText;
    [SerializeField] private TextMeshProUGUI totalQuestionsLeftText;
    [Space(2)]
    [SerializeField] private TMP_InputField firstAnswerInputField;
    [SerializeField] private TextMeshProUGUI firstAnswerCharacterLimitText;
    [Space(2)]
    [SerializeField] private TMP_InputField secondAnswerInputField;
    [SerializeField] private TextMeshProUGUI secondAnswerCharacterLimitText;
    [Space(2)]
    [SerializeField] private TMP_InputField thirdAnswerInputField;
    [SerializeField] private TextMeshProUGUI thirdAnswerCharacterLimitText;
    [Space(2)]
    [SerializeField] private TMP_InputField fourthAnswerInputField;
    [SerializeField] private TextMeshProUGUI fourthAnswerCharacterLimitText;
    [Space(2)]
    [SerializeField] private TMP_InputField fifthAnswerInputField;
    [SerializeField] private TextMeshProUGUI fifthAnswerCharacterLimitText;
    [Space(2)]
    [SerializeField] private Button submitQuestionButton;
    [SerializeField] private Button backToCategorySelectionButton;
    private bool isNewQuestionPanelOpen;

    [Header("New Question Hint")]
    [SerializeField] private GameObject questionEventHint;
    [SerializeField] private Button hintUnderstandButton;
    private const string QE_HINT_PREF = "QUESTION_EVENT_HINT";

    [Header("Success Message")]
    [SerializeField] private GameObject successPanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button okButton;

    private bool questionSubmitted;

    private List<CategoryItem> instancedCategories = new List<CategoryItem>();
    private EventDetails eventDetails;
    private ESDM.EventCategory selectedCategory;
    private MainUI mainUI;

    private void Start()
    {
        eventDetails = FindObjectOfType<EventDetails>();
        mainUI = FindObjectOfType<MainUI>();

        CloseCategorySelectionPanel();
        CheckSubmitQuestionButtonInteraction();
        CloseSuccessPanel(false);

        closeCategoriesButton.onClick.AddListener(eventDetails.OpenDetailsPanel);
        closeCategoriesButton.onClick.AddListener(CloseCategorySelectionPanel);
        closeCategoriesButton.onClick.AddListener(() =>
        {
            GameAnalytics.NewDesignEvent($"EVENTID_{eventDetails.CurrentEventInfo.id}_CATEGORY_SELECTION_CLOSED");
        });
        okButton.onClick.AddListener(() => CloseSuccessPanel(true));

        questionTitle.onValueChanged.AddListener((text) =>
        {
            questionCharacterLimitText.text = $"{text.Length}/{questionTitle.characterLimit}";
            CheckSubmitQuestionButtonInteraction();
        });

        firstAnswerInputField.onValueChanged.AddListener((text) =>
        {
            firstAnswerCharacterLimitText.text = $"{text.Length}/{firstAnswerInputField.characterLimit}";
            CheckSubmitQuestionButtonInteraction();
        });

        secondAnswerInputField.onValueChanged.AddListener((text) =>
        {
            secondAnswerCharacterLimitText.text = $"{text.Length}/{secondAnswerInputField.characterLimit}";
            CheckSubmitQuestionButtonInteraction();
        });

        thirdAnswerInputField.onValueChanged.AddListener((text) =>
        {
            thirdAnswerCharacterLimitText.text = $"{text.Length}/{thirdAnswerInputField.characterLimit}";
            CheckSubmitQuestionButtonInteraction();
        });

        fourthAnswerInputField.onValueChanged.AddListener((text) =>
        {
            fourthAnswerCharacterLimitText.text = $"{text.Length}/{fourthAnswerInputField.characterLimit}";
            CheckSubmitQuestionButtonInteraction();
        });

        fifthAnswerInputField.onValueChanged.AddListener((text) =>
        {
            fifthAnswerCharacterLimitText.text = $"{text.Length}/{fifthAnswerInputField.characterLimit}";
            CheckSubmitQuestionButtonInteraction();
        });

        backToCategorySelectionButton.onClick.AddListener(CloseNewQuestionPanel);
        backToCategorySelectionButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        submitQuestionButton.onClick.AddListener(SubmitNewQuestion);
        submitQuestionButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        submitButton.onClick.AddListener(OpenNewQuestionPanel);
        submitButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);

        hintUnderstandButton.onClick.AddListener(() =>
        {
            questionEventHint.SetActive(false);
            SaveManager.Set(QE_HINT_PREF, true);
        });
        hintUnderstandButton.onClick.AddListener(AudioManager.Instance.ClickButtonSFX);
    }

    private void Update()
    {
        submitButton.interactable = selectedCategory != null;
    }

    public void OpenCategorySelectionPanel()
    {
        categorySelectionPanel.gameObject.SetActive(true);
    }

    public void CloseCategorySelectionPanel()
    {
        categorySelectionPanel.gameObject.SetActive(false);
        ResetQuestionFields();
    }

    public void OpenNewQuestionPanel()
    {
        questionSubmitted = false;
        newQuestionPanel.gameObject.SetActive(true);

        selectedCategoryText.text = selectedCategory.category;
        totalQuestionsLeftText.text = eventDetails.CurrentEventInfo.limitQuestion.ToString();

        questionEventHint.SetActive(!SaveManager.HasKey(QE_HINT_PREF));
        isNewQuestionPanelOpen = true;
    }

    public void CloseNewQuestionPanel()
    {
        if (!isNewQuestionPanelOpen)
            return;

        newQuestionPanel.gameObject.SetActive(false);

        if (instancedCategories.Count > 1)
            OpenCategorySelectionPanel();
        else
        {
            CloseCategorySelectionPanel();
            eventDetails?.OpenDetailsPanel();
        }

        if (eventDetails)
            GameAnalytics.NewDesignEvent($"EVENTID_{eventDetails.CurrentEventInfo.id}_NEW_QUESTION_CLOSED");
    }

    public void StartEvent()
    {
        RemoveInstancedCategories();

        for (int i = 0; i < eventDetails.CurrentEventCategories.Count; i++)
        {
            var instance = Instantiate(categoryItemPrefab, categoriesHolder);
            instance.Setup(eventDetails.CurrentEventCategories[i]);

            instancedCategories.Add(instance);
        }

        if (instancedCategories.Count > 1)
            OpenCategorySelectionPanel();
        else
        {
            selectedCategory = eventDetails.CurrentEventCategories[0];
            OpenNewQuestionPanel();
        }
    }

    private void RemoveInstancedCategories()
    {
        foreach (Transform obj in categoriesHolder)
            Destroy(obj.gameObject);

        instancedCategories.Clear();
    }

    public void SelectCategory(CategoryItem newSelectedItem, ESDM.EventCategory category)
    {
        selectedCategory = category;

        foreach (var item in instancedCategories)
        {
            if (item == newSelectedItem)
                item.SetUIAsSelected();
            else
                item.SetUIAsUnSelected();
        }

        GameAnalytics.NewDesignEvent($"EVENTID_{eventDetails.CurrentEventInfo.id}_CATEGORYID_{selectedCategory.id}_SELECTED");
    }

    private void CheckSubmitQuestionButtonInteraction()
    {
        var hasAnyAnswerAdded = !string.IsNullOrEmpty(firstAnswerInputField.text)
           || !string.IsNullOrEmpty(secondAnswerInputField.text)
           || !string.IsNullOrEmpty(thirdAnswerInputField.text)
           || !string.IsNullOrEmpty(fourthAnswerInputField.text)
           || !string.IsNullOrEmpty(fifthAnswerInputField.text);

        submitQuestionButton.interactable = !string.IsNullOrEmpty(questionTitle.text) && hasAnyAnswerAdded;
    }

    public void SubmitNewQuestion()
    {
        var answers = new List<ESDM.EventAnswer>();

        if (!string.IsNullOrEmpty(firstAnswerInputField.text))
            answers.Add(new ESDM.EventAnswer(firstAnswerInputField.text));

        if (!string.IsNullOrEmpty(secondAnswerInputField.text))
            answers.Add(new ESDM.EventAnswer(secondAnswerInputField.text));

        if (!string.IsNullOrEmpty(thirdAnswerInputField.text))
            answers.Add(new ESDM.EventAnswer(thirdAnswerInputField.text));

        if (!string.IsNullOrEmpty(fourthAnswerInputField.text))
            answers.Add(new ESDM.EventAnswer(fourthAnswerInputField.text));

        if (!string.IsNullOrEmpty(fifthAnswerInputField.text))
            answers.Add(new ESDM.EventAnswer(fifthAnswerInputField.text));

        if (eventDetails.CurrentEventInfo.limitQuestion == 0)
        {
            OpenMessagePanel("تمام سوالات خود را طرح کرده اید!");
            return;
        }

        var newQuestionPacket = new ESDM.NewQuestionEventElement(questionTitle.text, new List<ESDM.EventCategory>() { selectedCategory }, answers, eventDetails.CurrentEventInfo.id);

        ServerConnection.Instance.SendQuestionForEvent(newQuestionPacket, (callback) =>
        {
            questionSubmitted = true;
            OpenMessagePanel("سوال شما ثبت شد!\nباید صبر کنید تا سوال شما توسط تیم رقابت مرگ تایید بشه");
        }, (failMessage) =>
        {
            OpenMessagePanel(failMessage);
        });

        GameAnalytics.NewDesignEvent($"EVENTID_{eventDetails.CurrentEventInfo.id}_QUESTION_SUBMITTED");
    }

    private void ResetQuestionFields()
    {
        questionTitle.text = string.Empty;

        firstAnswerInputField.text = string.Empty;
        secondAnswerInputField.text = string.Empty;
        thirdAnswerInputField.text = string.Empty;
        fourthAnswerInputField.text = string.Empty;
        firstAnswerInputField.text = string.Empty;
    }

    private void OpenMessagePanel(string message)
    {
        messageText.text = message;
        successPanel.SetActive(true);
    }

    private void CloseSuccessPanel(bool callActions)
    {
        successPanel.SetActive(false);

        if (callActions && questionSubmitted)
        {
            CloseNewQuestionPanel();
            CloseCategorySelectionPanel();
            eventDetails.OpenDetailsPanel();
            ResetQuestionFields();
        }
    }
}