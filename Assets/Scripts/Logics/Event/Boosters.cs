using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using GameAnalyticsSDK;
using System.Collections;
using GSDM;
using System.Runtime.InteropServices;

namespace DeathMatch
{
    public class Boosters : MonoBehaviour
    {
        public const int HELPS_COUNT = 3;

        [Space(2)]
        [Header("UI")]
        [SerializeField] private RectTransform boosters;
        [SerializeField] private float hidePosition;
        [SerializeField] private float showPosition;
        [SerializeField] private float transitionTime = 1;

        [Space(2)]
        [Header("Help UI")]
        [SerializeField] private GameObject firstHelpBox;
        [SerializeField] private TextMeshProUGUI firstHelpText;
        [SerializeField] private GameObject helpTextBox;
        [SerializeField] private TextMeshProUGUI helpText;
        [SerializeField] private Sprite[] helpStages;
        [SerializeField] private Button helpButton;
        [SerializeField] private Image helpImage;
        [SerializeField] private TextMeshProUGUI helpPriceText;

        private HelpItem[] helps;
        private int currentHelpNumber;

        private EventManager _gameManager;
        private TutorialSteps _tutorial;
        private MatchActiveBooster _matchActiveBooster;

        [System.Serializable]
        public struct HelpItem
        {
            public string HelpText;
            public int Price;

            public HelpItem(string helpText, int price)
            {
                HelpText = helpText;
                Price = price;
            }
        }

        private void Start()
        {
            _tutorial = FindObjectOfType<TutorialSteps>();
            _gameManager = FindObjectOfType<EventManager>();
            _matchActiveBooster = FindObjectOfType<MatchActiveBooster>();

            helpButton.onClick.AddListener(() => Help(true, true));

            HideBoosters(true);
            SetFirstHelpText(null);
            SetSecondHelpText(null);
        }

        private void Update()
        {
            helpButton.targetGraphic.color = IsHelpBoosterAvailable() ? helpButton.colors.normalColor : helpButton.colors.disabledColor;
        }

        public void SetHelps(Question question)
        {
            SetSecondHelpText(null);
            currentHelpNumber = 0;

            /// Fix for fking lazy backend :)
            if (string.IsNullOrEmpty(question.guide1) || string.IsNullOrEmpty(question.guide2) || string.IsNullOrEmpty(question.guide3))
            {
                ServerConnection.Instance.ReportQuestion(question.id, false, "CLIENT AUTO REPORT", 4);
                helps = null;
                return;
            }

            helps = new HelpItem[]
            {
                new HelpItem(question.guide1, GameManager.Instance.LoadedQuestions.guide1Cost),
                new HelpItem(question.guide2, GameManager.Instance.LoadedQuestions.guide2Cost),
                new HelpItem(question.guide3, GameManager.Instance.LoadedQuestions.guide3Cost)
            };

            UpdateHelpUI();
        }

        private void UpdateHelpUI()
        {
            if (currentHelpNumber >= HELPS_COUNT)
                return;

            helpPriceText.text = helps[currentHelpNumber].Price.ToString();
            helpImage.sprite = helpStages[currentHelpNumber];
        }

        public void Help(bool useCoin, bool playSFX)
        {
            if (currentHelpNumber >= HELPS_COUNT)
                return;

            if (!_tutorial.TutorialMode)
            {
                var currentCost = helps[currentHelpNumber].Price;

                if (IsHelpBoosterAvailable())
                {
                    if (useCoin)
                        WealthManager.Instance.UseCoins(currentCost, true);

                    GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, "Coin", currentCost, "HintBooster", "HintBooster");
                }
            }

            if (!IsHelpBoosterAvailable())
            {
                AudioManager.Instance.ErrorSFX();
                return;
            }

            if (currentHelpNumber == 0)
            {
                if (_matchActiveBooster.HasFirstHelp())
                    SetFirstHelpText($"راهنمای اول: {helps[currentHelpNumber].HelpText}");
            }
            else if (currentHelpNumber == 1)
            {
                if (_matchActiveBooster.HasSecondHelp())
                    SetSecondHelpText($"راهنمای دوم: {helps[currentHelpNumber].HelpText}");
            }
            else
            {
                SetFirstHelpText(null);
                SetSecondHelpText(helps[currentHelpNumber].HelpText);
            }

            if (!_matchActiveBooster.HasFirstHelp() && !_matchActiveBooster.HasSecondHelp())
            {
                SetFirstHelpText(null);
                SetSecondHelpText(helps[currentHelpNumber].HelpText);
            }

            _gameManager.HelpBooster(_gameManager.Player);

            if (playSFX)
                AudioManager.Instance.BoosterSFX();

            currentHelpNumber++;
            UpdateHelpUI();
        }

        private bool IsHelpBoosterAvailable()
        {
            if (!_gameManager.Player)
                return false;

            if (helps == null)
                return false;

            if (currentHelpNumber >= HELPS_COUNT)
                return false;

            if (_tutorial.TutorialMode)
                return true;

            if (!WealthManager.Instance.IsEnoughCoin(helps[currentHelpNumber].Price))
                return false;

            return true;
        }

        private void SetFirstHelpText(string text)
        {
            firstHelpText.text = text;
            firstHelpBox.gameObject.SetActive(text != null);
        }

        private void SetSecondHelpText(string text)
        {
            helpText.text = text;
            helpTextBox.gameObject.SetActive(text != null);
        }

        public void HideBoosters(bool force = false)
        {
            helpButton.interactable = false;
            SetFirstHelpText(null);
            SetSecondHelpText(null);

            boosters.DOKill();
            boosters.DOAnchorPosY(hidePosition, force ? 0 : transitionTime);
        }

        public void ShowBoosters(float delay)
        {
            StartCoroutine(ShowBoostersCoroutine(delay));
        }

        private IEnumerator ShowBoostersCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            helpButton.interactable = true;

            boosters.DOKill();
            boosters.DOAnchorPosY(showPosition, transitionTime).OnComplete(() => _gameManager.TUT_ShowBoostersStep());
        }
    }
}